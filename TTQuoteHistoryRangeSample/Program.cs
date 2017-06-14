using System;
using System.Diagnostics;
using NDesk.Options;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryRangeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool help = false;

                string address = "localhost";
                string login = "5";
                string password = "123qwe!";
                int port = 5020;

                DateTime from = DateTime.UtcNow.AddDays(-1);
                DateTime to = DateTime.UtcNow;
                string symbol = "EURUSD";
                string periodicity = "M1";
                PriceType priceType = PriceType.Bid;
                bool bars = false;
                bool ticks = false;
                bool level2 = false;
                bool verbose = false;

                var options = new OptionSet()
                {
                    { "a|address=", v => address = v },
                    { "l|login=", v => login = v },
                    { "w|password=", v => password = v },
                    { "p|port=", v => port = int.Parse(v) },
                    { "h|?|help",   v => help = v != null },
                    { "f|from=", v => from = DateTime.Parse(v) },
                    { "t|to=", v => to = DateTime.Parse(v) },
                    { "s|symobl=", v => symbol = v },
                    { "d|periodicity=", v => periodicity = v },
                    { "v|verbose=", v => verbose = bool.Parse(v) },
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
                    Console.Write("TTQuoteHistoryRangeSample: ");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Try `TTQuoteHistoryRangeSample --help' for more information.");
                    return;
                }

                if (help)
                {
                    Console.WriteLine("TTQuoteHistoryRangeSample usage:");
                    options.WriteOptionDescriptions(Console.Out);
                    return;
                }

                WorkingThread(address, login, password, port, from, to, symbol, periodicity, priceType, bars, ticks, level2, verbose);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        static void WorkingThread(string address, string login, string password, int port, DateTime from, DateTime to, string symbol, string periodicity, PriceType priceType, bool bars, bool ticks, bool level2, bool verbose)
        {
            try
            {
                // Create an instance of Quote History client
                using (var client = new QuoteHistoryClient("QuoteHistoryCache", port, false))
                {
                    // Connect to the server
                    client.Connect(address);

                    // Login
                    client.Login(login, password, "", "");

                    if (level2)
                        RequestTicksFiles(client, from , to, symbol, true, verbose);

                    if (ticks)
                        RequestTicksFiles(client, from, to, symbol, false, verbose);

                    if (bars)
                        RequestBarsFiles(client, from, to, symbol, periodicity, priceType, verbose);

                    // Logout
                    client.Logout("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        static void RequestTicksFiles(QuoteHistoryClient client, DateTime from, DateTime to, string symbol, bool level2, bool verbose)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int count = 0;
            foreach (var tick in client.QueryQuoteHistoryTicksRange(from, to, symbol, level2))
            {
                count++;
                if (verbose)
                    Console.WriteLine(tick);
            }

            long elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed = {elapsed}ms");
            Console.WriteLine($"Throughput = {((double)count / (double)elapsed) * 1000.0:0.#####} ticks per second");
        }

        static void RequestBarsFiles(QuoteHistoryClient client, DateTime from, DateTime to, string symbol, string periodicity, PriceType priceType, bool verbose)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int count = 0;
            foreach (var bar in client.QueryQuoteHistoryBarsRange(from, to, symbol, periodicity, priceType))
            {
                count++;
                if (verbose)
                    Console.WriteLine(bar);
            }

            long elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed = {elapsed}ms");
            Console.WriteLine($"Throughput = {((double)count/(double)elapsed)*1000.0:0.#####} bars per second");
        }
    }
}
