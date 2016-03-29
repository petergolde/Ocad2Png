using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Ocad2Png
{
    class Converter
    {
        Options options;
        TextWriter console;
        Map map;
        int width, height;
        RectangleF bounds;
        string inputFile;

        public const int DEFAULT_SIZE = 4096;

        public Converter(Options options, TextWriter console)
        {
            this.options = options;
            this.console = console;
        }

        public bool Convert(string inputFile)
        {
            this.inputFile = inputFile;

            if (!LoadMap())
                return false;

            ReportMissingFonts();
            DetermineBounds();
            DetermineBitmapSize();

            string outputFile = GetOutputFileName();
            bool success = Render(outputFile);
            if (success)
                console.WriteLine("Successfully created \"{0}\".", outputFile);

            return success;
        }

        private string GetOutputFileName()
        {
            if (options.OutputFile != null)
                return options.OutputFile;
            else
                return Path.ChangeExtension(inputFile, ".png");
        }

        private bool Render(string outputFile)
        {
            GDIPlus_Bitmap bitmap;
            GDIPlus_ColorConverter colorConverter = options.RgbColorSpace ? null : new SwopColorConverter();
            using (var graphicsTarget = new GDIPlus_BitmapGraphicsTarget(width, height, false, CmykColor.FromCmyk(0, 0, 0, 0), bounds, true, colorConverter, 1)) {
                RenderOptions renderOpts = new RenderOptions();
                renderOpts.minResolution = Math.Min(bounds.Height / height, bounds.Width / width);
                renderOpts.renderTemplates = options.Templates ? RenderTemplateOption.MapAndTemplates : RenderTemplateOption.MapOnly;
                renderOpts.usePatternBitmaps = false;
                renderOpts.blendOverprintedColors = options.Overprint;

                graphicsTarget.PushAntiAliasing(options.AntiAlias);

                using (map.Read()) {
                    map.Draw(graphicsTarget, bounds, renderOpts, null);
                }

                graphicsTarget.PopAntiAliasing();

                bitmap = (GDIPlus_Bitmap) graphicsTarget.FinishBitmap();
            }

            bitmap.Bitmap.Save(outputFile, ImageFormat.Png);
            bitmap.Dispose();
            return true;
        }
    
        private void DetermineBitmapSize()
        {
            int? width = options.Width;
            int? height = options.Height;

            if (!width.HasValue && !height.HasValue) {
                if (bounds.Width > bounds.Height)
                    width = DEFAULT_SIZE;
                else
                    height = DEFAULT_SIZE;
            }

            if (!width.HasValue) {
                width = (int)Math.Ceiling(height.Value * bounds.Width / bounds.Height);
            }
            else if (!height.HasValue)
                height = (int)Math.Ceiling(width.Value * bounds.Height / bounds.Width);

            this.width = width.Value;
            this.height = height.Value;
        }

        private void DetermineBounds()
        {
            // TODO: Allow command line option to override.
            using (map.Read()) {
                bounds = map.Bounds;
            }

            // zero width or height will cause problems.
            if (bounds.Height <= 0.01F)
                bounds.Height += 1F;
            else if (bounds.Width <= 0.01F)
                bounds.Width += 1F;
        }

        void ReportMissingFonts()
        {
            foreach (string fontName in map.MissingFonts) {
                console.WriteLine("Warning: font \"{0}\" is not present on this computer.", fontName);
            }
        }

        bool LoadMap()
        {
            if (!File.Exists(inputFile)) {
                console.WriteLine("Input file \"{0}\" does not exist.", inputFile);
                return false;
            }

            map = new Map(new GDIPlus_TextMetrics(), new FileLoader(Path.GetDirectoryName(inputFile), console));

            try {
                InputOutput.ReadFile(inputFile, map);
            }
            catch (OcadFileFormatException e) {
                console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}
