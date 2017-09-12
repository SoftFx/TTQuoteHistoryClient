using System;
using System.CodeDom;
using System.Collections;
using TTQuoteHistoryClient;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Common.ModelLayer;

namespace rTTQuoteHistory
{
    public class TTQuoteHistoryHost
    {
        private const string DefaultAddress = "ttdemo.fxopen.com";
        private const string DefaultLogin = "59932";
        private const string DefaultPassword = "8mEx7zZ2";
        private const string DefaultName = "client";

        private static QuoteHistoryClient _client;
        private static List<Bar> _barList;
        private static List<Tick> _tickList;

        private static IEnumerable<List<Tick>> _tickRange;
        private static IEnumerator<List<Tick>> _tickIEnumerator; 
        private static IEnumerable<List<Bar>> _barRange;
        private static IEnumerator<List<Bar>> _barIEnumerator;  

        private static bool isEndOfBarStream = true;
        public static bool IsEndOfBarStream() => isEndOfBarStream;
        private static bool isEndOfTickStream = true;
        public static bool IsEndOfTickStream() => isEndOfTickStream;
        private static bool isEndOfTickL2Stream = true;
        public static bool IsEndOfTickL2Stream() => isEndOfTickL2Stream;

        private class LastBarQuery
        {
            public DateTime from;
            public DateTime to;
            public string symbol;
            public string periodicity;
            public string priceType;
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return true;
                if (obj is LastBarQuery)
                {
                    var newObj = (LastBarQuery) obj;
                    return from == newObj.from && to == newObj.to && symbol == newObj.symbol &&
                           periodicity == newObj.periodicity && priceType == newObj.priceType;
                }
                return false;
            }
        }

        private static LastBarQuery _lastBarQuery;

