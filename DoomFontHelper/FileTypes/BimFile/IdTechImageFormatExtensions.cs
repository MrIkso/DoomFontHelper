using BCnEncoder.Shared.ImageFiles;

namespace DoomFontHelper.FileTypes.BimFile
{
    public static class IdTechImageFormatExtensions
    {
        public static IdTechImageFormat ToImageFormat(int formatValue)
        {
            return Enum.IsDefined(typeof(IdTechImageFormat), formatValue)
                ? (IdTechImageFormat)formatValue
                : IdTechImageFormat.NONE;
        }

        public static string GetFormatDescription(IdTechImageFormat format)
        {
            return format switch
            {
                IdTechImageFormat.NONE => "No Format",
                IdTechImageFormat.RGBA32F => "RGBA 32-bit Float",
                IdTechImageFormat.RGBA16F => "RGBA 16-bit Float (Half)",
                IdTechImageFormat.RGBA8 => "RGBA 8-bit Unsigned Integer",
                IdTechImageFormat.ALPHA_8 => "Alpha 8-bit",
                IdTechImageFormat.L8A8 => "Luminance 8-bit + Alpha 8-bit",
                IdTechImageFormat.RG8 => "RG 8-bit Unsigned Integer",
                IdTechImageFormat.BC1 => "Compressed BC1 (DXT1)",
                IdTechImageFormat.BC3 => "Compressed BC3 (DXT5)",
                IdTechImageFormat.DEPTH_STENCIL => "Depth 24-bit + Stencil 8-bit",
                IdTechImageFormat.R8 => "R 8-bit Unsigned Integer",
                IdTechImageFormat.R11G11B10F => "RGB 11-11-10 Float",
                IdTechImageFormat.BC6H_UF16 => "Compressed BC6H (Unsigned HDR)",
                IdTechImageFormat.BC7 => "Compressed BC7",
                IdTechImageFormat.BC4 => "Compressed BC4 (R Channel)",
                IdTechImageFormat.BC5 => "Compressed BC5 (RG Channels)",
                IdTechImageFormat.RG16F => "RG 16-bit Float",
                IdTechImageFormat.R10G10B10A2 => "RGBA 10-10-10-2 Unsigned Integer",
                IdTechImageFormat.RG32F => "RG 32-bit Float",
                IdTechImageFormat.RGBA8_SRGB => "sRGBA 8-bit Unsigned Integer",
                IdTechImageFormat.BC1_SRGB => "Compressed sRGB BC1 (DXT1)",
                IdTechImageFormat.BC3_SRGB => "Compressed sRGB BC3 (DXT5)",
                IdTechImageFormat.BC7_SRGB => "Compressed sRGB BC7",
                IdTechImageFormat.ASTC_4X4 => "Compressed ASTC 4x4",
              
                _ => throw new ArgumentException($"Unknown texture format (ID: {format})")
            };
        }

        public static int MapDxgiFormatToIdTech(DxgiFormat dxgiFormat, bool isSrgb = false)
        {
            return dxgiFormat switch
            {
                DxgiFormat.DxgiFormatR32G32B32A32Float => (int)IdTechImageFormat.RGBA32F,
                DxgiFormat.DxgiFormatR16G16B16A16Float => (int)IdTechImageFormat.RGBA16F,
                DxgiFormat.DxgiFormatR8G8B8A8Unorm when !isSrgb => (int)IdTechImageFormat.RGBA8,
                DxgiFormat.DxgiFormatR8G8B8A8UnormSrgb when isSrgb => (int)IdTechImageFormat.RGBA8_SRGB,

                DxgiFormat.DxgiFormatA8Unorm => (int)IdTechImageFormat.ALPHA_8,
                DxgiFormat.DxgiFormatR8G8Unorm => (int)IdTechImageFormat.RG8,
                DxgiFormat.DxgiFormatR8Unorm => (int)IdTechImageFormat.R8,
                DxgiFormat.DxgiFormatR16G16Float => (int)IdTechImageFormat.RG16F,
                DxgiFormat.DxgiFormatR32G32Float => (int)IdTechImageFormat.RG32F,

                DxgiFormat.DxgiFormatBc1Unorm when !isSrgb => (int)IdTechImageFormat.BC1,
                DxgiFormat.DxgiFormatBc1UnormSrgb when isSrgb => (int)IdTechImageFormat.BC1_SRGB,

                DxgiFormat.DxgiFormatBc3Unorm when !isSrgb => (int)IdTechImageFormat.BC3,
                DxgiFormat.DxgiFormatBc3UnormSrgb when isSrgb => (int)IdTechImageFormat.BC3_SRGB,

                DxgiFormat.DxgiFormatBc4Unorm => (int)IdTechImageFormat.BC4,
                DxgiFormat.DxgiFormatBc5Unorm => (int)IdTechImageFormat.BC5,

                DxgiFormat.DxgiFormatBc6HUf16 => (int)IdTechImageFormat.BC6H_UF16, // Unsigned
                DxgiFormat.DxgiFormatBc6HSf16 => (int)IdTechImageFormat.BC6H_SF16, // Signed

                DxgiFormat.DxgiFormatBc7Unorm when !isSrgb => (int)IdTechImageFormat.BC7,
                DxgiFormat.DxgiFormatBc7UnormSrgb when isSrgb => (int)IdTechImageFormat.BC7_SRGB,

                DxgiFormat.DxgiFormatD32Float => (int)IdTechImageFormat.DEPTH32F,
                DxgiFormat.DxgiFormatD24UnormS8Uint => (int)IdTechImageFormat.DEPTH_STENCIL,
                DxgiFormat.DxgiFormatD16Unorm => (int)IdTechImageFormat.DEPTH16,

                DxgiFormat.DxgiFormatB5G6R5Unorm => (int)IdTechImageFormat.RGB565,
                DxgiFormat.DxgiFormatR11G11B10Float => (int)IdTechImageFormat.R11G11B10F,
                DxgiFormat.DxgiFormatR10G10B10A2Unorm => (int)IdTechImageFormat.R10G10B10A2,

                DxgiFormat.DxgiFormatR32Uint => (int)IdTechImageFormat.R32_UINT,
                DxgiFormat.DxgiFormatR16Uint => (int)IdTechImageFormat.R16_UINT,
                _ => -1 
            };

        }

