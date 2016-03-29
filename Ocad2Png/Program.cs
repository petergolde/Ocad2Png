using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ocad2Png
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = new Options();

            if (! CommandLine.Parser.Default.ParseArguments(args, options)) {
                // Display the default usage information
                return 2;
            }

            if (options.InputFiles.Count == 0) {
                Console.WriteLine(options.GetUsage());
                return 2;
            }

            options.InputFiles = ExpandWildcards(options.InputFiles);

            if (!ValidateOptions(options)) {
                return 2;
            }

            try {
                bool success = true;

                Converter converter = new Converter(options, Console.Out);

                foreach (string inputFile in options.InputFiles) {
                    success = success & converter.Convert(inputFile);
                }

                return success ? 0 : 1;
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: {0}", e.Message);
                return 1;
            }
        }

        static IList<string> ExpandWildcards(IList<string> fileList)
        {
            List<string> output = new List<string>();
            foreach (string s in fileList) {
                if (HasWildcard(s)) {
                    output.AddRange(ExpandWildcard(s));
                }
                else {
                    output.Add(s);
                }
            }

            return output;
        }

        private static IEnumerable<string> ExpandWildcard(string s)
        {
            var dirPart = Path.GetDirectoryName(s);
            if (dirPart.Length == 0)
                dirPart = ".";

            var filePart = Path.GetFileName(s);

            return Directory.EnumerateFiles(dirPart, filePart, SearchOption.TopDirectoryOnly);
        }

        private static bool HasWildcard(string s)
        {
            return s.Contains('?') || s.Contains('*');
        }

        static bool ValidateOptions(Options options)
        {
            if (options.OutputFile != null && options.InputFiles.Count >= 2) {
                Console.WriteLine("Cannot use --output option with more than one input file.");
                return false;
            }

            return true;
        }
    }
}
