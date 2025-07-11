using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;

namespace DoomFontHelper.Utils
{
    public class DDSUtils
    {
        public static Bitmap DdsToBitmap(DdsFile ddsFile)
        {
            var d = new BcDecoder();

            var colors = d.Decode(ddsFile);

            int w = (int)ddsFile.Faces[0].Width;
            int h = (int)ddsFile.Faces[0].Height;
            var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var rgba = colors[y * w + x];
                    var clr = Color.FromArgb(rgba.a, rgba.r, rgba.g, rgba.b);
                    bitmap.SetPixel(x, y, clr);
                }
            }

            return bitmap;
        }

        public static Bitmap DdsToBitmap(byte[] ddsBytes)
        {
            var d = new BcDecoder();
            var dds = DdsFile.Load(new MemoryStream(ddsBytes));
            var colors = d.Decode(dds);

            int w = (int)dds.Faces[0].Width;
            int h = (int)dds.Faces[0].Height;
            var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var rgba = colors[y * w + x];
                    var clr = Color.FromArgb(rgba.a, rgba.r, rgba.g, rgba.b);
                    bitmap.SetPixel(x, y, clr);
                }
            }

            return bitmap;
        }

        public static void SaveDds(DdsFile ddsFile, string outputPath)
        {
            using (var fs = new FileStream(outputPath, FileMode.Create))
            {
                ddsFile.Write(fs);
            }
        }

        public static DdsFile ConvertPngToDds(string pngImagePath)
        {
            if (!File.Exists(pngImagePath))
            {
                throw new FileNotFoundException($"File not found: {pngImagePath}");
            }

            using var bitmap = new Bitmap(pngImagePath);
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixelData = TextureConverter.ConvertPngToRGBA8(pngImagePath);
            var header = DdsHeader.InitializeUncompressed(width, height, DxgiFormat.DxgiFormatR8G8B8A8Unorm);
            var ddsFile = new DdsFile(header);

            var face = new DdsFace((uint)width, (uint)height, (uint)pixelData.Length, 1);
            var surface = new DdsMipMap(pixelData, (uint)width, (uint)height);

            face.MipMaps[0] = surface;
            ddsFile.Faces.Add(face);

            return ddsFile;
        }
    }
}
