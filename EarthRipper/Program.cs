using EarthRipperShared;
using Microsoft.Win32;
using Reloaded.Injector;
using System.Diagnostics;

namespace EarthRipper
{
    internal class Program
    {
        const string TargetExecutableName = "googleearth.exe";

        static void Main(string[] args)
        {
            try
            {
                bool targetIs64Bit;
                string? targetPath;
                if (Environment.Is64BitOperatingSystem && TryGetTargetPath(RegistryView.Registry64, out targetPath))
                {
                    targetIs64Bit = true;
                }
                else if (TryGetTargetPath(RegistryView.Registry32, out targetPath))
                {
                    targetIs64Bit = false;
                }
                else
                {
                    Log.Error("Unable to determine installation location of Google Earth Pro. Try reinstalling.");
                    return;
                }

                if (!Path.Exists(targetPath))
                {
                    Log.Error($"{TargetExecutableName} not found at installation location. Try reinstalling.");
                    return;
                }

                Process? process = Process.GetProcessesByName("googleearth").FirstOrDefault();
                if (process == null)
                {
                    Log.Information("Launching Google Earth Pro...");
                    process = Process.Start(targetPath);

                    //process = Process.Start(new ProcessStartInfo(targetPath)
                    //{
                    //    UseShellExecute = false,
                    //    RedirectStandardOutput = true,
                    //    RedirectStandardError = true
                    //});
                    //process.OutputDataReceived += (sender, e) => { if (e.Data != null) { Logger.LogInformation(e.Data); } };
                    //process.BeginOutputReadLine();

                    //process.ErrorDataReceived += (sender, e) => { if (e.Data != null) { Logger.LogError(e.Data); } };
                    //process.BeginErrorReadLine();
                }

                Log.Information("Injecting into Google Earth Pro...");
                string baseHookPath = typeof(EarthRipperHook.EntryPoint).Assembly.Location;
                string? hookDirectory = Path.GetDirectoryName(baseHookPath);
                string? hookName = string.Concat(
                    Path.GetFileNameWithoutExtension(baseHookPath),
                    "NE", //targetIs64Bit ? "64" : "32",
                    Path.GetExtension(baseHookPath));

                string hookPath;
                if (hookDirectory == null || hookName == null || !Path.Exists(hookPath = Path.Combine(hookDirectory, hookName)))
                {
                    Log.Error($"Unable to resolve hook library path");
                    return;
                }

                int currentProcessID = Process.GetCurrentProcess().Id;
                Log.Initialize(LogUtil.GetSharedLogName(currentProcessID));

                Injector injector = new Injector(process);
                long handle = injector.Inject(Path.Combine(hookDirectory, hookName));
                if (handle == 0)
                {
                    Log.Error("Hook injection failed");
                }

                RunResult result = (RunResult)injector.CallFunction(
                    hookName, "_Run@4",
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

                while (!process.HasExited)
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

        private static bool TryGetTargetPath(RegistryView registryView, out string? path)
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);
            if (baseKey.OpenSubKey("SOFTWARE\\Google\\Google Earth Pro") is RegistryKey key
                && key.GetValue("InstallLocation") is string installLocation)
            {
                path = Path.Combine(installLocation, TargetExecutableName);
                return true;
            }

            path = default;
            return false;
        }
    }
}
