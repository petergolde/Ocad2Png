using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PurplePen.Graphics2D;

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

            if (options.SameAreaForAll && options.InputFiles.Count >= 2 && !options.AreaToDraw.HasValue) {
                // Get areas from all input files and merge.
                options.AreaToDraw = MergeBounds(options.InputFiles);
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

        private static RectangleF? MergeBounds(IList<string> inputFiles)
        {
            RectangleF? merged = null;
            foreach (string fileName in inputFiles) {
                RectangleF? bounds = Converter.GetMapBounds(fileName);
                if (!merged.HasValue)
                    merged = bounds;
                else if (bounds.HasValue)
                    merged = RectangleF.Union(merged.Value, bounds.Value);
            }

            return merged;
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

            options.AreaToDraw = null;
            if (options.Area != null) {
                float x1, y1, x2, y2;
                string[] values = options.Area.Split(new char[] { ',' }, StringSplitOptions.None);
                if (values.Length != 4 || !TryParseCoord(values[0], out x1) || !TryParseCoord(values[1], out y1) ||
                    !TryParseCoord(values[2], out x2) || !TryParseCoord(values[3], out y2)) {
                    Console.WriteLine("Invalid syntax for --area; must specify 4 numbers separated by commas.");
                    return false;
                }

                options.AreaToDraw = Geometry.RectFromPoints(x1, y1, x2, y2);
            }

            return true;
        }

        // Parse a coordinate with the invariant culture.
        private static bool TryParseCoord(string s, out float f)
        {
            return float.TryParse(s, NumberStyles.Integer | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out f);
        }
    }
}
