namespace DoomFontHelper.FileTypes.BimFile
{
    public enum IdTechTextureType
    {
        TT_2D = 0x0,
        TT_3D = 0x1,
        TT_CUBIC = 0x2,
    }

    public enum IdTechImageFormat
    {
        NONE = 0x0,
        RGBA32F = 0x1,
        RGBA16F = 0x2,
        RGBA8 = 0x3,
        ARGB8 = 0x4,
        ALPHA_8 = 0x5,
        L8A8 = 0x6,
        RG8 = 0x7,
        LUM8 = 0x8,
        INT8 = 0x9,
        BC1 = 0xA,
        BC3 = 0xB,
        DEPTH = 0xC,
        DEPTH_STENCIL = 0xD,
        X32F_R32F = 0xE, 
        Y16F_X16F_RG16F = 0xF, // same as RG16F
        X16_R16 = 0x10,
        Y16_X16_RG16 = 0x11,
        RGB565 = 0x12,
        R8 = 0x13,
        R11G11B10F = 0x14,
        X16F_R16F = 0x15,
        BC6H_UF16 = 0x16,
        BC7 = 0x17,
        BC4 = 0x18,
        BC5 = 0x19,
        RG16F = 0x1A,
        R10G10B10A2 = 0x1B,
        RG32F = 0x1C,
        R32_UINT = 0x1D,
        R16_UINT = 0x1E,
        DEPTH16 = 0x1F,
        RGBA8_SRGB = 0x20,
        BC1_SRGB = 0x21,
        BC3_SRGB = 0x22,
        BC7_SRGB = 0x23,
        BC6H_SF16 = 0x24,
        ASTC_4X4 = 0x25,
        ASTC_4X4_SRGB = 0x26,
        DEPTH32F = 0x35,
        BC1_ZERO_ALPHA = 0x36,
        NEXTAVAILABLE = 0x37,
    }
}
