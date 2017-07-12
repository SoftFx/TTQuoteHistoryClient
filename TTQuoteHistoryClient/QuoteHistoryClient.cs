using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SoftFX.Net.Core;
using SoftFX.Net.QuoteHistory;
using TickTrader.BusinessObjects;
using TickTrader.Common.Business;
using TickTrader.Common.Time;
using TickTrader.Server.QuoteHistory.Serialization;
using ClientSession = SoftFX.Net.QuoteHistory.ClientSession;
using ClientSessionOptions = SoftFX.Net.QuoteHistory.ClientSessionOptions;

namespace TTQuoteHistoryClient
{
    public class QuoteHistoryClient : IDisposable
    {
        public static TimeSpan Timeout = TimeSpan.FromMilliseconds(30000);

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

        private class QueryQuoteHistoryBarsFilesAsyncContext : BarsFileRequestClientContext, IAsyncContext
        {
            public QueryQuoteHistoryBarsFilesAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<List<byte[]>> Tcs = new TaskCompletionSource<List<byte[]>>();
        }

        private class QueryQuoteHistoryTicksFilesAsyncContext : TicksFileRequestClientContext, IAsyncContext
        {
            public QueryQuoteHistoryTicksFilesAsyncContext() : base(false) { }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<List<byte[]>> Tcs = new TaskCompletionSource<List<byte[]>>();
        }

        private class FileAsyncContext : FileRequestClientContext, IAsyncContext
        {
            public List<string> FileIds { get; set; }
            public int FileIndex { get; set; }
            public int ChunkIndex { get; set; }

            public List<byte> Buffer { get; set; }
            public List<byte[]> Files { get; set; }

            public FileAsyncContext() : base(false)
            {
                FileIds = new List<string>();
                Buffer = new List<byte>();
                Files = new List<byte[]>();
            }

            public void SetException(Exception ex) { Tcs.SetException(ex); }

            public readonly TaskCompletionSource<byte[]> Tcs = new TaskCompletionSource<byte[]>();
        }

        private class BarsFileAsyncContext : FileAsyncContext
        {
            public QueryQuoteHistoryBarsFilesAsyncContext ParentContext { get; set; }

            public BarsFileAsyncContext(QueryQuoteHistoryBarsFilesAsyncContext parent) { ParentContext = parent; }
        }

        private class TicksFileAsyncContext : FileAsyncContext
        {
            public QueryQuoteHistoryTicksFilesAsyncContext ParentContext { get; set; }

            public TicksFileAsyncContext(QueryQuoteHistoryTicksFilesAsyncContext parent) { ParentContext = parent; }
        }

        #endregion

        #region Constructors

        public QuoteHistoryClient(string name) : this(name, 5020)
        {
        }

        public QuoteHistoryClient(string name, int port) : this(name, port, true)
        {
        }

        public QuoteHistoryClient(string name, int port, bool automaticReconnect) : this(name, new ClientSessionOptions(port) { ConnectMaxCount = 1, ReconnectMaxCount = automaticReconnect ? -1 : 0, SendBufferSize = 1048576 })
        {
        }

