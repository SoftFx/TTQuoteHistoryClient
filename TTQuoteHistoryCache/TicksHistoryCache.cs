using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.BusinessObjects;
using TickTrader.Server.QuoteHistory.Cache;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryCache
{
    public class TicksHistoryCache : HistoryCache<TickValue>
    {
        private readonly QuoteHistoryClient _historyClient;
        private readonly bool _level2;

        public TicksHistoryCache(string symbol, bool level2, QuoteHistoryClient historyClient) : base(symbol)
        {
            _historyClient = historyClient;
            _level2 = level2;
        }

        protected override DateTime NormalizeTimestamp(DateTime timestamp)
        {
            return timestamp;
        }

        protected override List<TickValue> RequestHistory(DateTime timestamp, int count)
        {
            var result = new List<TickValue>(Math.Abs(count));
            if (count == 0)
                return result;

            try
            {
                var report = _historyClient.QueryQuoteHistoryTicks(timestamp, count, Symbol, _level2);
                var ticks = report.Select(srcTick =>
                {
                    var bids = srcTick.Level2.Bids.Select(bid => new TickTrader.BusinessObjects.Level2Value(new Price(bid.Price), (double)bid.Volume));
                    var asks = srcTick.Level2.Asks.Select(ask => new TickTrader.BusinessObjects.Level2Value(new Price(ask.Price), (double)ask.Volume));
                    var level2 = new TickTrader.BusinessObjects.Level2Collection(bids, asks);
                    var dstTick = new TickValue(new FeedTickId(srcTick.Id.Time, srcTick.Id.Index), level2);
                    return dstTick;
                });
                result.AddRange(ticks);
            }
            catch (Exception) {}

            return result;
        }
    }
}
