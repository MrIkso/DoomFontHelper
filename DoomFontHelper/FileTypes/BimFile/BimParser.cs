using BCnEncoder.Shared.ImageFiles;
using DoomFontHelper.Helpers.IO;
using DoomFontHelper.Utils;

namespace DoomFontHelper.FileTypes.BimFile
{
    public class BimParser
    {
        private static readonly int Signature = 0x074d4942; // BIM and version on Little Endian

        public BimImage BimImageFile { get; set; } = new BimImage();

        public BimParser(string filePath)
        {
            using Stream bimStream = File.OpenRead(filePath);
            var br = new ExtendedBinaryReader(bimStream);
            Load(br);
        }

        public void Load(ExtendedBinaryReader reader)
        {
            var file = new BimImage();

            // Read header magic and version
            file.Header.Timestamp = reader.ReadUInt32Reverse();
            file.Header.MagicAndVersion = reader.ReadUInt32Reverse();
            if (file.Header.MagicAndVersion != Signature)
            {
                throw new InvalidDataException($"Invalid BIM signature or version. Expected {Signature:X}, got {file.Header.MagicAndVersion:X}.");
            }

            file.Header.TextureType = reader.ReadInt32Reverse();
            file.Header.PixelWidth = reader.ReadInt32Reverse();
            file.Header.PixelHeight = reader.ReadInt32Reverse();
            file.Header.Depth = reader.ReadInt32Reverse();
            file.Header.MipCount = reader.ReadInt32Reverse();
            file.Header.BoolIsStreamed = reader.ReadByte();
            file.Header.TextureFormat = reader.ReadInt32Reverse();
            file.Header.TextureMaterialKind = reader.ReadInt32Reverse();
            file.Header.StreamDBMipCount = reader.ReadInt32Reverse();
            file.Header.UnknownByte = reader.ReadByte();

            // Read mipmap information and pixel data
            var mipMaps = new List<BimageMipmap>();
            for (int i = 0; i < file.Header.MipCount; i++)
            {
                BimageMipmap mip = new BimageMipmap();

                mip.MipLevelIndex = reader.ReadUInt32Reverse();
                mip.Unknown1 = reader.ReadUInt32Reverse();
                mip.MipPixelWidth = reader.ReadInt32Reverse();
                mip.MipPixelHeight = reader.ReadInt32Reverse();
                mip.CompressedSize = reader.ReadInt32Reverse();
                mip.PixelData = reader.ReadBytes(mip.CompressedSize);

                mipMaps.Add(mip);
            }

            file.MipMaps.AddRange(mipMaps);
            BimImageFile = file;
        }

        public void Write(BimImage bimImageFile, string filePath)
        {
            using Stream stream = File.Create(filePath);
            using ExtendedBinaryWriter writer = new ExtendedBinaryWriter(stream);
            // Write header
            writer.WriteReverse(bimImageFile.Header.Timestamp);
            writer.WriteReverse(Signature);
            writer.WriteReverse(bimImageFile.Header.TextureType);
            writer.WriteReverse(bimImageFile.Header.PixelWidth);
            writer.WriteReverse(bimImageFile.Header.PixelHeight);
            writer.WriteReverse(bimImageFile.Header.Depth);
            writer.WriteReverse(bimImageFile.Header.MipCount);
            writer.Write(bimImageFile.Header.BoolIsStreamed);
            writer.WriteReverse(bimImageFile.Header.TextureFormat);
            writer.WriteReverse(bimImageFile.Header.TextureMaterialKind);
            writer.WriteReverse(bimImageFile.Header.StreamDBMipCount);
            writer.Write(bimImageFile.Header.UnknownByte);
            // Write mipmaps
            foreach (var mip in bimImageFile.MipMaps)
            {
                writer.WriteReverse(mip.MipLevelIndex);
                writer.WriteReverse(mip.Unknown1);
                writer.WriteReverse(mip.MipPixelWidth);
                writer.WriteReverse(mip.MipPixelHeight);
                writer.WriteReverse(mip.CompressedSize);
            }

            foreach (var mip in bimImageFile.MipMaps)
            {
                if (mip.PixelData != null && mip.PixelData.Length > 0)
                {
                    writer.Write(mip.PixelData);
                }
                else if (mip.CompressedSize > 0)
                {
                    writer.Write(new byte[mip.CompressedSize]);
                }
            }
        }

