namespace EarthRipperHook.OpenGL
{
    internal enum PixelFormat : uint
    {
        GL_COLOR_INDEX = 0x1900,
        GL_STENCIL_INDEX = 0x1901,
        GL_DEPTH_COMPONENT = 0x1902,
        GL_RED = 0x1903,
        GL_GREEN = 0x1904,
        GL_BLUE = 0x1905,
        GL_ALPHA = 0x1906,
        GL_RGB = 0x1907,
        GL_RGBA = 0x1908,
        GL_LUMINANCE = 0x1909,
        GL_LUMINANCE_ALPHA = 0x190A
    }

    internal enum DataType : uint
    {
        GL_BYTE = 0x1400,
        GL_UNSIGNED_BYTE = 0x1401,
        GL_SHORT = 0x1402,
        GL_UNSIGNED_SHORT = 0x1403,
        GL_INT = 0x1404,
        GL_UNSIGNED_INT = 0x1405,
        GL_FLOAT = 0x1406,
        GL_2_BYTES = 0x1407,
        GL_3_BYTES = 0x1408,
        GL_4_BYTES = 0x1409,
        GL_DOUBLE = 0x140A
    }
}
