using ii.InfinityEngine;
using PaintDotNet;
using PaintDotNet.Drawing;
using PaintDotNet.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace TisFileTypePlugin
{
    [PluginSupportInfo(typeof(FileTypeFactory), DisplayName = "TIS File (Infinity Engine)")]
    public class TisFileType : FileType
    {
        public TisFileType()
            : base(
                "TIS File",
                new FileTypeOptions
                {
                    LoadExtensions = [".tis"],
                    SaveExtensions = [".tis"]
                })
        {
        }

        protected override Document OnLoad(Stream input)
        {
            try
            {
                // TIS files do not store their dimensions. We can't use a filename lookup as per ieShellEx as
                // paint.net doesn't give us the filename. Intead we calculate a hash and lookup based on that
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    input.CopyTo(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var fileHash = CalculateHash(fileData);
                var relevantAreaData = LoadAreaDataByHash(fileHash);
                if (relevantAreaData == null || relevantAreaData.Width == default || relevantAreaData.Height == default)
                {
                    throw new Exception("Unable to determine TIS dimensions");
                }

                // Pixel dimensions
                var width = relevantAreaData.Width;
                var height = relevantAreaData.Height;

                // iiInfinityEngine needs a file at the moment, so we'll create a temp one
                var tempFile = Path.GetTempFileName();
                try
                {
                    File.WriteAllBytes(tempFile, fileData);

                    var image = new TisConverter().Convert(tempFile, width, height) ?? throw new Exception("Failed to decode TIS file - could not extract image data");
                    Bitmap bitmap = new Bitmap(image);
                    Document doc = new Document(bitmap.Width, bitmap.Height);
                    Surface surface = Surface.CopyFromBitmap(bitmap);

                    var layer = Layer.CreateBackgroundLayer(doc.Size);
                    layer.Surface.CopySurface(surface);

                    doc.Layers.Clear();
                    doc.Layers.Add(layer);

                    return doc;
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading TIS file: {ex.Message}", ex);
            }
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            try
            {
                IRenderer<ColorBgra> renderer = input.CreateRenderer();
                renderer.Render(scratchSurface);

                var bitmap = scratchSurface.ToGdipBitmap();

                try
                {
                    // TODO: TIS Saving
                    throw new NotImplementedException("TIS file saving is not currently implemented.");
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving TIS file: {ex.Message}", ex);
            }
        }

        private string CalculateHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return Convert.ToHexStringLower(hashBytes);
        }

        private AreaData LoadAreaDataByHash(string hash)
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                var path = Environment.GetEnvironmentVariable("iiIEPaintNetPlugins", EnvironmentVariableTarget.Machine);

                if (string.IsNullOrEmpty(path))
                {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "areadatasplugin.xml");
                }

                if (!File.Exists(path))
                {
                    return null;
                }


                // We need to use manual XML parsing, if we use anything more sensible we get errors about collectible/non-collectible assemblies
                XDocument doc;
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    doc = XDocument.Load(fileStream);
                }
                
                List<AreaData> areaDatas = new List<AreaData>();
                var root = doc.Root;
                if (root != null)
                {
                    foreach (var areaDataElement in root.Elements("AreaData"))
                    {
                        var areaData = new AreaData
                        {
                            Game = areaDataElement.Element("Game")?.Value,
                            Filename = areaDataElement.Element("Filename")?.Value,
                            Filesize = long.TryParse(areaDataElement.Element("Filesize")?.Value, out long filesize) ? filesize : 0,
                            Hash = areaDataElement.Element("Hash")?.Value,
                            Width = int.TryParse(areaDataElement.Element("Width")?.Value, out int width) ? width : 0,
                            Height = int.TryParse(areaDataElement.Element("Height")?.Value, out int height) ? height : 0
                        };
                        areaDatas.Add(areaData);
                    }
                }

                if (areaDatas == null || areaDatas.Count == 0)
                {
                    return null;
                }
                var relevantAreaData = areaDatas.FirstOrDefault(w => w.Hash?.Equals(hash, StringComparison.OrdinalIgnoreCase) == true);
                return relevantAreaData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading TIS dimensions: {ex.Message}", ex);
            }
        }
    }

    public class FileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances() => [new TisFileType()];
    }

    public class AreaData
    {
        public string Game { get; set; }
        public string Filename { get; set; }
        public long Filesize { get; set; }
        public string Hash { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public AreaData() { }

        public AreaData(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}