        public DdsFile ConvertToDds(BimImage? bimImage)
        {
            if (bimImage == null)
            {
                throw new ArgumentException("Invalid bimImage information provided.");
            }

            // Create a deep copy of the BimImage to avoid modifying the original object
            var info = bimImage.Header;
            var copiedBimImage = new BimImage
            {
                Header = new BimageHeader
                {
                    Timestamp = info.Timestamp,
                    MagicAndVersion = info.MagicAndVersion,
                    TextureType = info.TextureType,
                    PixelWidth = info.PixelWidth,
                    PixelHeight = info.PixelHeight,
                    Depth = info.Depth,
                    MipCount = info.MipCount,
                    BoolIsStreamed = info.BoolIsStreamed,
                    TextureFormat = info.TextureFormat,
                    TextureMaterialKind = info.TextureMaterialKind,
                    StreamDBMipCount = info.StreamDBMipCount,
                    UnknownByte = info.UnknownByte
                },
                MipMaps = [.. bimImage.MipMaps.Select(mip => new BimageMipmap
                {
                    MipLevelIndex = mip.MipLevelIndex,
                    Unknown1 = mip.Unknown1,
                    MipPixelWidth = mip.MipPixelWidth,
                    MipPixelHeight = mip.MipPixelHeight,
                    CompressedSize = mip.CompressedSize,
                    PixelData = mip.PixelData != null ? (byte[])mip.PixelData.Clone() : null
                })]
            };

            var finalMips = copiedBimImage.MipMaps;
            var finalFormatId = IdTechImageFormatExtensions.ToImageFormat(info.TextureFormat);

            if (finalFormatId == IdTechImageFormat.ALPHA_8)
            {
                Console.WriteLine($"Converting format ID 5 (Alpha 8-bit) to RGBA8, size might be bigger...");
                finalFormatId = IdTechImageFormat.RGBA8;

                for (int j = 0; j < finalMips.Count; j++)
                {
                    var currentMip = finalMips[j];
                    if (currentMip.PixelData == null)
                    {
                        throw new ArgumentException("Pixel Data Not Found!");
                    }

                    byte[] convertedData = TextureConverter.ConvertA8ToRgba8(currentMip.PixelData, currentMip.MipPixelWidth, currentMip.MipPixelHeight);

                    currentMip.PixelData = convertedData;
                    currentMip.CompressedSize = convertedData.Length;
                    finalMips[j] = currentMip;
                }
            }
            else if (finalFormatId == IdTechImageFormat.L8A8)
            {
                Console.WriteLine($"Converting format ID 6 (Luminance+Alpha) to RGBA8, size might be bigger...");
                finalFormatId = IdTechImageFormat.RGBA8;

                for (int j = 0; j < finalMips.Count; j++)
                {
                    var currentMip = finalMips[j];
                    if (currentMip.PixelData == null)
                    {
                        throw new ArgumentException("Pixel Data Not Found!");
                    }

                    byte[] convertedData = TextureConverter.ConvertLa8ToRgba8(currentMip.PixelData, currentMip.MipPixelWidth, currentMip.MipPixelHeight);

                    currentMip.PixelData = convertedData;
                    currentMip.CompressedSize = convertedData.Length;
                    finalMips[j] = currentMip;
                }
            }

            var dxgiFormat = IdTechImageFormatExtensions.MapIdTechFormatToDxgi((int)finalFormatId);
            bool isCompressed = DxgiFormatExtensions.IsCompressedFormat(dxgiFormat);

            DdsFile ddsFile;
            if (isCompressed)
            {
                var (header, dx10Header) = DdsHeader.InitializeCompressed(info.PixelWidth, info.PixelHeight, dxgiFormat, preferDxt10Header: false);
                ddsFile = new DdsFile(header, dx10Header);
                if (info.MipCount > 1)
                {
                    ddsFile.header.dwFlags |= HeaderFlags.DdsdMipmapcount;
                    ddsFile.header.dwCaps |= HeaderCaps.DdscapsComplex | HeaderCaps.DdscapsMipmap;
                    ddsFile.header.dwPitchOrLinearSize = (uint)info.MipCount;
                }
            }
            else
            {
                var header = DdsHeader.InitializeUncompressed(info.PixelWidth, info.PixelHeight, dxgiFormat);
                ddsFile = new DdsFile(header);
            }

            var mip = copiedBimImage.MipMaps[0];

            if (mip.PixelData == null || mip.PixelData.Length == 0)
            {
                throw new InvalidDataException($"Mip {mip.MipLevelIndex} has no pixel data.");
            }

            var face = new DdsFace((uint)info.PixelWidth, (uint)info.PixelHeight, (uint)mip.PixelData.Length, info.MipCount);
            ddsFile.Faces.Add(face);
            //var surface = new DdsMipMap(mip.PixelData, (uint)mip.MipPixelWidth, (uint)mip.MipPixelHeight);

          //  ddsFile.Faces[0].MipMaps[0] = surface;

            for (var mipCounter = 0; mipCounter < copiedBimImage.MipMaps.Count; mipCounter++)
            {
                var mipMod = copiedBimImage.MipMaps[mipCounter];
                var surface = new DdsMipMap(mipMod.PixelData, (uint)mipMod.MipPixelWidth, (uint)mipMod.MipPixelHeight);

                ddsFile.Faces[0].MipMaps[mipCounter] = surface;
            }

            return ddsFile;
        }

