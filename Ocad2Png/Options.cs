using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Ocad2Png
{
    class Options
    {
        [Option('w', "width", HelpText="Set the width of the output bitmap (default is 4096)")]
        public int? Width { get; set; }

        [Option('h', "height", HelpText = "Set the height of the output (default is 4096)")]
        public int? Height { get; set; }

        [Option('o', "output", HelpText = "Sets the name of the output file (default is input file with png extension)")]
        public string OutputFile { get; set; }

        [Option('a', "antialias", HelpText = "Use anti-aliasing")]
        public bool AntiAlias { get; set; }

        [ValueOption(0)]
        public string InputFile { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText() {
                Heading = new HeadingInfo("Ocad2Png", "1.0"),
                Copyright = "by Peter Golde",
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Usage: Ocad2Png [options] <ocad-file>");
            help.AddOptions(this);
            return help;
        }
    }
}
