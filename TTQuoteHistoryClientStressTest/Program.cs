﻿using System;
using System.Threading;
using NDesk.Options;
using TTQuoteHistoryClient;

namespace TTQuoteHistoryClientStressTest
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
            int threadCount = 10;

            var options = new OptionSet()
            {
                { "a|address=", v => address = v },
                { "p|port=", v => port = int.Parse(v) },
                { "h|?|help",   v => help = v != null },
                { "t|timestamp=", v => timestamp = DateTime.Parse(v) },
                { "c|count=", v => count = int.Parse(v) },
                { "s|symobl=", v => symbol = v },
                { "d|periodicity=", v => periodicity = v },
                { "q|threads=", v => threadCount = int.Parse(v) },
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
                Console.Write("TTQuoteHistoryClientStressTest: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `TTQuoteHistoryClientStressTest --help' for more information.");
                return;
            }

            if (help)
            {
                Console.WriteLine("TTQuoteHistoryClientStressTest usage:");
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

                    ThreadParams threadParams = new ThreadParams();
                    threadParams.client = client;
                    threadParams.timestamp = timestamp;
                    threadParams.count = count;
                    threadParams.symbol = symbol;
                    threadParams.periodicity = periodicity;
                    threadParams.priceType = priceType;
                    threadParams.bars = bars;
                    threadParams.ticks = ticks;
                    threadParams.level2 = level2;

                    Thread[] threads = new Thread[threadCount];

                    for (int i = 0; i < threads.Length; ++ i)
                        threads[i] = new Thread(new ParameterizedThreadStart(Program.Thread));

                    stop_ = false;

                    for (int i = 0; i < threads.Length; ++i)
                    {                        
                        threads[i].Start(threadParams);
//                        System.Threading.Thread.Sleep(10);
                    }

                    Console.WriteLine("Press any key to stop");
                    Console.ReadKey();

                    stop_ = true;

                    for (int i = 0; i < threads.Length; ++i)
                        threads[i].Join();

                    // Disconnect to the server
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        class ThreadParams
        {
            public QuoteHistoryClient client;
            public DateTime timestamp;
            public int count;
            public string symbol;
            public string periodicity;
            public PriceType priceType;
            public bool bars;
            public bool ticks;
            public bool level2;
        }

        static void Thread(object arg)
        {
            ThreadParams threadParams = (ThreadParams) arg;

            while (!stop_)
            {
                // Request the server
                if (threadParams.level2)
                {
                    // Request for the level2 history
                    var result = threadParams.client.QueryQuoteHistoryTicks(threadParams.timestamp, threadParams.count, threadParams.symbol, true);
                }

                if (threadParams.ticks)
                {
                    // Request for the ticks history
                    var result = threadParams.client.QueryQuoteHistoryTicks(threadParams.timestamp, threadParams.count, threadParams.symbol, false);
                }

                if (threadParams.bars)
                {
                    // Request for the bars history
                    var result = threadParams.client.QueryQuoteHistoryBars(threadParams.timestamp, threadParams.count, threadParams.symbol, threadParams.periodicity, threadParams.priceType);
                }
            }
        }

        static bool stop_;
    }
}
