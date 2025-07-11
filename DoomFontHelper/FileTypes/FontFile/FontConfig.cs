namespace DoomFontHelper.FileTypes.FontFile
{
    public class FontConfig
    {
        public byte Version { get; set; }
        public ushort Size { get; set; }
        public ushort Base { get; set; }
        public byte Unknown1 { get; set; }
        public byte FontType { get; set; }
        public ushort Spacing { get; set; }
        public ushort NumGlyphs { get; set; }
    }
}
