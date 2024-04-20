using Reloaded.Hooks;
using Reloaded.Hooks.Definitions.Helpers;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.Sigscan;
using System.Diagnostics;
using Reloaded.Memory.Sources;

namespace EarthRipperHook
{
    internal static class NativeHelper
    {
        [X86Function(X86CallingConventions.Stdcall)]
        [X64Function(X64CallingConventions.Microsoft)]
        private delegate nuint GetProcAddressDelegate(nuint module, string procName);
        private static GetProcAddressDelegate _getProcAddressFunc;

        private static readonly object _lock;
        private static readonly Dictionary<string, nuint> _moduleAddresses;

        static NativeHelper()
        {
            _getProcAddressFunc = (_, _) => throw new InvalidOperationException();
            
            _lock = new object();
            _moduleAddresses = [];
        }

        internal static void Initialize(long getProcAddressAddress)
        {
            _getProcAddressFunc = ReloadedHooks.Instance.CreateWrapper<GetProcAddressDelegate>(getProcAddressAddress, out _);
        }

        internal static nuint GetModuleAddress(string moduleName)
        {
            lock (_lock)
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
        }

        internal static nuint GetProcAddress(string moduleName, string procName)
        {
            nuint module = GetModuleAddress(moduleName);
            return _getProcAddressFunc.Invoke(module, procName);
        }

        internal unsafe static void PatchPattern(string moduleName, byte[] originalBytes, byte[] replacementBytes)
        {
            Process process = Process.GetCurrentProcess();

            ProcessModule module = process.Modules
                .Cast<ProcessModule>()
                .Single(module => module.ModuleName == moduleName);

            // It'd be nicer to use the Scanner(process, processModule) constructor here, but it appears to reference an
            // old version of Reloaded.Memory.Sources.ExternalMemory that still relied on signed pointers, leading to a
            // MissingMethodException. Unsure if this is a package versioning issue on our end or on Reloaded's end.
            Scanner scanner = new Scanner((byte*)(void*)module.BaseAddress, module.ModuleMemorySize);

            string pattern = string.Join(' ', originalBytes.Select(b => b.ToString("X2")));
            PatternScanResult result = scanner.FindPattern(pattern);
            if (result.Found)
            {
                Memory.CurrentProcess.SafeWriteRaw(module.BaseAddress.ToUnsigned() + (nuint)result.Offset, replacementBytes);
            }
            else
            {
                throw new InvalidOperationException("Pattern not found");
            }
        }
    }
}
