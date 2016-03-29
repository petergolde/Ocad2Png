using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SD = System.Drawing;
using SWM = System.Windows.Media;

namespace Ocad2Png
{
    class SwopColorConverter: GDIPlus_ColorConverter
    {
        public readonly static string SwopFileName;
        public readonly static Uri SwopUri;
        private static Dictionary<CmykColor, SD.Color> cmykToColor = new Dictionary<CmykColor, SD.Color>();

        static SwopColorConverter()
        {
            SwopFileName = GetFileInAppDirectory("USWebCoatedSWOP.icc");
            SwopUri = new Uri(SwopFileName);
        }

        static string GetFileInAppDirectory(string filename)
        {
            // Using Application.StartupPath would be
            // simpler and probably faster, but doesn't work with NUnit.
            string codebase = typeof(SwopColorConverter).Assembly.CodeBase;
            Uri uri = new Uri(codebase);
            string appPath = Path.GetDirectoryName(uri.LocalPath);

            // Create the core objects needed for the application to run.
            return Path.Combine(appPath, filename);
        }


        public static SD.Color CmykToRgbColor(CmykColor cmykColor)
        {
            SD.Color result;

            if (cmykColor.Cyan == 0 && cmykColor.Magenta == 0 && cmykColor.Yellow == 0 && cmykColor.Black == 0) {
                // The default mapping doesn't quite map white to pure white.
                if (cmykColor.Alpha == 1)
                    return SD.Color.White;
                else
                    return SD.Color.FromArgb((byte)Math.Round(cmykColor.Alpha * 255), SD.Color.White);
            }

            if (!cmykToColor.TryGetValue(cmykColor, out result)) {
                float[] colorValues = { cmykColor.Cyan, cmykColor.Magenta, cmykColor.Yellow, cmykColor.Black };
                SWM.Color color = SWM.Color.FromValues(colorValues, SwopUri);
                result = SD.Color.FromArgb((byte)Math.Round(cmykColor.Alpha * 255), color.R, color.G, color.B);
                lock (cmykToColor) {
                    cmykToColor[cmykColor] = result;
                }
            }

            return result;
        }


        public override SD.Color ToColor(CmykColor cmykColor)
        {
            return CmykToRgbColor(cmykColor);
        }

    }
}
