using System.Diagnostics;

namespace DoomFontHelper.MSDF
{
    internal class FontCompiler
    {
        public virtual uint Resolution { get; set; } = 32;

        public virtual uint Range { get; set; } = 4;
        private readonly string toolPath;

        public FontCompiler()
        : this(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ThirdParty/msdf-atlas-gen.exe"))
        {
        }

        public FontCompiler(string toolPath)
        {
            this.toolPath = toolPath;
        }

        /// <summary>
		/// Run msdf-atlas-gen on our font
		/// </summary>
		/// <param name="ttfFontPatch">Path for the font</param>
		/// <param name="charsetPath">Path for the charset.txt for msdf-atlas-gen</param>
		/// <param name="objPath">Path for the folder to store the output in</param>
		/// <returns>Tuple of paths to atlas bitmap and atlas json</returns>
		public (string atlasBitmap, string atlasJSON) CreateAtlas(string ttfFontPatch, string objPath, string charsetPath="")
        {
            if (charsetPath== string.Empty)
            {
                charsetPath = Path.Combine(Path.GetDirectoryName(toolPath), "charset.txt");
            }
            var name = Path.GetFileNameWithoutExtension(ttfFontPatch);
            var outputPath = Path.Combine(objPath, $"{name}-atlas.png");
            var jsonPath = Path.Combine(objPath, $"{name}-layout.json");

            var startInfo = new ProcessStartInfo(toolPath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = $"-font \"{ttfFontPatch}\" -imageout \"{outputPath}\" -type sdf -charset \"{charsetPath}\" -size {this.Resolution} -pxrange {this.Range} -json \"{jsonPath}\" -yorigin top"
            };
            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Could not start msdf-atlas-gen.exe");
            }
            process.WaitForExit();
            return (outputPath, jsonPath);
        }
    }
}
