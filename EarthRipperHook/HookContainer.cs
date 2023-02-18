using EasyHook;
using System;

namespace EarthRipperHook
{
    internal abstract class HookContainer : IDisposable
    {
        public abstract void Dispose();

        protected LocalHook CreateHook(string module, string symbol, Delegate handler)
        {
            LocalHook result = LocalHook.Create(LocalHook.GetProcAddress(module, symbol), handler, this);
            result.ThreadACL.SetExclusiveACL(new int[] { 0 });

            return result;
        }
    }
}
