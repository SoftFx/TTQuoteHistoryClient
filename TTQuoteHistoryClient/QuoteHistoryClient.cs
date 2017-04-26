﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SoftFX.Net.Core;
using SoftFX.Net.QuoteHistoryCacheProtocol;

namespace TTQuoteHistoryClient
{
    public class QuoteHistoryClient : IDisposable
    {
        public static TimeSpan Timeout = TimeSpan.FromMilliseconds(10000);

        private int _timeout = 15000;
        private readonly string _address;
        private readonly ClientSession _session;

        #region Async call tokens

        interface IAsyncCallToken
        {
            string RequestId { get; }
            void SetException(Exception ex);
        }

        class AsyncCallToken<T> : IAsyncCallToken
        {
            public string RequestId { get; set; }
            public readonly TaskCompletionSource<T> Tcs = new TaskCompletionSource<T>();

            public void SetException(Exception ex)
            {
                Tcs.SetException(ex);
            }
        }

        private readonly object _locker = new object();
        private readonly Dictionary<string, IAsyncCallToken> _callTokens = new Dictionary<string, IAsyncCallToken>();

        IAsyncCallToken TryGetAsyncCallToken(string requestId)
        {
            if (!string.IsNullOrEmpty(requestId))
            {
                lock (_locker)
                {
                    IAsyncCallToken token;
                    if (_callTokens.TryGetValue(requestId, out token))
                        return token;
                }
            }

            return null;
        }

        void RemoveAsyncCallToken(string requestId)
        {
            lock (_locker)
            {
                _callTokens.Remove(requestId);
            }
        }

        #endregion

            #region Constructors

        public QuoteHistoryClient(string address) : this(address, 5020)
        {
        }

        public QuoteHistoryClient(string address, int port) : this(address, port, new ClientSessionOptions(port) { ConnectMaxCount = 1 })
        {
        }

        public QuoteHistoryClient(string address, int port, ClientSessionOptions options)
        {
            _address = address;
            options.ConnectPort = port;
            _session = new ClientSession("QuoteHistoryClient", SoftFX.Net.QuoteHistoryCacheProtocol.Info.QuoteHistoryCacheProtocol, options);
            _session.OnConnect += OnConnect;
            _session.OnConnectError += OnConnectError;
            _session.OnDisconnect += OnDisconnect;
            _session.OnReceive += OnReceive;
        }

        private void OnConnect(ClientSession clientSession)
        {
            IsConnected = true;
        }

        private void OnConnectError(ClientSession clientSession)
        {
            IsConnected = false;
        }

        private void OnDisconnect(ClientSession clientSession, string text)
        {
            lock (_locker)
            {
                // Reset all async call tokens
                foreach (var token in _callTokens)
                    token.Value.SetException(new Exception("Client is disconnected"));
                _callTokens.Clear();
            }

            IsConnected = false;
        }

