using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ocad2Png
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = new Options();

            if (! CommandLine.Parser.Default.ParseArguments(args, options) || options.InputFile == null) {
                // Display the default usage information
                Console.WriteLine(options.GetUsage());
                return 2;
            }

            try {
                Converter converter = new Converter(options, Console.Out);
                bool success = converter.Convert();

                return success ? 0 : 1;
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: {0}", e.Message);
                return 1;
            }
        }
    }
}
