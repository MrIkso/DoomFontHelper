using DoomFontHelper.FileTypes.FontFile;
using System.Drawing.Imaging;

namespace DoomFontHelper.Utils
{
    public class FontVisualizer
    {
        private readonly IdfFile _fontFile;
        private readonly Bitmap _textureAtlas;

        /// <summary>
        /// Initializes the visualizer.
        /// </summary>
        /// <param name="fontFile">Parsed font object.</param>
        /// <param name="textureAtlas">Bitmap texture atlas file (.tga, .png, etc.).</param>
        public FontVisualizer(IdfFile fontFile, Bitmap textureAtlas)
        {
            _fontFile = fontFile ?? throw new ArgumentNullException(nameof(fontFile));
            _textureAtlas = textureAtlas;
        }

        /// <summary>
        /// Generates and saves a character map as a PNG file.
        /// </summary>
        /// <param name="outputPath">Path to save the PNG file.</param>
        /// <param name="cellWidth">Width of a single cell for a character.</param>
        /// <param name="cellHeight">Height of a single cell for a character.</param>
        public void CreateCharacterMap(string outputPath, int cellWidth = 80, int cellHeight = 80)
        {
            if (_fontFile.GlyphsList.Count == 0)
            {
                Console.WriteLine("The font has no glyphs to visualize.");
                return;
            }

            const int charsPerRow = 20; // Number of characters per row
            int rowCount = (int)Math.Ceiling((double)_fontFile.GlyphsList.Count / charsPerRow);
            int mapWidth = charsPerRow * cellWidth;
            int mapHeight = rowCount * cellHeight;

            using (var characterMapBitmap = new Bitmap(mapWidth, mapHeight))
            using (var canvas = Graphics.FromImage(characterMapBitmap))
            using (var labelFont = new Font("Arial", 8))
            using (var labelBrush = new SolidBrush(Color.LimeGreen))
            using (var borderPen = new Pen(Color.FromArgb(50, 50, 50)))
            {
                canvas.Clear(Color.FromArgb(20, 20, 20));

                int currentX = 0;
                int currentY = 0;

                // Use a sorted list for sequential rendering
                var sortedGlyphs = _fontFile.GlyphsList.OrderBy(g => g.Key).ToList();

                foreach (var kvp in sortedGlyphs)
                {
                    int charCode = kvp.Key;
                    GlyphDescription glyph = kvp.Value;

                    // Draw the cell border
                    canvas.DrawRectangle(borderPen, currentX, currentY, cellWidth, cellHeight);

                    // Logic for cutting the glyph from the atlas
                    var sourceRect = new Rectangle(glyph.X, glyph.Y, glyph.Width, glyph.Height);

                    // Center the glyph inside the cell
                    int destX = currentX + (cellWidth - glyph.Width) / 2;
                    int destY = currentY + (cellHeight - glyph.Height) / 2;
                    var destRect = new Rectangle(destX, destY, glyph.Width, glyph.Height);

                    // Draw the cut glyph on our canvas
                    canvas.DrawImage(_textureAtlas, destRect, sourceRect, GraphicsUnit.Pixel);

                    // Label the character and its ID
                    string charLabel = $"'{Convert.ToChar(charCode)}' ({charCode})";
                    canvas.DrawString(charLabel, labelFont, labelBrush, currentX + 2, currentY + 2);

                    // Move to the next cell
                    currentX += cellWidth;
                    if (currentX >= mapWidth)
                    {
                        currentX = 0;
                        currentY += cellHeight;
                    }
                }

                characterMapBitmap.Save(outputPath, ImageFormat.Png);
                Console.WriteLine($"Character map saved to: {outputPath}");
            }
        }
    }
}
