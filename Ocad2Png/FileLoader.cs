using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Ocad2Png
{
    class FileLoader: IFileLoader
    {
        private string basePath;
        TextWriter console;

        public FileLoader(string basePath, TextWriter console)
        {
            this.basePath = basePath;
            this.console = console;
        }

        public IGraphicsBitmap LoadBitmap(string path, bool isTemplate)
        {
            string filePath = SearchForFile(path);
            if (filePath == null) {
                if (isTemplate)
                    console.WriteLine("Warning: template file \"{0}\" not found.", path);
                else
                    console.WriteLine("Warning: bitmap file \"{0}\" not found.", path);

                return null;
            }

            Bitmap bitmap = Image.FromFile(path) as Bitmap;
            if (bitmap == null)
                return null;

            return new GDIPlus_Bitmap(bitmap);
        }

        public FileKind CheckFileKind(string path)
        {
            string filePath = SearchForFile(path);
            if (filePath == null) {
                console.WriteLine("Warning: template file \"{0}\" not found.", path);
                return FileKind.DoesntExist;
            }

            try {
                using (Stream s = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    if (InputOutput.IsOcadFile(s))
                        return FileKind.OcadFile;
                    else 
                        return FileKind.OtherFile;
                }
            }
            catch (IOException) {
                console.WriteLine("Warning: template file \"{0}\" could not be opened.", path);
                return FileKind.NotReadable;
            }
            catch (UnauthorizedAccessException) {
                console.WriteLine("Warning: template file \"{0}\" could not be opened.", path);
                return FileKind.NotReadable;
            }
        }

        public Map LoadMap(string path, Map referencingMap)
        {
            string filePath = SearchForFile(path);
            if (filePath == null) {
                console.WriteLine("Warning: template file \"{0}\" not found.", path);
                return null;
            }

            Map newMap = new Map(referencingMap.TextMetricsProvider, new FileLoader(Path.GetDirectoryName(filePath), console));

            InputOutput.ReadFile(filePath, newMap);
            return newMap;
        }

        private string SearchForFile(string path)
        {
            try {
                if (File.Exists(path))
                    return path;

                if (basePath != null) {
                    string baseName = Path.GetFileName(path);
                    string revisedPath = Path.Combine(basePath, baseName);
                    if (File.Exists(revisedPath))
                        return revisedPath;
                }
            }
            catch (ArgumentException) {
                // If the path has invalid characters in it, we get here.
                return null;
            }

            return null;
        }
    }

}