        private void OnReceive(ClientSession clientSession, Message message)
        {
            if (SoftFX.Net.QuoteHistoryCacheProtocol.Is.QuoteHistorySymbolsReport(message))
            {
                var report = SoftFX.Net.QuoteHistoryCacheProtocol.Cast.QuoteHistorySymbolsReport(message);
                var token = TryGetAsyncCallToken(report.RequestId) as AsyncCallToken<List<string>>;
                if (token != null)
                {
                    var result = new List<string>();
                    for (int i = 0; i < report.Symbols.Length; i++)
                    {
                        var symbol = report.Symbols[i];
                        result.Add(symbol);
                    }
                    token.Tcs.SetResult(result);
                    RemoveAsyncCallToken(token.RequestId);
                }
            }
            else if (SoftFX.Net.QuoteHistoryCacheProtocol.Is.QuoteHistoryPeriodicitiesReport(message))
            {
                var report = SoftFX.Net.QuoteHistoryCacheProtocol.Cast.QuoteHistoryPeriodicitiesReport(message);
                var token = TryGetAsyncCallToken(report.RequestId) as AsyncCallToken<List<string>>;
                if (token != null)
                {
                    var result = new List<string>();
                    for (int i = 0; i < report.Periodicities.Length; i++)
                    {
                        var periodicity = report.Periodicities[i];
                        result.Add(periodicity);
                    }
                    token.Tcs.SetResult(result);
                    RemoveAsyncCallToken(token.RequestId);
                }
            }
            else if (SoftFX.Net.QuoteHistoryCacheProtocol.Is.QueryQuoteHistoryBarsReport(message))
            {
                var report = SoftFX.Net.QuoteHistoryCacheProtocol.Cast.QueryQuoteHistoryBarsReport(message);
                var token = TryGetAsyncCallToken(report.RequestId) as AsyncCallToken<List<Bar>>;
                if (token != null)
                {
                    var result = new List<Bar>();
                    for (int i = 0; i < report.Bars.Length; i++)
                    {
                        var sourceBar = report.Bars[i];
                        var bar = new Bar
                        {
                            Time = sourceBar.Time,
                            Open = (decimal)sourceBar.Open,
                            High = (decimal)sourceBar.High,
                            Low = (decimal)sourceBar.Low,
                            Close = (decimal)sourceBar.Close,
                            Volume = (decimal)sourceBar.Volume
                        };
                        result.Add(bar);
                    }
                    token.Tcs.SetResult(result);
                    RemoveAsyncCallToken(token.RequestId);
                }
            }
            else if (SoftFX.Net.QuoteHistoryCacheProtocol.Is.QueryQuoteHistoryTicksReport(message))
            {
                var report = SoftFX.Net.QuoteHistoryCacheProtocol.Cast.QueryQuoteHistoryTicksReport(message);
                var token = TryGetAsyncCallToken(report.RequestId) as AsyncCallToken<List<Tick>>;
                if (token != null)
                {
                    var result = new List<Tick>();
                    for (int i = 0; i < report.Ticks.Length; i++)
                    {
                        var sourceTick = report.Ticks[i];
                        var tick = new Tick()
                        {
                            Id = new TickId
                            {
                                Time = sourceTick.Id.Time,
                                Index = sourceTick.Id.Index
                            },
                            Level2 = new Level2Collection()
                        };

                        for (int j = 0; j < sourceTick.Level2.Bids.Length; j++)
                        {
                            var sourceBid = sourceTick.Level2.Bids[j];
                            var bid = new Level2Value
                            {
                                Price = (decimal)sourceBid.Price,
                                Volume = (decimal)sourceBid.Volume
                            };
                            tick.Level2.Bids.Add(bid);
                        }

                        for (int j = 0; j < sourceTick.Level2.Asks.Length; j++)
                        {
                            var sourceAsk = sourceTick.Level2.Asks[j];
                            var ask = new Level2Value
                            {
                                Price = (decimal)sourceAsk.Price,
                                Volume = (decimal)sourceAsk.Volume
                            };
                            tick.Level2.Asks.Add(ask);
                        }

                        result.Add(tick);
                    }
                    token.Tcs.SetResult(result);
                    RemoveAsyncCallToken(token.RequestId);
                }
            }
            else if (SoftFX.Net.QuoteHistoryCacheProtocol.Is.QueryQuoteHistoryReject(message))
            {
                var reject = SoftFX.Net.QuoteHistoryCacheProtocol.Cast.QueryQuoteHistoryReject(message);
                var token = TryGetAsyncCallToken(reject.RequestId) as AsyncCallToken<string>;
                if (token != null)
                {
                    var result = reject.RejectMessage;
                    token.Tcs.SetResult(result);
                    RemoveAsyncCallToken(token.RequestId);
                }
            }
        }

        #endregion

        #region Connection

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            _session.Connect(_address);
            if (!_session.WaitConnect(_timeout))
            {
                Disconnect();
                throw new TimeoutException("Connect timeout");
            }
        }

        public void Disconnect()
        {
            _session.Disconnect("Disconnect client");
            _session.Join();
        }

        #endregion

        #region Quote History cache

        public List<string> GetSupportedSymbols() { return ConvertToSync(() => GetSupportedSymbolsAsync().Result); }

        public Task<List<string>> GetSupportedSymbolsAsync()
        {
            return GetSupportedSymbolsAsync(Timeout);
        }

        public Task<List<string>> GetSupportedSymbolsAsync(TimeSpan timeout)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new QH cache request
            var request = new QuoteHistorySymbolsRequest(0)
            {
                RequestId = Guid.NewGuid().ToString()
            };

            // Create a new async call token
            var token = new AsyncCallToken<List<string>> { RequestId = request.RequestId };
            lock (_locker)
            {
                _callTokens[token.RequestId] = token;
            }

            try
            {
                // Send request to the server
                _session.Send(SoftFX.Net.QuoteHistoryCacheProtocol.Cast.Message(request));
            }
            catch (Exception)
            {
                lock (_locker)
                {
                    _callTokens.Remove(token.RequestId);
                }
            }

