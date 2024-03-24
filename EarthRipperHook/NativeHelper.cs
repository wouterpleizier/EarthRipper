using Reloaded.Hooks;
using System.Diagnostics;

namespace EarthRipperHook
{
    internal static class NativeHelper
    {
        [X86Function(X86CallingConventions.Stdcall)]
        [X64Function(X64CallingConventions.Microsoft)]
        private delegate nuint GetProcAddressDelegate(nuint module, string procName);
        private static GetProcAddressDelegate _getProcAddressFunc;

        private static readonly Dictionary<string, nuint> _moduleAddresses;

        static NativeHelper()
        {
            _getProcAddressFunc = (_, _) => throw new InvalidOperationException();
            _moduleAddresses = [];
        }

        internal static void Initialize(long getProcAddressAddress)
        {
            _getProcAddressFunc = ReloadedHooks.Instance.CreateWrapper<GetProcAddressDelegate>(getProcAddressAddress, out _);
        }

        internal static nuint GetModuleAddress(string moduleName)
        {
            if (!_moduleAddresses.TryGetValue(moduleName, out nuint result))
            {
                if (Process.GetCurrentProcess()
                    .Modules.Cast<ProcessModule>()
                    .FirstOrDefault(module => module.ModuleName == moduleName) is ProcessModule module)
                {
                    result = (nuint)module.BaseAddress;
                    _moduleAddresses.Add(moduleName, result);
                }
            }

            return result;
        }

        internal static nuint GetProcAddress(string moduleName, string procName)
        {
            nuint module = GetModuleAddress(moduleName);
            return _getProcAddressFunc.Invoke(module, procName);
        }
    }
}
