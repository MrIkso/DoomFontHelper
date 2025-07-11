using DoomFontHelper.FileTypes.FontFile;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DoomFontHelper
{
    public class FontProcessor
    {
      
        public static IdfFile? GenerateFontFromBmFont(string fntFilePath)
        {
            if (!File.Exists(fntFilePath))
            {
                Console.WriteLine($"Error: file {fntFilePath} not found.");
                return null;
            }

            var fontData = new IdfFile();
            var fontConfig = new FontConfig();
            var parsedGlyphs = new List<KeyValuePair<int, GlyphDescription>>();

            var fntLines = File.ReadAllLines(fntFilePath, Encoding.UTF8);
            var baseLine = 0;
            foreach (var line in fntLines)
            {
                if (line.StartsWith("info"))
                {
                    fontConfig.Size = (ushort)GetIntValue(line, "size");
                }
                 else if (line.StartsWith("common"))
                 {
                    baseLine = GetIntValue(line, "base");
                    fontConfig.Base = (ushort) baseLine;
                }
                else if (line.StartsWith("char ") && !line.StartsWith("chars count"))
                {
                    int charCode = GetIntValue(line, "id");
                    var glyph = new GlyphDescription
                    {
                        // Data from BMFont
                        Width = (byte)GetIntValue(line, "width"),
                        Height = (byte)GetIntValue(line, "height"),
                        X = (ushort)GetIntValue(line, "x"),
                        Y = (ushort)GetIntValue(line, "y"),
                        Left = (sbyte)GetIntValue(line, "xoffset"),
                        Top = (sbyte)(baseLine - GetIntValue(line, "yoffset")),
                        XSkip = (byte)GetIntValue(line, "xadvance"),
                        Unknown = 0,
                    };
                    var keyValuePair = new KeyValuePair<int, GlyphDescription>(charCode, glyph);
                    parsedGlyphs.Add(keyValuePair);
                }
            }
            if (parsedGlyphs.Count == 0)
            {
                Console.WriteLine("Warning: No glyphs found in the .fnt file.");
                return null;
            }

            parsedGlyphs.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));

            fontConfig.NumGlyphs = (ushort)parsedGlyphs.Count;
            fontData.Config = fontConfig;
            fontData.GlyphsList = [.. parsedGlyphs];

            Console.WriteLine($"Successfully parsed {fontData.Config.NumGlyphs} glyphs from file {Path.GetFileName(fntFilePath)}.");

            return fontData;
        }

        private static string? GetValue(string line, string key)
        {
            var match = Regex.Match(line, $@"{key}=""?([^""\s]+)""?");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static int GetIntValue(string line, string key)
        {
            var valueStr = GetValue(line, key);
            return valueStr != null ? int.Parse(valueStr, CultureInfo.InvariantCulture) : 0;
        }

        private static float GetFloatValue(string line, string key)
        {
            var valueStr = GetValue(line, key);
            return valueStr != null ? float.Parse(valueStr, CultureInfo.InvariantCulture) : 0f;
        }
    }
}