        private QuoteHistoryClient(string name, ClientSessionOptions options)
        {
            options.ConnectionType = ConnectionType.Secure;
            options.ServerCertificateName = "TickTraderManagerService";
#if DEBUG
            options.Log.Events = true;
            options.Log.States = false;
            options.Log.Messages = true;
#else
            options.Log.Events = false;
            options.Log.States = false;
            options.Log.Messages = false;
#endif
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
                string message = "Client disconnected";
                if (text != null)
                {
                    message += " : ";
                    message += text;
                }
                Exception exception = new Exception(message);

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

            public override void OnBarsFileReport(ClientSession session, BarsFileRequestClientContext BarsFileRequestClientContext, QueryBarsFileReport report)
            {
                var context = (QueryQuoteHistoryBarsFilesAsyncContext)BarsFileRequestClientContext;

                if (report.Files.Length > 0)
                {
                    var fileContext = new BarsFileAsyncContext(context);
                    for (int i = 0; i < report.Files.Length; i++)
                        fileContext.FileIds.Add(report.Files[i].FileId);

                    var request = new FileRequest(0);
                    request.FileId = fileContext.FileIds[0];
                    request.Chunk = 0;
                    session.SendFileRequest(fileContext, request);
                }
                else
                    context.Tcs.SetResult(new List<byte[]>());
            }

            public override void OnBarsFileReject(ClientSession session, BarsFileRequestClientContext BarsFileRequestClientContext, QueryReject reject)
            {
                var context = (QueryQuoteHistoryBarsFilesAsyncContext)BarsFileRequestClientContext;
                var exception = new Exception(reject.Message);
                context.Tcs.SetException(exception);
            }

            public override void OnTicksFileReport(ClientSession session, TicksFileRequestClientContext TicksFileRequestClientContext, QueryTicksFileReport report)
            {
                var context = (QueryQuoteHistoryTicksFilesAsyncContext)TicksFileRequestClientContext;

                if (report.Files.Length > 0)
                {
                    var fileContext = new TicksFileAsyncContext(context);
                    for (int i = 0; i < report.Files.Length; i++)
                        fileContext.FileIds.Add(report.Files[i].FileId);

                    var request = new FileRequest(0);
                    request.FileId = fileContext.FileIds[0];
                    request.Chunk = 0;
                    session.SendFileRequest(fileContext, request);
                }
                else
                    context.Tcs.SetResult(new List<byte[]>());
            }

            public override void OnTicksFileReject(ClientSession session, TicksFileRequestClientContext TicksFileRequestClientContext, QueryReject reject)
            {
                var context = (QueryQuoteHistoryTicksFilesAsyncContext)TicksFileRequestClientContext;
                var exception = new Exception(reject.Message);
                context.Tcs.SetException(exception);
            }

            public override void OnFileReport(ClientSession session, FileRequestClientContext FileRequestClientContext, FileReport report)
            {
                var context = (FileAsyncContext)FileRequestClientContext;
                context.Buffer.AddRange(report.Content);

                if (report.EndOfFile)
                {
                    context.Files.Add(context.Buffer.ToArray());
                    context.Buffer.Clear();
                    context.FileIndex++;
                    context.Tcs.SetResult(context.Files.Last());
                    if (context.FileIndex == context.Files.Count)
                    {
                        (context as BarsFileAsyncContext)?.ParentContext.Tcs.SetResult(context.Files);
                        (context as TicksFileAsyncContext)?.ParentContext.Tcs.SetResult(context.Files);
                        return;
                    }
                }

                var request = new FileRequest(0);
                request.FileId = context.FileIds[context.FileIndex];
                request.Chunk = ++context.ChunkIndex;
                session.SendFileRequest(context, request);
            }

            public override void OnFileReject(ClientSession session, FileRequestClientContext FileRequestClientContext, QueryReject reject)
            {
                var barsContext = FileRequestClientContext as BarsFileAsyncContext;
                if (barsContext != null)
                {
                    var exception = new Exception(reject.Message);
                    barsContext.Tcs.SetException(exception);
                    barsContext.ParentContext.Tcs.SetException(exception);
                }
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

            if (!_session.WaitConnect((int) Timeout.TotalMilliseconds))
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

        #region Quote History range

        public IEnumerable<Bar> QueryQuoteHistoryBarsRange(DateTime from, DateTime to, string symbol, string pereodicity, PriceType priceType)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            if (to < from)
            {
                DateTime temp = from;
                from = to;
                to = temp;
            }

            var periodicity = Periodicity.Parse(pereodicity);
            var timestamp = periodicity.GetPeriodStartTime(from);

            var filename = periodicity + " " + priceType.ToString("g").ToLowerInvariant();
            var zipSerializer = new ItemsZipSerializer<HistoryBar, List<HistoryBar>>(BarFormatter.Default, filename);
            var txtSerializer = new ItemsTextSerializer<HistoryBar, List<HistoryBar>>(BarFormatter.Default, filename);

            do
            {
                List<byte[]> content = QueryQuoteHistoryBarsFilesInternal(timestamp, symbol, pereodicity, priceType);
                foreach (var file in content)
                {
                    List<HistoryBar> historyBars;
                    if ((file.Length >= 4) && (file[0] == 0x50) && (file[1] == 0x4b) && (file[2] == 0x03) && (file[3] == 0x04))
                        historyBars = zipSerializer.Deserialize(file);
                    else
                        historyBars = txtSerializer.Deserialize(file);

                    foreach (var historyBar in historyBars)
                    {
                        if (historyBar.Time < from)
                            continue;
                        if (historyBar.Time > to)
                            break;
                        yield return new Bar
                        {
                            Time = historyBar.Time,
                            Open = historyBar.Open,
                            High = historyBar.Hi,
                            Low = historyBar.Low,
                            Close = historyBar.Close,
                            Volume = (decimal)historyBar.Volume
                        };
                    }
                }

                if (periodicity.Interval == TimeInterval.Second)
                {
                    if (periodicity.IntervalsCount < 10)
                        timestamp = timestamp.AddHours(1);
                    else
                        timestamp = timestamp.AddDays(1);
                }
                else if (periodicity.Interval == TimeInterval.Minute)
                {
                    if (periodicity.IntervalsCount < 5)
                        timestamp = timestamp.AddDays(1);
                    else
                        timestamp = timestamp.AddMonths(1);
                }
                else
                {
                    timestamp = timestamp.AddMonths(1);
                }

            } while (timestamp < to);
        }

        public IEnumerable<Tick> QueryQuoteHistoryTicksRange(DateTime from, DateTime to, string symbol, bool level2)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            if (to < from)
            {
                DateTime temp = from;
                from = to;
                to = temp;
            }

            var timestamp = from;

            var filename = level2 ? "ticks level2" : "ticks";
            var formatter = level2 ? (IFormatter<TickValue>)FeedTickLevel2Formatter.Instance : FeedTickFormatter.Instance;
            var zipSerializer = new ItemsZipSerializer<TickValue, TickValueList>(formatter, filename);
            var txtSerializer = new ItemsTextSerializer<TickValue, TickValueList>(formatter, filename);

            do
            {
                List<byte[]> content = QueryQuoteHistoryTicksFilesInternal(timestamp, symbol, level2);
                foreach (var file in content)
                {
                    TickValueList historyTicks;
                    if ((file.Length >= 4) && (file[0] == 0x50) && (file[1] == 0x4b) && (file[2] == 0x03) && (file[3] == 0x04))
                        historyTicks = zipSerializer.Deserialize(file);
                    else
                        historyTicks = txtSerializer.Deserialize(file);

                    foreach (var historyTick in historyTicks)
                    {
                        if (historyTick.Time < from)
                            continue;
                        if (historyTick.Time > to)
                            break;

                        var tick = new Tick()
                        {
                            Id = new TickId
                            {
                                Time = historyTick.Id.Time,
                                Index = historyTick.Id.Index
                            },
                            Level2 = new Level2Collection()
                        };

                        foreach (var level2record in historyTick.Level2)
                        {
                            if (level2record.Type == FxPriceType.Bid)
                            {
                                var bid = new Level2Value
                                {
                                    Price = (decimal)level2record.Price,
                                    Volume = (decimal)level2record.Volume
                                };
                                tick.Level2.Bids.Insert(0, bid);
                            }
                            if (level2record.Type == FxPriceType.Ask)
                            {
                                var ask = new Level2Value
                                {
                                    Price = (decimal)level2record.Price,
                                    Volume = (decimal)level2record.Volume
                                };
                                tick.Level2.Asks.Add(ask);
                            }
                        }

                        yield return tick;
                    }
                }

                timestamp = timestamp.AddHours(1);

            } while (timestamp < to);
        }

