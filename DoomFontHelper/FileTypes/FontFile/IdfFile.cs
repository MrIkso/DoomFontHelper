namespace DoomFontHelper.FileTypes.FontFile
{
    public class IdfFile
    {
        public FontConfig Config { get; set; }
        public List<KeyValuePair<int, GlyphDescription>> GlyphsList { get; set; } = [];
    }
}
