using System.Text;

namespace DoomFontHelper.FileTypes.BimFile
{
    public class BimageHeader
    {
        public uint Timestamp { get; set; }
        public uint MagicAndVersion { get; set; }
        public int TextureType { get; set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public int Depth { get; set; }
        public int MipCount { get; set; }
        public byte BoolIsStreamed { get; set; }
        public int TextureFormat { get; set; }
        public int TextureMaterialKind { get; set; }
        public int StreamDBMipCount { get; set; }
        public byte UnknownByte { get; set; }

        public string GetSignature() => Encoding.ASCII.GetString(BitConverter.GetBytes(MagicAndVersion), 0, 3);
        public byte GetVersion() => BitConverter.GetBytes(MagicAndVersion)[3];
    }

    public class BimageMipmap
    {
        public uint Unknown1 { get; set; }
        public uint MipLevelIndex { get; set; }
        public int MipPixelWidth { get; set; }
        public int MipPixelHeight { get; set; }
        public int CompressedSize { get; set; }
        public byte[] PixelData { get; set; }
    }

    public class BimImage
    {
        public BimageHeader Header { get; set; } = new BimageHeader();
        public List<BimageMipmap> MipMaps { get; set; } = [];
    }

}
