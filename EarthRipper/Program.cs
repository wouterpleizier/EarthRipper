using EarthRipperShared;
using Microsoft.Win32;
using Reloaded.Injector;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EarthRipper
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] nint processHandle, [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        const string TargetExecutableName = "googleearth";

        const string X86LibraryName = "EarthRipperHook32.dll";
        const string X86RunFunction = "_Run@4";

        const string X64LibraryName = "EarthRipperHook64.dll";
        const string X64RunFunction = "Run";

        static void Main(string[] args)
        {
            try
            {
                Process? targetProcess = GetTargetProcess(out bool targetIs64Bit);
                
                if (targetProcess == null)
                {
                    Log.Error("Failed to retrieve or launch target process. Exiting...");
                    Console.ReadKey();
                    return;
                }

                Log.Information("Injecting into Google Earth Pro...");
                string baseHookPath = typeof(EarthRipperHook.EntryPoint).Assembly.Location;
                string? hookDirectory = Path.GetDirectoryName(baseHookPath);
                string? hookName = targetIs64Bit ? X64LibraryName : X86LibraryName;

                string hookPath;
                if (hookDirectory == null || hookName == null || !Path.Exists(hookPath = Path.Combine(hookDirectory, hookName)))
                {
                    Log.Error($"Library {hookName} not found in {hookDirectory}. Exiting...");
                    Console.ReadKey();
                    return;
                }

                int currentProcessID = Process.GetCurrentProcess().Id;
                Log.Initialize(LogUtil.GetSharedLogName(currentProcessID));

                Injector injector = new Injector(targetProcess);
                long handle = injector.Inject(hookPath);
                if (handle == 0)
                {
                    Log.Error("Hook injection failed. Exiting...");
                    Console.ReadKey();
                    return;
                }

                RunResult result = (RunResult)injector.CallFunction(
                    hookName,
                    targetIs64Bit ? X64RunFunction : X86RunFunction,
                    new RunArgs()
                    {
                        InjectorProcessID = currentProcessID,
                        GetProcAddressAddress = injector.ShellCode.GetProcAddressAddress,
                    });

                if (result == RunResult.Success)
                {
                    Log.Information("Hook initialized");
                }
                else
                {
                    Log.Error($"Hook initialization failed ({result})");
                }

                while (!targetProcess.HasExited)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return;
            }
        }

        private static Process? GetTargetProcess(out bool is64Bit)
        {
            Process? process = Process.GetProcessesByName(TargetExecutableName).FirstOrDefault();
            if (process != null)
            {
                Log.Information("Google Earth Pro process found");
            }
            else
            {
                string? path = GetInstallationPathFromRegistry();
                if (!Path.Exists(path))
                {
                    Log.Information($"Failed to launch Google Earth Pro automatically because no valid installation path could be found. Start {TargetExecutableName}.exe manually if it is present, or (re)install if not.");

                    while (process == null)
                    {
                        Thread.Sleep(2000);
                        process = Process.GetProcessesByName(TargetExecutableName).FirstOrDefault();
                    }
                }
                else
                {
                    Log.Information("Launching Google Earth Pro...");
                    process = Process.Start(path);
                }
            }

            if (Environment.Is64BitOperatingSystem)
            {
                if (IsWow64Process(process.Handle, out bool isWow64Process))
                {
                    is64Bit = !isWow64Process;
                }
                else
                {
                    Log.Error("Unable to determine whether Google Earth Pro process is 32-bit or 64-bit",
                        new Win32Exception(Marshal.GetLastWin32Error()));

                    is64Bit = default;
                    return null;
                }
            }
            else
            {
                is64Bit = false;
            }

            // Wait for OpenSSL to load, as this indicates that the application has more or less finished launching.
            bool waitingForModule = false;
            while (!process.Modules.Cast<ProcessModule>().Any(module => module.ModuleName == "ssleay32.dll"))
            {
                if (!waitingForModule)
                {
                    Log.Information("Waiting for required modules to load...");
                    waitingForModule = true;
                }

                Thread.Sleep(100);
                process.Refresh();
            }

            // Need a bit of a delay here to ensure that various shaders have had a chance to compile and render. Not
            // foolproof but it works on my machine™.
            if (waitingForModule)
            {
                Thread.Sleep(2000);
            }

            return process;
        }

        private static string? GetInstallationPathFromRegistry()
        {
            RegistryView[] registryViews = Environment.Is64BitOperatingSystem
                ? [RegistryView.Registry64, RegistryView.Registry32]
                : [RegistryView.Registry32];

            foreach (RegistryView registryView in registryViews)
            {
                RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);
                if (baseKey.OpenSubKey("SOFTWARE\\Google\\Google Earth Pro") is RegistryKey key
                    && key.GetValue("InstallLocation") is string installLocation)
                {
                    return Path.Combine(installLocation, TargetExecutableName + ".exe");
                }
            }

            return null;
        }
    }
}
