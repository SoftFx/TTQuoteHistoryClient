using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.BusinessObjects;
using TickTrader.Common.Business;
using TickTrader.Server.QuoteHistory.Cache;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryCache
{
    public class BarsHistoryCache : HistoryCache<HistoryBar>
    {
        private readonly QuoteHistoryClient _historyClient;

        public Periodicity Periodicity { get; }
        public FxPriceType? PriceType { get; }

        public BarsHistoryCache(string symbol, Periodicity periodicity, FxPriceType priceType, QuoteHistoryClient historyClient) : base(symbol)
        {
            _historyClient = historyClient;

            Periodicity = periodicity;
            PriceType = priceType;
        }

        protected override DateTime NormalizeTimestamp(DateTime timestamp)
        {
            return Periodicity.GetPeriodStartTime(timestamp);
        }

        protected override List<HistoryBar> RequestHistory(DateTime timestamp, int count)
        {
            var result = new List<HistoryBar>(Math.Abs(count));
            if (count == 0)
                return result;

            try
            {
                var report = _historyClient.QueryQuoteHistoryBars(timestamp, count, Symbol, Periodicity.ToString(), (PriceType.HasValue && (PriceType.Value == FxPriceType.Ask)) ? TTQuoteHistoryClient.PriceType.Ask : TTQuoteHistoryClient.PriceType.Bid);
                var bars = report.Select(srcBar =>
                {
                    var dstBar = new HistoryBar
                    {
                        Time = srcBar.Time,
                        Open = srcBar.Open,
                        Hi = srcBar.High,
                        Low = srcBar.Low,
                        Close = srcBar.Close
                    };
                    return dstBar;
                });
                result.AddRange(bars);
            }
            catch (Exception) {}

            return result;
        }
    }
}
