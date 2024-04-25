using EarthRipperHook.Capture;
using EarthRipperHook.Menus;
using EarthRipperHook.OrthoCamera;
using EarthRipperHook.RenderPreset;
using EarthRipperShared;
using Reloaded.Memory;
using System.Runtime.InteropServices;

namespace EarthRipperHook
{
    public class EntryPoint
    {
        private static CaptureHook? _captureHook;
        private static OrthoCameraHook? _orthoCameraHook;

        [UnmanagedCallersOnly(EntryPoint = nameof(Run))]
        public static int Run(nuint argsPtr)
        {
            RunArgs args;
            try
            {
                Struct.FromPtr(argsPtr, out args);
            }
            catch
            {
                return (int)RunResult.ReadArgumentsFailed;
            }

            Log.Initialize(LogUtil.GetSharedLogName(args.InjectorProcessID));
            NativeHelper.Initialize(args.GetProcAddressAddress);
            MenuManager.Initialize();
            RenderPresetManager.Initialize();

            _captureHook = new CaptureHook();
            _orthoCameraHook = new OrthoCameraHook();

            return (int)RunResult.Success;
        }
    }
}
