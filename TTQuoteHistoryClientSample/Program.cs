using System;
using NDesk.Options;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryClientSample
{
    class Program
    {
        static void Main(string[] args)
        {
            bool help = false;

            string address = "localhost";
            int port = 5020;

            DateTime timestamp = DateTime.UtcNow;
            int count = 100;
            string symbol = "EURUSD";
            string periodicity = "M1";
            PriceType priceType = PriceType.Bid;
            bool bars = false;
            bool ticks = false;
            bool level2 = false;

            var options = new OptionSet()
            {
                { "a|address=", v => address = v },
                { "p|port=", v => port = int.Parse(v) },
                { "h|?|help",   v => help = v != null },
                { "t|timestamp=", v => timestamp = DateTime.Parse(v) },
                { "c|count=", v => count = int.Parse(v) },
                { "s|symobl=", v => symbol = v },
                { "d|periodicity=", v => periodicity = v },
                { "r|request=", v =>
                    {
                        switch (v.ToLowerInvariant())
                        {
                            case "bids":
                                priceType = PriceType.Bid;
                                bars = true;
                                break;
                            case "asks":
                                priceType = PriceType.Ask;
                                bars = true;
                                break;
                            case "ticks":
                                ticks = true;
                                break;
                            case "level2":
                                level2 = true;
                                break;
                            default:
                                throw new Exception("Unknown request type: " + v);
                        }
                    }
                },
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("TTQuoteHistoryClientSample: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TTQuoteHistoryClientSample --help' for more information.");
                return;
            }

            if (help)
            {
                Console.WriteLine("TTQuoteHistoryClientSample usage:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

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
