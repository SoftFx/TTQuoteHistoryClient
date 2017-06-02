using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SoftFX.Net.Core;
using SoftFX.Net.QuoteHistory;
using ClientSession = SoftFX.Net.QuoteHistory.ClientSession;
using ClientSessionOptions = SoftFX.Net.QuoteHistory.ClientSessionOptions;

namespace TTQuoteHistoryClient
{
    public class QuoteHistoryClient : IDisposable
    {
        public static TimeSpan Timeout = TimeSpan.FromMilliseconds(10000);

        private readonly ClientSession _session;
        private readonly ClientSessionListener _sessionListener;

        #region Async contexts

        private interface IAsyncContext
        {
            void SetException(Exception ex);
        }

        private class LoginAsyncContext : LoginRequestClientContext, IAsyncContext
        {
            public LoginAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<object> Tcs = new TaskCompletionSource<object>();
        }

        private class LogoutAsyncContext : LogoutRequestClientContext, IAsyncContext
        {
            public LogoutAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<LogoutInfo> Tcs = new TaskCompletionSource<LogoutInfo>();
        }

        private class GetSupportedSymbolsAsyncContext : SymbolsRequestClientContext, IAsyncContext
        {
            public GetSupportedSymbolsAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<List<string>> Tcs = new TaskCompletionSource<List<string>>();
        }

        private class GetSupportedPeriodicitiesAsyncContext : PeriodicitiesRequestClientContext, IAsyncContext
        {
            public GetSupportedPeriodicitiesAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<List<string>> Tcs = new TaskCompletionSource<List<string>>();
        }

        private class QueryQuoteHistoryBarsAsyncContext : BarsRequestClientContext, IAsyncContext
        {
            public QueryQuoteHistoryBarsAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<List<Bar>> Tcs = new TaskCompletionSource<List<Bar>>();
        }

        private class QueryQuoteHistoryTicksAsyncContext : TicksRequestClientContext, IAsyncContext
        {
            public QueryQuoteHistoryTicksAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<List<Tick>> Tcs = new TaskCompletionSource<List<Tick>>();
        }

        #endregion

        #region Constructors

        public QuoteHistoryClient(string name) : this(name, 5020)
        {
        }

        public QuoteHistoryClient(string name, int port) : this(name, port, new ClientSessionOptions(port) { ConnectMaxCount = 1, SendBufferSize = 1048576 })
        {
        }

        public QuoteHistoryClient(string name, int port, ClientSessionOptions options)
        {
            options.ConnectPort = port;
            options.ConnectionType = ConnectionType.Secure;
            options.ServerCertificateName = "TickTraderManagerService";
            options.Log.Events = false;
            options.Log.States = false;
            options.Log.Messages = false;
            _session = new ClientSession(name, options);
            _sessionListener = new ClientSessionListener(this);
            _session.Listener = _sessionListener;
        }

        #endregion

        #region Session listener

        private class ClientSessionListener : SoftFX.Net.QuoteHistory.ClientSessionListener
        {
            public ClientSessionListener(QuoteHistoryClient client)
            {
                _client = client;
            }

            public override void OnConnect(ClientSession clientSession)
            {
                _client.IsConnected = true;
                _client.Connected?.Invoke(_client);
            }

            public override void OnConnectError(ClientSession clientSession)
            {
                _client.IsConnected = false;
                _client.ConnectError?.Invoke(_client);
            }

            public override void OnDisconnect(ClientSession clientSession, ClientContext[] contexts, string text)
            {
                Exception exception = new Exception("Client disconnected : " + text);
                foreach (ClientContext context in contexts)
                    ((IAsyncContext) context).SetException(exception);

                _client.IsConnected = false;
                _client.Disconnected?.Invoke(_client);
            }

            public override void OnLoginReport(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReport message)
            {
                var context = (LoginAsyncContext) LoginRequestClientContext;
                context.Tcs.SetResult(null);
            }

            public override void OnLoginReject(ClientSession session, LoginRequestClientContext LoginRequestClientContext, LoginReject message)
            {
                var context = (LoginAsyncContext)LoginRequestClientContext;
                var exception = new Exception(message.Message);
                context.Tcs.SetException(exception);
            }

