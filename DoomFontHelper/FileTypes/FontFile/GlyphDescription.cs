namespace DoomFontHelper.FileTypes.FontFile
{
    /*
    Reversed of glyphInfo_t structure in Doom 2016 font files
    Same structure is used in Doom 3 BFG Edition font files
    struct glyphInfo_t
    {
        //offset 0 , size 1
        // width of glyph in pixels
        unsigned char width;
        //offset 1 , size 1
        //height of glyph in pixels
        unsigned char height;
        //offset 2 , size 1
        // distance in pixels from the base line to the top of the glyph
        char top;
        //offset 3 , size 1
        // distance in pixels from the pen to the left edge of the glyph
        char left;
        //offset 4 , size 1
        // x adjustment after rendering this glyph
        unsigned char xSkip;
        //offset 6 , size 2
        // x offset in image where glyph starts (in pixels)
        unsigned short s;
        //offset 8 , size 2
        // y offset in image where glyph starts (in pixels)
        unsigned short t;
    };
    */
    public class GlyphDescription
    {
        /// <summary>
        /// width of glyph in pixels
        /// </summary>
        public byte Width { get; set; }
        /// <summary>
        /// height of glyph in pixels
        /// </summary>
        public byte Height { get; set; }
        /// <summary>
        /// distance in pixels from the base line to the top of the glyph
        /// </summary>
        public sbyte Top { get; set; }
        /// <summary>
        /// distance in pixels from the pen to the left edge of the glyph
        /// </summary>
        public sbyte Left { get; set; }
        /// <summary>
        /// x adjustment after rendering this glyph
        /// </summary>
        public byte XSkip { get; set; }
        /// <summary>
        /// unknown value, must be a padding byte or color value
        /// </summary>
        public byte Unknown { get; set; }
        /// <summary>
        /// x offset in image where glyph starts (in pixels)
        /// </summary>
        public ushort X { get; set; }
        /// <summary>
        /// y offset in image where glyph starts (in pixels)
        /// </summary>
        public ushort Y { get; set; }

        public override string ToString()
        {
            return $"Pos:(X:{X},Y:{Y}), Size:(W:{Width} x H:{Height}), XOffset: {Left}, YOffset: {Top}, XAdvance:{XSkip}, Unknown:{Unknown} ";
        }
    }
}
