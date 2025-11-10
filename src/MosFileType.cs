using ii.InfinityEngine.Files;
using ii.InfinityEngine.Readers;
using PaintDotNet;
using PaintDotNet.Drawing;
using PaintDotNet.Rendering;
using System;
using System.Drawing;
using System.IO;

namespace MosFileTypePlugin
{
    [PluginSupportInfo(typeof(FileTypeFactory), DisplayName = "MOS File V1 (Infinity Engine)")]
    public class MosFileType : FileType
    {
        public MosFileType()
            : base(
                "MOS File",
                new FileTypeOptions
                {
                    LoadExtensions = [".mos"],
                    SaveExtensions = [".mos"]
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

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            try
            {
                IRenderer<ColorBgra> renderer = input.CreateRenderer();
                renderer.Render(scratchSurface);

                var bitmap = scratchSurface.ToGdipBitmap();

                try
                {
                    string tempFile = Path.GetTempFileName();
                    try
                    {
                        var mosConverter = new ii.InfinityEngine.MosConverter();
                        mosConverter.ToMos(bitmap, tempFile);

                        using FileStream tempStream = File.OpenRead(tempFile);
                        tempStream.CopyTo(output);
                    }
                    finally
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                    }
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving MOS file: {ex.Message}", ex);
            }
        }
    }

    public class FileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances() => [new MosFileType()];
    }
}