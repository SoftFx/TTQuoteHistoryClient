using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var options = new OptionSet()
            {
                { "a|address=", v => address = v },
                { "p|port=", v => port = int.Parse(v) },
                { "h|?|help",   v => help = v != null }
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
                    client.Connect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
