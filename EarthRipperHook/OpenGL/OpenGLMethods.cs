using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook.OpenGL
{
    internal static class OpenGLMethods
    {
        [DllImport("opengl32.dll", EntryPoint = "wglSwapBuffers", CallingConvention = CallingConvention.StdCall)]
        internal static extern bool wglSwapBuffers(IntPtr hdc);

        [DllImport("opengl32.dll", EntryPoint = "glReadPixels", CallingConvention = CallingConvention.StdCall)]
        internal static extern void glReadPixels(int x, int y, int width, int height, PixelFormat format, DataType type, IntPtr pixels);
    }
}
