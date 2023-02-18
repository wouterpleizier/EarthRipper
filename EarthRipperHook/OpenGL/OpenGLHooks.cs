using EasyHook;
using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook.OpenGL
{
    internal class OpenGLHooks : HookContainer
    {
        internal delegate void WGLSwapBuffersHandler(IntPtr hdc);
        internal static event WGLSwapBuffersHandler WGLSwapBuffers;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool wglSwapBuffersHookDelegate(IntPtr hdc);
        private LocalHook _wglSwapBuffersHook;

        public OpenGLHooks()
        {
            _wglSwapBuffersHook = CreateHook("opengl32.dll",
                "wglSwapBuffers",
                new wglSwapBuffersHookDelegate(wglSwapBuffersHook));
        }

        public override void Dispose()
        {
            _wglSwapBuffersHook.Dispose();
        }

        private bool wglSwapBuffersHook(IntPtr hdc)
        {
            WGLSwapBuffers?.Invoke(hdc);

            return OpenGLMethods.wglSwapBuffers(hdc);
        }
    }
}
