using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Ocad2Png
{
    class Options
    {
        [Option('w', "width", HelpText="Set the width of the output bitmap (default is 4096).")]
        public int? Width { get; set; }

        [Option('h', "height", HelpText = "Set the height of the output (default is 4096).")]
        public int? Height { get; set; }

        [Option('o', "output", HelpText = "Sets the name of the output file (default is input file with png extension).")]
        public string OutputFile { get; set; }

        [Option('p', "pixelate", HelpText = "Disable anti-aliasing; draw in whole pixels only.")]
        public bool Pixelate { get; set; }

        [Option('a', "area", HelpText = "Area of the map to draw. \"-a 10,-20,110,250\" will draw the area from (10,-20) to (110,250). The default is to draw the whole map.")]
        public string Area { get; set; }

        [Option("samearea", HelpText = "Draw same area for all maps. Only has an effect if there are more than one input file and --area is not used. Useful for comparing versions of the same map.")]
        public bool SameAreaForAll { get; set; }

        [Option("templates", HelpText = "Draw templates (if any)")]
        public bool Templates { get; set; }

        [Option("overprint", HelpText = "Use overprint effect on color table entries marked as overprint.")]
        public bool Overprint { get; set; }

        [Option("rgb", HelpText = "Use RGB color space (CMYK color space is the default).")]
        public bool RgbColorSpace { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> InputFiles { get; set; }

        public RectangleF? AreaToDraw { get; set; } // Parsed from area.

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText() {
                Heading = new HeadingInfo("Ocad2Png", "1.0"),
                Copyright = "by Peter Golde (peter@golde.org)", 
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("\r\nUsage: Ocad2Png [options] <ocad-file>...");
            help.AddOptions(this);
            return help;
        }
    }
}
