using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TickTrader.BusinessObjects;
using TickTrader.BusinessObjects.QuoteHistory.Exceptions;
using TickTrader.Common.Business;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryCache
{
    public class HistoryCacheManager
    {
        private readonly QuoteHistoryClient _historyClient;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly List<string> _symbols = new List<string>();
        private readonly List<Periodicity> _periodicities = new List<Periodicity>();
        private readonly Dictionary<Tuple<string, Periodicity, FxPriceType>, BarsHistoryCache> _cachedBars = new Dictionary<Tuple<string, Periodicity, FxPriceType>,BarsHistoryCache>();
        private readonly Dictionary<string, TicksHistoryCache> _cachedTicks = new Dictionary<string, TicksHistoryCache>();
        private readonly Dictionary<string, TicksHistoryCache> _cachedLevel2 = new Dictionary<string, TicksHistoryCache>();

        public HistoryCacheManager(QuoteHistoryClient historyClient)
        {
            _historyClient = historyClient;
        }

        public void Initialize(int count = 0)
        {
            try
            {
                _lock.EnterWriteLock();

                var symbols = _historyClient.GetSupportedSymbols();
                var periodicities = _historyClient.GetSupportedPeriodicities().Select(Periodicity.Parse).ToList();
                foreach (var symbol in symbols)
                {
                    _symbols.Add(symbol);

                    foreach (var periodicity in periodicities)
                    {
                        _periodicities.Add(periodicity);

                        var keyBid = new Tuple<string, Periodicity, FxPriceType>(symbol, periodicity, FxPriceType.Bid);
                        var keyAsk = new Tuple<string, Periodicity, FxPriceType>(symbol, periodicity, FxPriceType.Ask);
                        _cachedBars[keyBid] = new BarsHistoryCache(keyBid.Item1, keyBid.Item2, keyBid.Item3, _historyClient);
                        _cachedBars[keyAsk] = new BarsHistoryCache(keyAsk.Item1, keyAsk.Item2, keyAsk.Item3, _historyClient);
                    }
                    _cachedTicks[symbol] = new TicksHistoryCache(symbol, false, _historyClient);
                    _cachedLevel2[symbol] = new TicksHistoryCache(symbol, true, _historyClient);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            try
            {
                _lock.EnterReadLock();

                foreach (var historyCache in _cachedBars.Values)
                    historyCache.InitializeCache(count);
                foreach (var historyCache in _cachedTicks.Values)
                    historyCache.InitializeCache(count);
                foreach (var historyCache in _cachedLevel2.Values)
                    historyCache.InitializeCache(count);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Close()
        {
            try
            {
                _lock.EnterWriteLock();

                _cachedBars.Clear();
                _cachedTicks.Clear();
                _cachedLevel2.Clear();

                _periodicities.Clear();
                _symbols.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                _lock.EnterReadLock();

                foreach (var historyCache in _cachedBars.Values)
                    historyCache.ClearCache();
                foreach (var historyCache in _cachedTicks.Values)
                    historyCache.ClearCache();
                foreach (var historyCache in _cachedLevel2.Values)
                    historyCache.ClearCache();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<string> GetSymbols()
        {
            try
            {
                _lock.EnterReadLock();

                return _symbols.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<Periodicity> GetPeriodicities()
        {
            try
            {
                _lock.EnterReadLock();

                return _periodicities.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<HistoryBar> GetBarsHistory(string symbol, Periodicity periodicity, FxPriceType priceType, DateTime timestamp, int count)
        {
            BarsHistoryCache cache;

            try
            {
                _lock.EnterReadLock();

                var key = new Tuple<string, Periodicity, FxPriceType>(symbol, periodicity, priceType);
                if (!_cachedBars.TryGetValue(key, out cache) || (cache == null))
                    throw new HistoryNotFoundException();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return cache.GetHistory(timestamp, count);
        }

        public List<TickValue> GetTicksHistory(string symbol, bool level2, DateTime timestamp, int count)
        {
            TicksHistoryCache cache;

            try
            {
                _lock.EnterReadLock();

                if (level2)
                {
                    if (!_cachedLevel2.TryGetValue(symbol, out cache) || (cache == null))
                        throw new HistoryNotFoundException();
                }
                else
                {
                    if (!_cachedTicks.TryGetValue(symbol, out cache) || (cache == null))
                        throw new HistoryNotFoundException();
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return cache.GetHistory(timestamp, count);
        }

        public string DumpBarsIntervals(string symbol, Periodicity periodicity, FxPriceType priceType)
        {
            BarsHistoryCache cache;

            try
            {
                _lock.EnterReadLock();

                var key = new Tuple<string, Periodicity, FxPriceType>(symbol, periodicity, priceType);
                if (!_cachedBars.TryGetValue(key, out cache) || (cache == null))
                    throw new HistoryNotFoundException();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return cache.DumpIntervals();
        }

        public string DumpTicksIntervals(string symbol, bool level2)
        {
            TicksHistoryCache cache;

            try
            {
                _lock.EnterReadLock();

                if (level2)
                {
                    if (!_cachedLevel2.TryGetValue(symbol, out cache) || (cache == null))
                        throw new HistoryNotFoundException();
                }
                else
                {
                    if (!_cachedTicks.TryGetValue(symbol, out cache) || (cache == null))
                        throw new HistoryNotFoundException();
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return cache.DumpIntervals();
        }
    }
}
