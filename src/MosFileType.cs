using ii.InfinityEngine.Files;
using ii.InfinityEngine.Readers;
using PaintDotNet;
using System;
using System.Drawing;
using System.IO;

namespace MosFileTypePlugin
{
    [PluginSupportInfo(typeof(FileTypeFactory), DisplayName = "MOS File (Baldur's Gate)")]
    public class MosFileType : FileType
    {
        public MosFileType()
            : base(
                "MOS File",
                new FileTypeOptions
                {
                    LoadExtensions = new[] { ".mos" }
                })
        {
        }

        protected override Document OnLoad(Stream input)
        {
            try
            {
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    input.CopyTo(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var mosFile = new MosFileBinaryReader();

                using var stream = new MemoryStream(fileData);
                MosFile m = mosFile.Read(stream);

                Image image = m.Image;

                if (image == null)
                {
                    throw new Exception("Failed to decode MOS file - could not extract image data");
                }

                Bitmap bitmap = new Bitmap(image);

                Document doc = new Document(bitmap.Width, bitmap.Height);

                Surface surface = Surface.CopyFromBitmap(bitmap);

                var layer = Layer.CreateBackgroundLayer(doc.Size);
                layer.Surface.CopySurface(surface);

                doc.Layers.Clear();
                doc.Layers.Add(layer);

                return doc;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading MOS file: {ex.Message}", ex);
            }
        }
    }

    public class FileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new FileType[] { new MosFileType() };
        }
    }
}