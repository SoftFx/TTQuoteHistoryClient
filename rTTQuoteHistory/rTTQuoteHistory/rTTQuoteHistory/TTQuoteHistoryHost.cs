using System;
using TTQuoteHistoryClient;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessObjects.Requests;

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

        private static TickQuery _lastTickQuery;
        private static BarQuery _lastBarQuery;

        struct TickQuery
        {
            public DateTime from;
            public DateTime to;
            public string symbol;
            public bool level2;
        }

        struct BarQuery
        {
            public DateTime from;
            public DateTime to;
            public string symbol;
            public string periodicity;
            public string priceType;
        }

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
        public static int FileBarRequest(DateTime from, DateTime to, string symbol, string periodicity, string priceType)
        {
            _barList = new List<Bar>();
            int barCount = 0;
            foreach (
                var bar in
                    _client.QueryQuoteHistoryBarsRange(
                        new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute,
                            from.Second, from.Millisecond, DateTimeKind.Utc), new DateTime(to.Year, to.Month, to.Day, to.Hour, to.Minute,
                            to.Second, to.Millisecond, DateTimeKind.Utc), symbol, periodicity,
                        priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid))
            {
                if (barCount >= 1000000)
                {
                    _lastBarQuery = new BarQuery
                    {
                        from = _barList.Last().Time,
                        to = to,
                        periodicity = periodicity,
                        priceType = priceType,
                        symbol = symbol
                    };
                    break;
                }
                _barList.Add(bar);
                barCount++;
            }
            return 0;
        }

        public static int NextFileBarRequest()
        {
            if (_lastBarQuery.symbol == null) return -1;
            FileBarRequest(_lastBarQuery.from, _lastBarQuery.to, _lastBarQuery.symbol, _lastBarQuery.periodicity,
                _lastBarQuery.priceType);
            return 0;
        }

        private static List<Bar> BarMerge(DateTime timestamp, double count, string symbol, string periodicity,
            string priceType)
        {
            var buf = _client.QueryQuoteHistoryBars(
                        new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour,
                            timestamp.Minute, timestamp.Second,
                            timestamp.Millisecond, DateTimeKind.Utc), (int)count, symbol, periodicity,
                        priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid);
            if (count > 0) buf.Reverse();
            buf.RemoveAt(0);
            return buf;
        }

        public static int BarRequest(DateTime timestamp, double count, string symbol, string periodicity,
            string priceType)
        {
            var sign = Math.Sign(count);
            if (count * sign <= 5000)
            {
                _barList =
                    _client.QueryQuoteHistoryBars(
                        new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour,
                            timestamp.Minute, timestamp.Second,
                            timestamp.Millisecond, DateTimeKind.Utc), (int)count, symbol, periodicity,
                        priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid);
            }
            else
            {
                _barList =
                    _client.QueryQuoteHistoryBars(
                        new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour,
                            timestamp.Minute, timestamp.Second,
                            timestamp.Millisecond, DateTimeKind.Utc), 5000 * sign, symbol, periodicity,
                        priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid);
                if (_barList.Count > 0)
                    timestamp = (sign < 0) ? _barList[_barList.Count - 1].Time : _barList[0].Time;
                if (sign > 0) _barList.Reverse();
                while (_barList.Count < count * sign)
                {
                    _barList.AddRange(BarMerge(timestamp, count - _barList.Count * sign + sign, symbol, periodicity, priceType));
                    if (_barList.Count > 0)
                        timestamp = _barList[_barList.Count - 1].Time;
                }
                if (sign > 0) _barList.Reverse();
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

        public static int FileTickRequest(DateTime from, DateTime to, string symbol, bool level2)
        {
            _tickList = new List<Tick>();
            int tickCount = 0;
            foreach (
                var tick in
                    _client.QueryQuoteHistoryTicksRange(
                        new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute,
                            from.Second, from.Millisecond, DateTimeKind.Utc), new DateTime(to.Year, to.Month, to.Day, to.Hour, to.Minute,
                            to.Second, to.Millisecond, DateTimeKind.Utc), symbol, level2))
            {
                if (tickCount >= 1000000)
                {
                    _lastTickQuery = new TickQuery
                    {
                        from = _tickList.Last().Id.Time,
                        to = to,
                        level2 = level2,
                        symbol = symbol
                    };
                    break;
                }
                _tickList.Add(tick);
                tickCount++;
            }
            return 0;
        }

        public static int NextFileTickRequest()
        {
            if (_lastTickQuery.symbol == null) return -1;
            FileTickRequest(_lastTickQuery.from, _lastTickQuery.to, _lastTickQuery.symbol, _lastTickQuery.level2);
            return 0;
        }

        private static List<Tick> TickMerge(DateTime timestamp, double count, string symbol, bool level2)
        {
            var buf = _client.QueryQuoteHistoryTicks(
                        new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute,
                            timestamp.Second, timestamp.Millisecond - Math.Sign(count), DateTimeKind.Utc), (int)count, symbol, level2);
            if (count > 0) buf.Reverse();
            return buf;
        }

        public static int TickRequest(DateTime timestamp, double count, string symbol, bool level2)
        {
            var sign = Math.Sign(count);
            if (count * sign <= 1000)
            {
                _tickList =
                    _client.QueryQuoteHistoryTicks(
                        new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute,
                            timestamp.Second, timestamp.Millisecond, DateTimeKind.Utc), (int)count, symbol, level2);
            }
            else
            {
                _tickList =
                    _client.QueryQuoteHistoryTicks(
                        new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute,
                            timestamp.Second, timestamp.Millisecond, DateTimeKind.Utc), 1000 * sign, symbol, level2);
                if (_tickList.Count > 0)
                    timestamp = (sign < 0) ? _tickList[_tickList.Count - 1].Id.Time : _tickList[0].Id.Time;
                if (sign > 0) _tickList.Reverse();
                while (_tickList.Count < count * sign)
                {
                    _tickList.AddRange(TickMerge(timestamp, count - _tickList.Count * sign, symbol, level2));
                    if (_tickList.Count > 0)
                        timestamp = _tickList[_tickList.Count - 1].Id.Time;
                }
                if (sign > 0) _tickList.Reverse();
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
                for (var i = 1; i <= tick.Level2.Asks.Count - count; i++)
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
                for (var i = 1; i <= tick.Level2.Asks.Count - count; i++)
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
            Connect("name", "tp.st.soft-fx.eu", 5020, "5", "123qwe!");
            TickRequest(new DateTime(2017,02,01), -1001, "EURUSD", true);
            //var a = GetTickL2PriceBid();
            //  foreach (var tick in _tickList)
            // {
            //    Console.WriteLine(tick);
            //}
        }
    }
}