            public override void OnLogoutReport(ClientSession session, LogoutRequestClientContext LogoutRequestClientContext, LogoutReport message)
            {
                var context = (LogoutAsyncContext)LogoutRequestClientContext;
                var result = new LogoutInfo();
                result.Reason = (LogoutReason) message.Reason; // values match
                result.Message = message.Message;
                context.Tcs.SetResult(result);
            }

            public override void OnSymbolsReport(ClientSession session, SymbolsRequestClientContext SymbolsRequestClientContext, SymbolsReport report)
            {
                var context = (GetSupportedSymbolsAsyncContext) SymbolsRequestClientContext;
                var result = new List<string>();
                for (int i = 0; i < report.Symbols.Length; i++)
                {
                    var symbol = report.Symbols[i];
                    result.Add(symbol);
                }
                context.Tcs.SetResult(result);
            }

            public override void OnSymbolsReject(ClientSession session, SymbolsRequestClientContext SymbolsRequestClientContext, QueryReject reject)
            {
                var context = (GetSupportedSymbolsAsyncContext) SymbolsRequestClientContext;
                var exception = new Exception(reject.Message);
                context.Tcs.SetException(exception);
            }

            public override void OnPeriodicitiesReport(ClientSession session, PeriodicitiesRequestClientContext PeriodicitiesRequestClientContext, PeriodicitiesReport report)
            {
                var context = (GetSupportedPeriodicitiesAsyncContext) PeriodicitiesRequestClientContext;
                var result = new List<string>();
                for (int i = 0; i < report.Periodicities.Length; i++)
                {
                    var periodicity = report.Periodicities[i];
                    result.Add(periodicity);
                }
                context.Tcs.SetResult(result);
            }

            public override void OnPeriodicitiesReject(ClientSession session, PeriodicitiesRequestClientContext PeriodicitiesRequestClientContext, QueryReject reject)
            {
                var context = (GetSupportedPeriodicitiesAsyncContext) PeriodicitiesRequestClientContext;
                var exception = new Exception(reject.Message);
                context.Tcs.SetException(exception);
            }

            public override void OnBarsReport(ClientSession session, BarsRequestClientContext BarsRequestClientContext, QueryBarsReport report)
            {
                var context = (QueryQuoteHistoryBarsAsyncContext) BarsRequestClientContext;
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
                context.Tcs.SetResult(result);
            }

            public override void OnBarsReject(ClientSession session, BarsRequestClientContext BarsRequestClientContext, QueryReject reject)
            {
                var context = (QueryQuoteHistoryBarsAsyncContext) BarsRequestClientContext;
                var exception = new Exception(reject.Message);
                context.Tcs.SetException(exception);
            }

            public override void OnTicksReport(ClientSession session, TicksRequestClientContext TicksRequestClientContext, QueryTicksReport report)
            {
                var context = (QueryQuoteHistoryTicksAsyncContext) TicksRequestClientContext;
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
                context.Tcs.SetResult(result);
            }

            public override void OnTicksReject(ClientSession session, TicksRequestClientContext TicksRequestClientContext, QueryReject reject)
            {
                var context = (QueryQuoteHistoryTicksAsyncContext) TicksRequestClientContext;
                var exception = new Exception(reject.Message);
                context.Tcs.SetException(exception);
            }

            QuoteHistoryClient _client;
        }

        #endregion

        #region Connection

        public delegate void ConnectedDelegate(QuoteHistoryClient client);
        public delegate void ConnectErrorDelegate(QuoteHistoryClient client);
        public delegate void DisconnectedDelegate(QuoteHistoryClient client);

        public event ConnectedDelegate Connected;
        public event ConnectErrorDelegate ConnectError;
        public event ConnectedDelegate Disconnected;

        public bool IsConnected { get; private set; }

        public void Connect(string address)
        {
            _session.Connect(address);

            if (!_session.WaitConnect(15000))
            {
                Disconnect();
                throw new TimeoutException("Connect timeout");
            }
        }

        public void ConnectAsync(string address)
        {
            _session.Connect(address);
        }

        public void Disconnect()
        {
            DisconnectAsync();
            Join();
        }

        public void DisconnectAsync()
        {
            _session.Disconnect("Disconnect client");
        }