        #endregion

        #region Quote History files private methods

        private List<byte[]> QueryQuoteHistoryBarsFilesInternal(DateTime timestamp, string symbol, string pereodicity, PriceType priceType)
        {
            return ConvertToSync(QueryQuoteHistoryBarsFilesInternalAsync(timestamp, symbol, pereodicity, priceType), Timeout);
        }

        private Task<List<byte[]>> QueryQuoteHistoryBarsFilesInternalAsync(DateTime timestamp, string symbol, string pereodicity, PriceType priceType)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new QueryQuoteHistoryBarsFilesAsyncContext();

            // Create a new QH cache request
            var request = new QueryBarsFileRequest(0)
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = timestamp,
                Symbol = symbol,
                Periodicity = pereodicity,
                PriceType = (SoftFX.Net.QuoteHistory.PriceType)priceType
            };

            // Send request to the server
            _session.SendBarsFileRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        private List<byte[]> QueryQuoteHistoryTicksFilesInternal(DateTime timestamp, string symbol, bool level2)
        {
            return ConvertToSync(QueryQuoteHistoryTicksFilesInternalAsync(timestamp, symbol, level2), Timeout);
        }

        private Task<List<byte[]>> QueryQuoteHistoryTicksFilesInternalAsync(DateTime timestamp, string symbol, bool level2)
        {
            if (!IsConnected)
                throw new Exception("Client is not connected!");

            // Create a new async context
            var context = new QueryQuoteHistoryTicksFilesAsyncContext();

            // Create a new QH cache request
            var request = new QueryTicksFileRequest(0)
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = timestamp,
                Symbol = symbol,
                Level2 = level2
            };

            // Send request to the server
            _session.SendTicksFileRequest(context, request);

            // Return result task
            return context.Tcs.Task;
        }

        #endregion

        #region Async helpers

        public static void ConvertToSync(Task task, TimeSpan timeout)
        {
            try
            {
                if (!task.Wait(timeout))
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
                if (!task.Wait(timeout))
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
