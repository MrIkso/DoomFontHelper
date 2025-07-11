using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DoomFontHelper.Utils
{
    public class TextureConverter
    {
        /// <summary>
        /// Сonverts 8-bit Alpha (A8) to 32-bit RGBA8.
        ///  R, G, B channels are set to 255.
        ///  </summary>
        public static byte[] ConvertA8ToRgba8(byte[] a8Data, int width, int height)
        {
            int pixelCount = width * height;
            if (a8Data.Length != pixelCount)
            {
                throw new ArgumentException("Incorrect input data size for A8.");
            }

            byte[] rgba8Data = new byte[pixelCount * 4];
            int a8Index = 0;
            for (int i = 0; i < rgba8Data.Length; i += 4)
            {
                byte alpha = a8Data[a8Index++];
                rgba8Data[i] = 255;     // Red
                rgba8Data[i + 1] = 255; // Green
                rgba8Data[i + 2] = 255; // Blue
                rgba8Data[i + 3] = alpha;  // Alpha
            }
            return rgba8Data;
        }

        /// <summary>
        /// Converts 16-bit Luminance-Alpha (LA8) to 32-bit RGBA8.
        /// R, G, B channels are set to the Luminance value.
        /// </summary>
        public static byte[] ConvertLa8ToRgba8(byte[] la8Data, int width, int height)
        {
            int pixelCount = width * height;
            if (la8Data.Length != pixelCount * 2)
            {
                throw new ArgumentException("Incorrect input data size for LA8.");
            }

            byte[] rgba8Data = new byte[pixelCount * 4];
            int laIndex = 0;
            for (int i = 0; i < rgba8Data.Length; i += 4)
            {
                byte luminance = la8Data[laIndex++];
                byte alpha = la8Data[laIndex++];
                rgba8Data[i] = luminance; // Red
                rgba8Data[i + 1] = luminance; // Green
                rgba8Data[i + 2] = luminance; // Blue
                rgba8Data[i + 3] = alpha;     // Alpha
            }
            return rgba8Data;
        }


        public static Bitmap ConvertA8ToBitmap(byte[] a8Data, int width, int height)
        {
            int pixelCount = width * height;
            if (a8Data.Length != pixelCount)
            {
                throw new ArgumentException("Incorrect input data size for A8.");
            }
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            int bytesPerPixel = 4;
            byte[] bgraData = new byte[width * height * bytesPerPixel];
            for (int i = 0; i < pixelCount; i++)
            {
                byte alpha = a8Data[i];
                int offset = i * bytesPerPixel;
                bgraData[offset] = 255; // Blue
                bgraData[offset + 1] = 255; // Green
                bgraData[offset + 2] = 255; // Red
                bgraData[offset + 3] = alpha; // Alpha
            }
            Marshal.Copy(bgraData, 0, bmpData.Scan0, bgraData.Length);
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }

        public static void SaveA8AsPng(byte[] a8Data, int width, int height, string outputPath)
        {
            ConvertA8ToBitmap (a8Data, width, height).Save(outputPath, ImageFormat.Png);
        }

        public static byte[] ConvertPngToAlpha8(string pngFilePath)
        {
            if (!File.Exists(pngFilePath))
            {
                throw new FileNotFoundException("PNG файл не знайдено.", pngFilePath);
            }

            using (var bitmap = new Bitmap(pngFilePath))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                byte[] alpha8Data = new byte[width * height];

                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    byte[] rowBuffer = new byte[bmpData.Stride];
                    IntPtr currentRowPtr = bmpData.Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        Marshal.Copy(currentRowPtr, rowBuffer, 0, bmpData.Stride);

                        for (int x = 0; x < width; x++)
                        {
                            int sourceIndex = x * 4; 
                            byte alpha = rowBuffer[sourceIndex + 3];
                            int destIndex = y * width + x;
                            alpha8Data[destIndex] = alpha;
                        }
                        currentRowPtr += bmpData.Stride;
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                return alpha8Data;
            }
        }

        // reruns RGBA8 pixel data from PNG file
        public static byte[] ConvertPngToRGBA8(string texturePath)
        {
            using (var bitmap = new Bitmap(texturePath))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                byte[] textureData = new byte[width * height * 4];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        int index = y * width * 4 + x * 4;

                        textureData[index] = pixel.R;
                        textureData[index + 1] = pixel.G;
                        textureData[index + 2] = pixel.B;
                        textureData[index + 3] = pixel.A;
                    }
                }

                return textureData;
            }
        }   
    }
}