            // Return result task
            return token.Tcs.Task;
        }

        public List<string> GetSupportedPeriodicities() { return ConvertToSync(() => GetSupportedPeriodicitiesAsync().Result); }

        public Task<List<string>> GetSupportedPeriodicitiesAsync()
        {
            return GetSupportedPeriodicitiesAsync(Timeout);
        }

        public Task<List<string>> GetSupportedPeriodicitiesAsync(TimeSpan timeout)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new QH cache request
            var request = new QuoteHistoryPeriodicitiesRequest(0)
            {
                RequestId = Guid.NewGuid().ToString()
            };

            // Create a new async call token
            var token = new AsyncCallToken<List<string>> { RequestId = request.RequestId };
            lock (_locker)
            {
                _callTokens[token.RequestId] = token;
            }

            try
            {
                // Send request to the server
                _session.Send(SoftFX.Net.QuoteHistoryCacheProtocol.Cast.Message(request));
            }
            catch (Exception)
            {
                lock (_locker)
                {
                    _callTokens.Remove(token.RequestId);
                }
            }

            // Return result task
            return token.Tcs.Task;
        }

        public List<Bar> QueryQuoteHistoryBars(DateTime timestamp, int count, string symbol, string pereodicity, PriceType priceType) { return ConvertToSync(() => QueryQuoteHistoryBarsAsync(timestamp, count, symbol, pereodicity, priceType).Result); }

        public Task<List<Bar>> QueryQuoteHistoryBarsAsync(DateTime timestamp, int count, string symbol, string pereodicity, PriceType priceType)
        {
            return QueryQuoteHistoryBarsAsync(timestamp, count, symbol, pereodicity, priceType, Timeout);
        }

        public Task<List<Bar>> QueryQuoteHistoryBarsAsync(DateTime timestamp, int count, string symbol, string pereodicity, PriceType priceType, TimeSpan timeout)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new QH cache request
            var request = new QueryQuoteHistoryBarsRequest(0)
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = timestamp,
                Count = count,
                Symbol = symbol,
                Periodicity = pereodicity,
                PriceType = (SoftFX.Net.QuoteHistoryCacheProtocol.PriceType) priceType
            };

            // Create a new async call token
            var token = new AsyncCallToken<List<Bar>> { RequestId = request.RequestId };
            lock (_locker)
            {
                _callTokens[token.RequestId] = token;
            }

            try
            {
                // Send request to the server
                _session.Send(SoftFX.Net.QuoteHistoryCacheProtocol.Cast.Message(request));
            }
            catch (Exception)
            {
                lock (_locker)
                {
                    _callTokens.Remove(token.RequestId);
                }
            }

            // Return result task
            return token.Tcs.Task;
        }

        public List<Tick> QueryQuoteHistoryTicks(DateTime timestamp, int count, string symbol, bool level2) { return ConvertToSync(() => QueryQuoteHistoryTicksAsync(timestamp, count, symbol, level2).Result); }

        public Task<List<Tick>> QueryQuoteHistoryTicksAsync(DateTime timestamp, int count, string symbol, bool level2)
        {
            return QueryQuoteHistoryTicksAsync(timestamp, count, symbol, level2, Timeout);
        }

        public Task<List<Tick>> QueryQuoteHistoryTicksAsync(DateTime timestamp, int count, string symbol, bool level2, TimeSpan timeout)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new QH cache request
            var request = new QueryQuoteHistoryTicksRequest(0)
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = timestamp,
                Count = count,
                Symbol = symbol,
                Level2 = level2
            };

            // Create a new async call token
            var token = new AsyncCallToken<List<Tick>> { RequestId = request.RequestId };
            lock (_locker)
            {
                _callTokens[token.RequestId] = token;
            }

            try
            {
                // Send request to the server
                _session.Send(SoftFX.Net.QuoteHistoryCacheProtocol.Cast.Message(request));
            }
            catch (Exception)
            {
                lock (_locker)
                {
                    _callTokens.Remove(token.RequestId);
                }
            }

            // Return result task
            return token.Tcs.Task;
        }

        #endregion

        #region Async helpers

        public static void ConvertToSync(Action method)
        {
            try
            {
                method();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
            }
        }

        public static TResult ConvertToSync<TResult>(Func<TResult> method)
        {
            try
            {
                return method();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
                // Unreacheble code...
                return default(TResult);
            }
        }

        #endregion

        #region IDisposable

        ~QuoteHistoryClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Disconnect();
        }

        #endregion
    }
}