        public void Join()
        {
            _session.Join();

            _session.Listener = null;
        }

        #endregion

        #region Login / logout

        public void Login(string username, string password, string deviceId, string appSessionId)
        {
            ConvertToSync(LoginAsync(username, password, deviceId, appSessionId), Timeout);
        }

        public Task LoginAsync(string username, string password, string deviceId, string appSessionId)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new LoginAsyncContext();

            // Create a request
            var request = new LoginRequest(0)
            {
                Username = username,
                Password = password,
                DeviceID = deviceId,
                AppSessionID = appSessionId
            };

            // Send request to the server
            _session.SendLoginRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        public LogoutInfo Logout(string message)
        {
            return ConvertToSync(LogoutAsync(message), Timeout);
        }

        public Task<LogoutInfo> LogoutAsync(string message)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new LogoutAsyncContext();

            // Create a request
            var request = new LogoutRequest(0)
            {
                Message = message
            };

            // Send request to the server
            _session.SendLogoutRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        #endregion

        #region Quote History cache

        public List<string> GetSupportedSymbols()
        {
            return ConvertToSync(GetSupportedSymbolsAsync(), Timeout);
        }

        public Task<List<string>> GetSupportedSymbolsAsync()
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new GetSupportedSymbolsAsyncContext();

            // Create a new QH cache request
            var request = new SymbolsRequest(0)
            {
                RequestId = Guid.NewGuid().ToString()
            };

            // Send request to the server
            _session.SendSymbolsRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        public List<string> GetSupportedPeriodicities()
        {
            return ConvertToSync(GetSupportedPeriodicitiesAsync(), Timeout);
        }

        public Task<List<string>> GetSupportedPeriodicitiesAsync()
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new GetSupportedPeriodicitiesAsyncContext();

            // Create a new QH cache request
            var request = new PeriodicitiesRequest(0)
            {
                RequestId = Guid.NewGuid().ToString()
            };

            // Send request to the server
            _session.SendPeriodicitiesRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        public List<Bar> QueryQuoteHistoryBars(DateTime timestamp, int count, string symbol, string pereodicity, PriceType priceType)
        {
            return ConvertToSync(QueryQuoteHistoryBarsAsync(timestamp, count, symbol, pereodicity, priceType), Timeout);
        }

        public Task<List<Bar>> QueryQuoteHistoryBarsAsync(DateTime timestamp, int count, string symbol, string pereodicity, PriceType priceType)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new QueryQuoteHistoryBarsAsyncContext();

            // Create a new QH cache request
            var request = new QueryBarsRequest(0)
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = timestamp,
                Count = Math.Min(count, 5000),
                Symbol = symbol,
                Periodicity = pereodicity,
                PriceType = (SoftFX.Net.QuoteHistory.PriceType) priceType
            };

            // Send request to the server
            _session.SendBarsRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        public List<Tick> QueryQuoteHistoryTicks(DateTime timestamp, int count, string symbol, bool level2)
        {
            return ConvertToSync(QueryQuoteHistoryTicksAsync(timestamp, count, symbol, level2), Timeout);
        }

        public Task<List<Tick>> QueryQuoteHistoryTicksAsync(DateTime timestamp, int count, string symbol, bool level2)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new QueryQuoteHistoryTicksAsyncContext();

            // Create a new QH cache request
            var request = new QueryTicksRequest(0)
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = timestamp,
                Count = Math.Min(count, 1000),
                Symbol = symbol,
                Level2 = level2
            };

            // Send request to the server
            _session.SendTicksRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        #endregion

        #region Async helpers

        public static void ConvertToSync(Task task, TimeSpan timeout)
        {
            try
            {
                if (!task.Wait(Timeout))
                    throw new TimeoutException("Method call timeout");
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
            }
        }

        public static TResult ConvertToSync<TResult>(Task<TResult> task, TimeSpan timeout)
        {
            try
            {
                if (!task.Wait(Timeout))
                    throw new TimeoutException("Method call timeout");

                return task.Result;
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

        private bool _disposed;

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
            if (disposing & !_disposed)
            {
                _disposed = true;
                Disconnect();
            }
        }

        #endregion
    }
}
