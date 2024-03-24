using EarthRipperHook.Capture;
using EarthRipperShared;
using Reloaded.Memory;
using System.Runtime.InteropServices;

namespace EarthRipperHook
{
    public class EntryPoint
    {
        private static CaptureHook? _captureHook;

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
            ShaderHelper.Initialize();

            _captureHook = new CaptureHook();

            return (int)RunResult.Success;
        }
    }
}
