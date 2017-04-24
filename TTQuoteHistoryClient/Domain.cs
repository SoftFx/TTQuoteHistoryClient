using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTQuoteHistoryClient
{
    public enum PriceType
    {
        Bid = 0,
        Ask = 1
    }

    public class Bar
    {
        public DateTime Time;
        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;
        public decimal Volume;

        public override string ToString() => $"{Time:yyyy.MM.dd HH:mm:ss.fff}: {Open:0.#####}, {High:0.#####}, {Low:0.#####}, {Close:0.#####}, {Volume:0.#####}";
    }

    public class TickId
    {
        public DateTime Time;
        public byte Index;

        public override string ToString() => $"{Time:yyyy.MM.dd HH:mm:ss.fff}.{Index}";
    }

    public class Level2Value
    {
        public decimal Price;
        public decimal Volume;

        public override string ToString() => $"Price={Price:0.#####}, Volume={Volume:0.#####}";
    }

    public class Level2Collection
    {
        public List<Level2Value> Bids = new List<Level2Value>();
        public List<Level2Value> Asks = new List<Level2Value>();

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("Bids[");
            foreach (var bid in Bids)
            {
                builder.Append(" { ");
                builder.Append(bid);
                builder.Append(" } ");
            }
            builder.Append("], Asks[");
            foreach (var ask in Asks)
            {
                builder.Append(" { ");
                builder.Append(ask);
                builder.Append(" } ");
            }
            builder.Append("]");

            return builder.ToString();
        }
    }

    public class Tick
    {
        public TickId Id;
        public Level2Collection Level2;

        public bool HasBids => Level2?.Bids?.Count > 0;
        public bool HasAsks => Level2?.Asks?.Count > 0;

        public Level2Value BestBid => Level2?.Bids?.FirstOrDefault();
        public Level2Value BestAsk => Level2?.Asks?.FirstOrDefault();

        public override string ToString() => $"{Id}: {Level2}";
    }
}
