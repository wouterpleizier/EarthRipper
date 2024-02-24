using EarthRipperHook.Hooks;
using EarthRipperShared;
using Reloaded.Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EarthRipperHook
{
    public class EntryPoint
    {
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
            HookUtil.Initialize(args.GetProcAddressAddress);

            if (!Debugger.IsAttached)
            {
                Log.Warning("Waiting for debugger");
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }
            }

            HookProxy<IGGfx.IgOglVisualContext.GenericDraw>.Subscribe(HandleGenericDraw);
            HookProxy<Qt5Widgets.QFileDialog.GetSaveFileName>.Subscribe(HandleGetSaveFileName);

            return (int)RunResult.Success;
        }

        private static Random _random = new Random();
        private static void HandleGenericDraw(nuint igOglVisualContext, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5)
        {
            if (_random.Next(10) >= 1)
            {
                HookProxy<IGGfx.IgOglVisualContext.GenericDraw>.SuppressOriginal();
            }
        }

        private static nuint HandleGetSaveFileName(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter)
        {
            nuint returnValue = HookProxy<Qt5Widgets.QFileDialog.GetSaveFileName>.OriginalFunction(result, parentWidget, caption, dir, filter, selectedFilter);
            
            HookProxy<Qt5Widgets.QFileDialog.GetSaveFileName>.SuppressOriginal(returnValue);

            return default;
        }
    }
}
