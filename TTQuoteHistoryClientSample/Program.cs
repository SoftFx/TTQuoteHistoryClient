using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NDesk.Options;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryClientSample
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

                DateTime timestamp = DateTime.UtcNow;
                int count = 100;
                int threads = 1;
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
                    { "t|timestamp=", v => timestamp = DateTime.Parse(v) },
                    { "c|count=", v => count = int.Parse(v) },
                    { "s|symobl=", v => symbol = v },
                    { "d|periodicity=", v => periodicity = v },
                    { "z|threads=", v => threads = int.Parse(v) },
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

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < threads; i++)
                    tasks.Add(Task.Factory.StartNew(() => WorkingThread(address, login, password, port, timestamp, count, symbol, periodicity, priceType, bars, ticks, level2, verbose), TaskCreationOptions.LongRunning));

                foreach (var task in tasks)
                    task.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        static void WorkingThread(string address, string login, string password, int port, DateTime timestamp, int count, string symbol, string periodicity, PriceType priceType, bool bars, bool ticks, bool level2, bool verbose)
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
                        RequestTicks(client, timestamp, count, symbol, true, verbose);

                    if (ticks)
                        RequestTicks(client, timestamp, count, symbol, false, verbose);

                    if (bars)
                        RequestBars(client, timestamp, count, symbol, periodicity, priceType, verbose);

                    // Logout
                    client.Logout("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        static void RequestTicks(QuoteHistoryClient client, DateTime timestamp, int count, string symbol, bool level2, bool verbose)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool backward = count < 0;
            count = Math.Abs(count);

            List<Tick> result;
            for (int i = 0; i < count / 1000; i++)
            {
                // Request for the bars history
                result = client.QueryQuoteHistoryTicks(timestamp, backward ? -1000 : 1000, symbol, level2);
                if (result.Count > 0)
                    timestamp = backward ? result[result.Count - 1].Id.Time : result[0].Id.Time;
                if (verbose)
                {
                    foreach (var tick in result)
                        Console.WriteLine(tick);
                }
            }

            int remain = count % 1000;
            if (remain > 0)
            {
                result = client.QueryQuoteHistoryTicks(timestamp, backward ? -remain : remain, symbol, level2);
                if (verbose)
                {
                    foreach (var tick in result)
                        Console.WriteLine(tick);
                }
            }

            long elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed = {elapsed}ms");
            Console.WriteLine($"Throughput = {((double)count / (double)elapsed) * 1000.0:0.#####} ticks per second");
        }

        static void RequestBars(QuoteHistoryClient client, DateTime timestamp, int count, string symbol, string periodicity, PriceType priceType, bool verbose)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            bool backward = count < 0;
            count = Math.Abs(count);

            List<Bar> result;
            for (int i = 0; i < count/5000; i++)
            {
                // Request for the bars history
                result = client.QueryQuoteHistoryBars(timestamp, backward ? -5000 : 5000, symbol, periodicity, priceType);
                if (result.Count > 0)
                    timestamp = backward ? result[result.Count - 1].Time : result[0].Time;
                if (verbose)
                {
                    foreach (var bar in result)
                        Console.WriteLine(bar);
                }
            }

            int remain = count%5000;
            if (remain > 0)
            {
                result = client.QueryQuoteHistoryBars(timestamp, backward ? -remain : remain, symbol, periodicity, priceType);
                if (verbose)
                {
                    foreach (var bar in result)
                        Console.WriteLine(bar);
                }
            }

            long elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed = {elapsed}ms");
            Console.WriteLine($"Throughput = {((double)count/(double)elapsed)*1000.0:0.#####} bars per second");
        }
    }
}