        public void ReplacePngImageInBim(string pngImage, string outputPath)
        {
            if (!File.Exists(pngImage))
            {
                Console.WriteLine($"File not found: {pngImage}");
                return;
            }

            var newDds = DDSUtils.ConvertPngToDds(pngImage);
            DddFileToBim(newDds, outputPath);
        }

        public void ReplaceDDSImageInBim(string ddsFilePath, string outputPath)
        {
            if (!File.Exists(ddsFilePath))
            {
                Console.WriteLine($"File not found: {ddsFilePath}");
                return;
            }
            var newDds = DdsFile.Load(File.OpenRead(ddsFilePath));
            if (newDds.Faces.Count == 0 || newDds.Faces[0].MipMaps.Length == 0)
            {
                throw new ArgumentException("The source DDS file is empty or invalid.");
            }

            DddFileToBim(newDds, outputPath);
        }

        private void DddFileToBim(DdsFile ddsFile, string outputPath)
        {
            var newTextureSurface = ddsFile.Faces[0].MipMaps[0];
            int newWidth = (int)newTextureSurface.Width;
            int newHeight = (int)newTextureSurface.Height;
            DxgiFormat newDxgiFormat = ddsFile.header.ddsPixelFormat.IsDxt10Format
                ? ddsFile.dx10Header.dxgiFormat
                : ddsFile.header.ddsPixelFormat.DxgiFormat;

            int newFormat = IdTechImageFormatExtensions.MapDxgiFormatToIdTech(newDxgiFormat);
            if (newFormat == -1)
            {
                throw new ArgumentException($"Unsupported DDS format: {newDxgiFormat}");
            }

            byte[] newPixelData = newTextureSurface.Data;
            int newDataSize = newPixelData.Length;
            int newMipMapCount = ddsFile.Faces[0].MipMaps.Length;

            BimImageFile.Header.PixelWidth = newWidth;
            BimImageFile.Header.PixelHeight = newHeight;
            BimImageFile.Header.TextureFormat = newFormat;

            BimImageFile.MipMaps[0].PixelData = newPixelData;
            BimImageFile.MipMaps[0].MipPixelHeight = newHeight;
            BimImageFile.MipMaps[0].MipPixelWidth = newWidth;
            BimImageFile.MipMaps[0].CompressedSize = newDataSize;

            Write(BimImageFile, outputPath);
        }
    }
}
