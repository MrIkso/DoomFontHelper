
using BCnEncoder.Shared.ImageFiles;
using DoomFontHelper;
using DoomFontHelper.FileTypes.BimFile;
using DoomFontHelper.FileTypes.FontFile;
using DoomFontHelper.Utils;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;
        Console.InputEncoding = Encoding.Unicode;
        Console.WriteLine("DoomFontHelper Tool by MrIkso");
        if (args.Length == 0)
        {
            Console.WriteLine("Please specify an argument: repackfront, repackbim, fontinfo, extractbim, biminfo, fontvisualize");
            Console.WriteLine("Preass any key to exit.");
            Console.ReadKey();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "repackfont":
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: repackfont <inputFntFilePath> <inputFontFilePath> <outputFontFilePath>");
                    return;
                }
                string inputFntFilePath = args[1];
                string inputFontFilePath = args[2];
                string outputFontFilePath = args[3];
                RepackFontFile(inputFntFilePath, inputFontFilePath, outputFontFilePath);
                break;
            case "repackbim":
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: repackbim <newDdsFilePath> <inputBimageFilePath> <outputBimageFilePath>");
                    return;
                }
                string ddsFilePath = args[1];
                string inputBimFilePath = args[2];
                string outputBimFilePath = args[3];

                RepackBimFile(ddsFilePath, inputBimFilePath, outputBimFilePath);
                break;

            case "extractbim":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: extractbim <inputBimageFilePath>");
                    return;
                }
                string extractBimFilePath = args[1];
                var parser = new BimParser(extractBimFilePath);
                ExtractImageFromBim(parser, extractBimFilePath);
                break;
            case "fontinfo":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: fontinfo <inputFontFilePath>");
                    return;
                }
                string inputFont = args[1];
                PrintFontInfo(inputFont);
                break;
            case "biminfo":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: biminfo <inputBimageFilePath>");
                    return;
                }
                string inputBim = args[1];
                PrintBimImageInfo(inputBim);

                break;
            case "fontvisualize":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: fontvisualize <inputFontFilePath> <inputBimageFilePath>");
                    return;
                }
                VisualizeFont(args[1], args[2]);
                break;

            default:
                Console.WriteLine("Unknown argument. Use repackfont, repackbim, extractbim, fontinfo, biminfo, fontvisualize");
                break;
        }
    }

    #region Font Editing
    private static void RepackFontFile(string inputFntFilePath, string inputFontFilePath, string outputFontFilePath)
    {
        if (!File.Exists(inputFntFilePath))
        {
            Console.WriteLine($"Error: File {inputFntFilePath} not found.");
            return;
        }
        Console.WriteLine($"Processing font file: {inputFontFilePath}");

        var newFontData = FontProcessor.GenerateFontFromBmFont(inputFntFilePath);
        if (newFontData == null)
        {
            Console.WriteLine("Failed to generate font data.");
            return;
        }

        var outputDirectory = Path.GetDirectoryName(outputFontFilePath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var fontParser = new IdfParser(inputFontFilePath);
        // copying config from old font file
        newFontData.Config.Version = fontParser.IdfFileData.Config.Version;
        newFontData.Config.FontType = fontParser.IdfFileData.Config.FontType;
        newFontData.Config.Base = fontParser.IdfFileData.Config.Base;
        newFontData.Config.Spacing = fontParser.IdfFileData.Config.Spacing;
        newFontData.Config.Unknown1 = fontParser.IdfFileData.Config.Unknown1;

        fontParser.Write(newFontData, outputFontFilePath);
        Console.WriteLine($"Font file saved to: {outputFontFilePath}");
    }

    private static void PrintFontInfo(string inputFontFilePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding cyrillicEncoding = Encoding.GetEncoding("windows-1251");
        if (!File.Exists(inputFontFilePath))
        {
            Console.WriteLine($"Error: File {inputFontFilePath} not found.");
            return;
        }
        var fontParser = new IdfParser(inputFontFilePath);
        var fontData = fontParser.IdfFileData;
        StringBuilder fontIfo = new StringBuilder();
        fontIfo.AppendLine("Font Information:");
        fontIfo.AppendLine($"Version: {fontData.Config.Version}");
        fontIfo.AppendLine($"Size: {fontData.Config.Size}");
        fontIfo.AppendLine($"Base: {fontData.Config.Base}");
        fontIfo.AppendLine($"Unknown1: {fontData.Config.Unknown1}");
        fontIfo.AppendLine($"Font Type: {fontData.Config.FontType}");
        fontIfo.AppendLine($"Spacing: {fontData.Config.Spacing}");
        fontIfo.AppendLine($"Glyphs from file: {fontData.Config.NumGlyphs}");
        var glyphs = fontData.GlyphsList;
       // fontIfo.AppendLine($"Total Glyphs: {glyphs.Count}");
        fontIfo.AppendLine("Glyphs:");
        foreach (var glyph in glyphs)
        {
            fontIfo.AppendLine($"ID: {glyph.Key}, Char: {(char)glyph.Key}, Glyph: {glyph.Value}");
        }
        Console.WriteLine(fontIfo.ToString());
        // only for debug
        // fontParser.Write(fontData, inputFontFilePath + "new.fnt");
        File.WriteAllText(inputFontFilePath + ".info.txt", fontIfo.ToString());
        Console.WriteLine($"Font info saved to: {inputFontFilePath}.info.txt");
    }

    private static void VisualizeFont(string inputFontFilePath, string inputTexturePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding cyrillicEncoding = Encoding.GetEncoding("windows-1251");
        if (!File.Exists(inputFontFilePath))
        {
            Console.WriteLine($"Error: File {inputFontFilePath} not found.");
            return;
        }
        var fontParser = new IdfParser(inputFontFilePath);
        var fontData = fontParser.IdfFileData;

        var parser = new BimParser(inputTexturePath);
        BimImage image = parser.BimImageFile;
        var textureHeader = image.Header;

        Bitmap textureAtlas;
        if (textureHeader.TextureFormat == (int)IdTechImageFormat.ALPHA_8)
        {
            textureAtlas = TextureConverter.ConvertA8ToBitmap(image.MipMaps[0].PixelData, textureHeader.PixelWidth, textureHeader.PixelHeight);
        }
        else
        {
            DdsFile ddsFile = parser.ConvertToDds(parser.BimImageFile);
            textureAtlas = DDSUtils.DdsToBitmap(ddsFile);
        }
        var outputPngFilePath = Path.Combine(Path.GetDirectoryName(inputFontFilePath) ?? ".", $"{Path.GetFileNameWithoutExtension(inputFontFilePath)}_character_map.png");
        FontVisualizer visualizer = new FontVisualizer(fontData, textureAtlas);
        visualizer.CreateCharacterMap(outputPngFilePath);
        Console.WriteLine($"Character map saved to: {outputPngFilePath}");
    }
    #endregion

    #region Bim Editing
    private static void PrintBimImageInfo(string path)
    {
        var parser = new BimParser(path);
        BimImage image = parser.BimImageFile;
        Console.WriteLine("Bim Image Information:");
        Console.WriteLine($"Signature: {image.Header.GetSignature()}");
        Console.WriteLine($"Version: {image.Header.GetVersion()}");
        Console.WriteLine($"Size: {image.Header.PixelWidth}x{image.Header.PixelHeight}");
        Console.WriteLine($"Depth: {image.Header.Depth}");
        Console.WriteLine($"Mip map count: {image.Header.MipCount}");
        Console.WriteLine($"Is Streamed: {image.Header.BoolIsStreamed}");
        Console.WriteLine($"Unknown Byte: {image.Header.UnknownByte}");
        int formatId = image.Header.TextureFormat;
        IdTechImageFormat format = IdTechImageFormatExtensions.ToImageFormat(formatId);
        Console.WriteLine($"Texture Type: {image.Header.TextureType}");
        Console.WriteLine($"Texture Format (ID): {formatId}, {IdTechImageFormatExtensions.GetFormatDescription(format)}");
        Console.WriteLine($"Texture Material Kind: {image.Header.TextureMaterialKind}");
        if (image.MipMaps.Count > 0)
        {
            var mainMip = image.MipMaps[0];
            Console.WriteLine("===================");
            Console.WriteLine("Main Mip Map Info:");
            Console.WriteLine($"Mip map size: {mainMip.MipPixelWidth}x{mainMip.MipPixelHeight}");
            Console.WriteLine($"Data Size: {mainMip.CompressedSize} bytes");
            Console.WriteLine($"Pixel Data Length: {mainMip.PixelData?.Length ?? 0} bytes");
            Console.WriteLine($"Unknown1: {mainMip.Unknown1}");
        }
    }

    private static void ExtractImageFromBim(BimParser parser, string imageFilePath)
    {
        string saveDirrectory = Path.GetDirectoryName(imageFilePath);
        string saveFileName = Path.GetFileNameWithoutExtension(imageFilePath) + ".dds";
        string savePath = Path.Combine(saveDirrectory, saveFileName);

        DdsFile ddsFile = parser.ConvertToDds(parser.BimImageFile);
        DDSUtils.SaveDds(ddsFile, savePath);
        Console.WriteLine($"DDS file saved to: {savePath}");
    }

    private static void RepackBimFile(string textureFilePath, string inputFilePath, string oputputFilePath)
    {
        try
        {
            if (!File.Exists(textureFilePath))
            {
                Console.WriteLine($"Error: Texture file {inputFilePath} not found.");
                return;
            }
            var outputDirectory = Path.GetDirectoryName(oputputFilePath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var parser = new BimParser(inputFilePath);
            if (textureFilePath.EndsWith(".png")) {
                parser.ReplacePngImageInBim(textureFilePath, oputputFilePath);
            }
            else
            {
                parser.ReplaceDDSImageInBim(textureFilePath, oputputFilePath);
            }
           
            Console.WriteLine($"File processed and saved to: {oputputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    #endregion
}