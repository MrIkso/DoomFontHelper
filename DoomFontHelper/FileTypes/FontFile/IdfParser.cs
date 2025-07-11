using DoomFontHelper.Helpers.IO;

namespace DoomFontHelper.FileTypes.FontFile
{
    public class IdfParser
    {
        private static readonly int Signature = 0x696466; // idf

        public IdfFile IdfFileData { get; set; } = new IdfFile();

        public IdfParser(string filePath) {
            Stream fontSteam = File.OpenRead(filePath);
            var br = new ExtendedBinaryReader(fontSteam);
            Load(br);
        }

        public void Load(ExtendedBinaryReader reader)
        {
            var font = new IdfFile();

            // read font header size 14 bytes
            // 3 bytes magic + version byte
            byte[] magic = reader.ReaBytesReverse(3);
            byte version = reader.ReadByte();

            if (BitConverter.ToInt32(magic.Concat(new byte[] { 0 }).ToArray()) != Signature)
            {
                throw new IOException($"Invalid idf signature 0x{BitConverter.ToInt32(magic.Concat(new byte[] { 0 }).ToArray()):X}, expected 0x{Signature:X}");
            }
            // read font configuration in big-endian format
            // 14 bytes
            var config = new FontConfig
            {
                Version = version,
                Size = reader.ReadUInt16Reverse(),
                Base = reader.ReadUInt16Reverse(),
               
                Unknown1 = reader.ReadByte(), // always 255
                FontType = reader.ReadByte(),
                Spacing = reader.ReadUInt16Reverse(),

                NumGlyphs = reader.ReadUInt16Reverse()
            };
            font.Config = config;
            // Console.WriteLine($"Current offset after config: {reader.BaseStream.Position}");
            // read glyphs in little-endian format
            // 12 bytes for each glyph, total NumGlyphs * 12 bytes
            var glyphs = new List<GlyphDescription>(config.NumGlyphs);
            for (int i = 0; i < config.NumGlyphs; i++)
            {
                var glyph = new GlyphDescription
                {
                    Width = reader.ReadByte(),
                    Height = reader.ReadByte(),
                    Top = reader.ReadSByte(), // Top
                    Left = reader.ReadSByte(), // Left
                    XSkip = reader.ReadByte(), // XSkip
                    Unknown = reader.ReadByte(), // unused on game
                    X = reader.ReadUInt16(),
                    Y = reader.ReadUInt16(),
                };
               
                glyphs.Add(glyph);
            }
            
            //Console.WriteLine($"Current ofsset after GlyphDescription : {reader.BaseStream.Position:X}");

            // read symbols table
            // 4 bytes for each symbol, total NumGlyphs * 4 bytes
            var charCodes = new List<int>(config.NumGlyphs);
            for (int i = 0; i < config.NumGlyphs; i++)
            {
                charCodes.Add(reader.ReadInt32());
            }

            // Console.WriteLine($"Current ofsset after symbols table : {reader.BaseStream.Position:X}");

            // Mapping glyphs to character codes
            // on font might be more than one glyph for one character code
            for (int i = 0; i < config.NumGlyphs; i++)
            {
                int charCode = charCodes[i];
                GlyphDescription glyph = glyphs[i];
                var keyValuePair = new KeyValuePair<int, GlyphDescription>(charCode, glyph);
                font.GlyphsList.Add(keyValuePair);
            }

            IdfFileData = font;
        }

        public void Write(IdfFile idfFile, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var bw = new ExtendedBinaryWriter(fs))
            {
                // Write header
                var magic = "idf"u8.ToArray();
                bw.Write(magic);
                bw.Write(idfFile.Config.Version);
                bw.WriteReverse(idfFile.Config.Size);
                bw.WriteReverse(idfFile.Config.Base);
                bw.Write(idfFile.Config.Unknown1);
                bw.Write(idfFile.Config.FontType);
                bw.WriteReverse(idfFile.Config.Spacing);
                bw.WriteReverse(idfFile.Config.NumGlyphs);
                // Write glyphs
                foreach (var kvp in idfFile.GlyphsList)
                {
                    var glyph = kvp.Value;
                    bw.Write(glyph.Width);
                    bw.Write(glyph.Height);
                    bw.Write(glyph.Top);
                    bw.Write(glyph.Left);
                    bw.Write(glyph.XSkip);
                    bw.Write(glyph.Unknown);
                    bw.Write(glyph.X);
                    bw.Write(glyph.Y);
                }
                // Write symbols table
                foreach (var kvp in idfFile.GlyphsList)
                {
                    var charCode = kvp.Key;
                    bw.Write(charCode);
                }
            }
        }
    }
}