        public static DxgiFormat MapIdTechFormatToDxgi(int idTechFormatId)
        {
            return idTechFormatId switch
            {
                // Uncompresed RGBA ---
                0x1 => DxgiFormat.DxgiFormatR32G32B32A32Float, // FMT_RGBA32F
                0x2 => DxgiFormat.DxgiFormatR16G16B16A16Float, // FMT_RGBA16F
                0x3 => DxgiFormat.DxgiFormatR8G8B8A8Unorm,   // FMT_RGBA8
                0x20 => DxgiFormat.DxgiFormatR8G8B8A8UnormSrgb, // FMT_RGBA8_SRGB

                // Uncompressed with smaller chanels
                0x5 => DxgiFormat.DxgiFormatA8Unorm,          // FMT_ALPHA (8-bit Alpha)
                0x6 => DxgiFormat.DxgiFormatR8G8Unorm,         // FMT_L8A8_DEPRECATED (analouge - RG8)
                0x7 => DxgiFormat.DxgiFormatR8G8Unorm,         // FMT_RG8
                0x13 => DxgiFormat.DxgiFormatR8Unorm,       // FMT_R8
                0xF => DxgiFormat.DxgiFormatR16G16Float,      // FMT_Y16F_X16F_RG16F (RG16F)
                0x1A => DxgiFormat.DxgiFormatR16G16Float,      // FMT_RG16F
                0x1C => DxgiFormat.DxgiFormatR32G32Float,      // FMT_RG32F

                // Compresed formats (Block Compression)
                0xA => DxgiFormat.DxgiFormatBc1Unorm,       // FMT_BC1
                0x21 => DxgiFormat.DxgiFormatBc1UnormSrgb,    // FMT_BC1_SRGB
                0x36 => DxgiFormat.DxgiFormatBc1Unorm,       // FMT_BC1_ZERO_ALPHA
                0xB => DxgiFormat.DxgiFormatBc3Unorm,       // FMT_BC3
                0x22 => DxgiFormat.DxgiFormatBc3UnormSrgb,    // FMT_BC3_SRGB
                0x18 => DxgiFormat.DxgiFormatBc4Unorm,       // FMT_BC4
                0x19 => DxgiFormat.DxgiFormatBc5Unorm,       // FMT_BC5
                0x16 => DxgiFormat.DxgiFormatBc6HUf16,       // FMT_BC6H_UF16 (HDR, unsigned)
                0x24 => DxgiFormat.DxgiFormatBc6HSf16,       // FMT_BC6H_SF16 (HDR, signed)
                0x17 => DxgiFormat.DxgiFormatBc7Unorm,       // FMT_BC7
                0x23 => DxgiFormat.DxgiFormatBc7UnormSrgb,    // FMT_BC7_SRGB

                0xC => DxgiFormat.DxgiFormatD32Float,       // FMT_DEPTH
                0xD => DxgiFormat.DxgiFormatD24UnormS8Uint, // FMT_DEPTH_STENCIL
                0x1F => DxgiFormat.DxgiFormatD16Unorm,       // FMT_DEPTH16
                0x35 => DxgiFormat.DxgiFormatD32Float,      // FMT_DEPTH32F

                0x12 => DxgiFormat.DxgiFormatB5G6R5Unorm,     // FMT_RGB565
                0x14 => DxgiFormat.DxgiFormatR11G11B10Float,   // FMT_R11G11B10F
                0x1B => DxgiFormat.DxgiFormatR10G10B10A2Unorm, // FMT_R10G10B10A2

                0x1D => DxgiFormat.DxgiFormatR32Uint,        // FMT_R32_UINT
                0x1E => DxgiFormat.DxgiFormatR16Uint,        // FMT_R16_UINT

                0x0 => DxgiFormat.DxgiFormatUnknown,         // FMT_NONE
                _ => DxgiFormat.DxgiFormatUnknown
            };
        }
    }

}