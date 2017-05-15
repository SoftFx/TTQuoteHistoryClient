using System;
using TTQuoteHistoryClient;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rTTQuoteHistory
{
    public class TTQuoteHistoryHost
    {

        private static QuoteHistoryClient _client;
        private static List<Bar> _barList;
        private static List<Tick> _tickList;

        public static void Connect(string address, double port)
        {
            _client = new QuoteHistoryClient(address, (int)port);
            _client.Connect();
        }

        public static void Disconnect()
        {
            _client.Disconnect();
        }

        #region Bars
        public static void BarRequest(DateTime timestamp, double count, string symbol, string periodicity,
            string priceType)
        {
            _barList = _client.QueryQuoteHistoryBars(new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second,
                timestamp.Millisecond, DateTimeKind.Utc), (int)count, symbol, periodicity, priceType.Equals("Ask") ? PriceType.Ask : PriceType.Bid);
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
        public static void TickRequest(DateTime timestamp, double count, string symbol, bool level2)
        {
            _tickList = _client.QueryQuoteHistoryTicks(new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute,
                timestamp.Second, timestamp.Millisecond, DateTimeKind.Utc), (int)count, symbol, level2);
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
                result.AddRange(tick.HasBids ? tick.Level2.Bids.Select(bid => (double)bid.Volume) : new List<double>(tick.Level2.Asks.Count));
            }
            return result.ToArray();
        }

        public static double[] GetTickL2VolumeAsk()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                result.AddRange(tick.HasAsks ? tick.Level2.Asks.Select(ask => (double)ask.Volume) : new List<double>(tick.Level2.Bids.Count));
            }
            return result.ToArray();
        }

        public static double[] GetTickL2PriceBid()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                result.AddRange(tick.HasBids ? tick.Level2.Bids.Select(bid => (double)bid.Price) : new List<double>(tick.Level2.Asks.Count));
            }
            return result.ToArray();
        }

        public static double[] GetTickL2PriceAsk()
        {
            var result = new List<double>();
            foreach (var tick in _tickList)
            {
                result.AddRange(tick.HasAsks ? tick.Level2.Asks.Select(ask => (double)ask.Price) : new List<double>(tick.Level2.Bids.Count));
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

        static void Main(string[] args)
        {
            bool help = false;

            string address = "tp.st.soft-fx.eu";
            int port = 5020;

            DateTime timestamp = DateTime.UtcNow;
            int count = 100;
            string symbol = "EURUSD";
            string periodicity = "M1";
            PriceType priceType = PriceType.Bid;
            bool bars = false;
            bool ticks = false;
            bool level2 = true;
            try
            {
                // Create an instance of Quote History client
                using (var client = new QuoteHistoryClient(address, port))
                {
                    // Connect to the server
                    client.Connect();

                    // Request the server
                    if (level2)
                    {
                        // Request for the level2 history
                        var result = client.QueryQuoteHistoryTicks(timestamp, count, symbol, true);
                        foreach (var tick in result)
                            Console.WriteLine(tick);
                    }
                    else if (ticks)
                    {
                        // Request for the ticks history
                        var result = client.QueryQuoteHistoryTicks(timestamp, count, symbol, false);
                        foreach (var tick in result)
                            Console.WriteLine(tick);
                    }
                    else if (bars)
                    {
                        // Request for the bars history
                        var result = client.QueryQuoteHistoryBars(timestamp, count, symbol, periodicity, priceType);
                        foreach (var bar in result)
                            Console.WriteLine(bar);
                    }

                    // Disconnect to the server
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