        private class LastTickQuery
        {
            public DateTime from;
            public DateTime to;
            public string symbol;
            public bool level2;
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return true;
                if (obj is LastTickQuery)
                {
                    var newObj = (LastTickQuery)obj;
                    return from == newObj.from && to == newObj.to && symbol == newObj.symbol &&
                           level2 == newObj.level2;
                }
                return false;
            }
        }

        private static LastTickQuery _lastTickQuery;

        public static int Connect(string name, string address, double port, string login, string password)
        {
            try
            {
                _client = new QuoteHistoryClient(name == "" ? DefaultName : name, (int)port);
                _client.Connect(address == "" ? DefaultAddress : address);
                _client.Login(login == "" ? DefaultLogin : login, password == "" ? DefaultPassword : password, "", "");
                return 0;
            }
            catch (TimeoutException ex)
            {
                return -1;
            }
            catch (SoftFX.Net.Core.DisconnectException ex)
            {
                return -2;
            }
        }

        public static int Disconnect()
        {
            try
            {
                _client.Logout("");
                _client.Disconnect();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        #region Bars

        public static bool CheckNewBarParams(DateTime from, DateTime to, string symbol, string periodicity,
            string priceType)
        {
            var newBarQuery = new LastBarQuery
            {
                from = from,
                to = to,
                symbol = symbol,
                periodicity = periodicity,
                priceType = priceType
            };
            if (newBarQuery.Equals(_lastBarQuery))
            {
                return false;
            }
            _lastBarQuery = newBarQuery;
            return true;
        }

        public static int FillBarRange(DateTime from, DateTime to, string symbol, string periodicity, string priceType)
        {
            _barList = new List<Bar>();
            _barRange = DivideBars(_client.QueryQuoteHistoryBarsRange(
                new DateTime(from.Ticks, DateTimeKind.Utc), new DateTime(to.Ticks, DateTimeKind.Utc), symbol,
                periodicity,
                priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid));
            _barIEnumerator = _barRange.GetEnumerator();
            return 0;
        }

        public static IEnumerable<List<Bar>> DivideBars(IEnumerable<Bar> query)
        {
            List<Bar> curRes = new List<Bar>();
            foreach (var bar in query)
            {
                curRes.Add(bar);
                if (curRes.Count >= 1000000)
                {
                    yield return curRes;
                    curRes = new List<Bar>();
                }
            }
            if (curRes.Count > 0)
            {
                yield return curRes;
            }
        }

        public static int StreamBarRequest()
        {      
            _barList?.Clear();
            if (_barIEnumerator.MoveNext())
            {
                isEndOfBarStream = false;
                _barList = _barIEnumerator.Current;
                return 0;
            }
            isEndOfBarStream = true;
            return -1;
        }

        public static int BarRequest(DateTime timestamp, double count, string symbol, string periodicity,
            string priceType)
        {
            if (count<0)
            {
                count *= -1;
                if (count <= 5000)
                {
                    _barList =
                        _client.QueryQuoteHistoryBars(
                            new DateTime(timestamp.Ticks, DateTimeKind.Utc), -(int)count, symbol, periodicity,
                            priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid);
                }
                else
                {
                    _barList = new List<Bar>();
                    while (count >= 0)
                    {
                        _barList.AddRange(_client.QueryQuoteHistoryBars(
                            new DateTime(timestamp.Ticks, DateTimeKind.Utc), -(int)(count > 5000 ? 5000 : count), symbol, periodicity,
                            priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid));
                        count -= 5000;
                        timestamp = _barList.Last().Time;
                    }
                }
            }
            else
            {
                if (count <= 5000)
                {
                    _barList =
                        _client.QueryQuoteHistoryBars(
                            new DateTime(timestamp.Ticks, DateTimeKind.Utc), (int)count, symbol, periodicity,
                            priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid);
                }
                else
                {
                    _barList = new List<Bar>();
                    while (count >= 0)
                    {
                        _barList.InsertRange(0, _client.QueryQuoteHistoryBars(
                            new DateTime(timestamp.Ticks, DateTimeKind.Utc), (int)(count > 5000 ? 5000 : count), symbol, periodicity,
                            priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid));
                        count -= 5000;
                        timestamp = periodicity.Equals("M1") ? _barList.First().Time.AddMinutes(-1) :
                        _barList.First().Time.AddHours(-1);
                    }
                }
            }
            return 0;
        }

        public static DateTime[] GetBarTime()
        {
            return _barList.Select(bar => bar.Time.AddHours(3)).ToArray();
        }

        public static double[] GetBarOpen()
        {
            return _barList.Select(bar => (double)bar.Open).ToArray();
        }

        public static double[] GetBarHigh()
        {
            return _barList.Select(bar => (double)bar.High).ToArray();
        }

        public static double[] GetBarLow()
        {
            return _barList.Select(bar => (double)bar.Low).ToArray();
        }

        public static double[] GetBarClose()
        {
            return _barList.Select(bar => (double)bar.Close).ToArray();
        }

        public static double[] GetBarVolume()
        {
            return _barList.Select(bar => (double)bar.Volume).ToArray();
        }
        #endregion

        #region Ticks

        public static bool CheckNewTickParams(DateTime from, DateTime to, string symbol, bool level2)
        {
            var newTickQuery = new LastTickQuery
            {
                from = from,
                to = to,
                symbol = symbol,
                level2 = level2
            };
            if (newTickQuery.Equals(_lastTickQuery))
            {
                return false;
            }
            _lastTickQuery = newTickQuery;
            return true;
        }

        public static int FillTickRange(DateTime from, DateTime to, string symbol, bool level2)
        {
            _tickList = new List<Tick>();
            _tickRange = DivideTicks(_client.QueryQuoteHistoryTicksRange(new DateTime(from.Ticks,DateTimeKind.Utc), new DateTime(to.Ticks,DateTimeKind.Utc), symbol,level2));
            _tickIEnumerator = _tickRange.GetEnumerator();
            return 0;
        }

        public static IEnumerable<List<Tick>> DivideTicks(IEnumerable<Tick> query)
        {
            List<Tick> curRes = new List<Tick>();
            foreach (var tick in query)
            {
                curRes.Add(tick);
                if (curRes.Count >= 1000000)
                {
                    yield return curRes;
                    curRes = new List<Tick>();
                }
            }
            if (curRes.Count > 0)
            {
                yield return curRes;
            }
        }

        public static int StreamTickRequest(bool level2)
        {
            
            _tickList?.Clear();
            if (_tickIEnumerator.MoveNext())
            {
                if (level2) isEndOfTickL2Stream = false;
                else isEndOfTickStream = false;
                _tickList = _tickIEnumerator.Current;
                return 0;
            }
            if (level2) isEndOfTickL2Stream = true;
            else isEndOfTickStream = true;
            return -1;
        }

        public static int TickRequest(DateTime timestamp, double count, string symbol, bool level2)
        {
            if (count < 0)
            {
                count *= -1;
                if (count <= 1000)
                {
                    _tickList = _client.QueryQuoteHistoryTicks(timestamp, -(int)count, symbol, level2);
                }
                else
                {
                    _tickList = new List<Tick>();
                    while (count >= 0)
                    {
                        _tickList.AddRange(_client.QueryQuoteHistoryTicks(timestamp, -(int)(count > 1000 ? 1000 : count), symbol, level2));
                        count -= 1000;
                        timestamp = _tickList.Last().Id.Time.AddMilliseconds(1);
                    }
                }
            }
            else
            {
                if (count <= 1000)
                {
                    _tickList = _client.QueryQuoteHistoryTicks(timestamp, (int)count, symbol, level2);
                }
                else
                {
                    _tickList = new List<Tick>();
                    while (count >= 0)
                    {
                        _tickList.InsertRange(0, _client.QueryQuoteHistoryTicks(timestamp, (int)(count > 1000 ? 1000 : count), symbol, level2));
                        count -= 1000;
                        timestamp = _tickList.First().Id.Time.AddMilliseconds(-1);
                    }
                }
            }
            return 0;
        }

        public static DateTime[] GetTickDate()
        {
            return _tickList.Select(tick => tick.Id.Time.AddHours(3)).ToArray();
        }

        public static double[] GetTickBid()
        {
            return _tickList.Select(tick => tick.HasBids ? (double)tick.BestBid.Price : 0.0).ToArray();
        }

        public static double[] GetTickAsk()
        {
            return _tickList.Select(tick => tick.HasAsks ? (double)tick.BestAsk.Price : 0.0).ToArray();
        }

        public static double[] GetTickL2VolumeBid()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                var buf = tick.HasBids
                    ? tick.Level2.Bids.Select(bid => (double)bid.Volume).ToList()
                    : new List<double>(tick.Level2.Asks.Count);
                var count = buf.Count;
                for (var i = 1; i <= tick.Level2.Asks.Count - count; i++)
                {
                    buf.Add(0);
                }
                result.AddRange(buf);
            }
            return result.ToArray();
        }

        public static double[] GetTickL2VolumeAsk()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                var buf = tick.HasAsks ? tick.Level2.Asks.Select(ask => (double)ask.Volume).ToList() : new List<double>(tick.Level2.Bids.Count);
                var count = buf.Count;
                for (var i = 1; i <= tick.Level2.Bids.Count - count; i++)
                {
                    buf.Add(0);
                }
                result.AddRange(buf);
            }
            return result.ToArray();
        }

        public static double[] GetTickL2PriceBid()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                var buf = tick.HasBids ? tick.Level2.Bids.Select(bid => (double)bid.Price).ToList() : new List<double>(tick.Level2.Asks.Count);
                var count = buf.Count;
                for (var i = 1; i <= tick.Level2.Asks.Count - count; i++)
                {
                    buf.Add(0);
                }
                result.AddRange(buf);
            }
            return result.ToArray();
        }

        public static double[] GetTickL2PriceAsk()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                var buf = tick.HasAsks ? tick.Level2.Asks.Select(ask => (double)ask.Price).ToList() : new List<double>(tick.Level2.Bids.Count);
                var count = buf.Count;
                for (var i = 1; i <= tick.Level2.Bids.Count - count; i++)
                {
                    buf.Add(0);
                }
                result.AddRange(buf);
            }
            return result.ToArray();
        }

        public static int[] GetTickL2Level()
        {
            var result = new List<int>();
            foreach (var tick in _tickList)
            {
                result.AddRange(Enumerable.Range(1, Math.Max(tick.Level2.Bids.Count, tick.Level2.Asks.Count)));
            }
            return result.ToArray();
        }

        public static DateTime[] GetTickL2DateTime()
        {
            var result = new List<DateTime>();
            foreach (var tick in _tickList)
            {
                for (int level = 0; level < Math.Max(tick.Level2.Bids.Count, tick.Level2.Asks.Count); level++)
                {
                    result.Add(tick.Id.Time.AddHours(3));
                }
            }
            return result.ToArray();
        }

        #endregion

        public static string[] GetSupportesSymbols()
        {
            return _client.GetSupportedSymbols().ToArray();
        }

        public static string[] GetSupportedPeriodicities()
        {
            return _client.GetSupportedPeriodicities().ToArray();
        }

        public static int Clear()
        {
            try
            {
                _tickList = null;
                _barList = null;
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        static void Main(string[] args)
        {

        }
    }
}
