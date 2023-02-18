using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace EarthRipper
{
    internal class Program
    {
        static void Main(string[] arguments)
        {
            EarthRipperHook.Settings settings = ParseArguments(arguments);
            if (settings == null)
            {
                return;
            }

            var process = Process.GetProcessesByName("googleearth").FirstOrDefault();
            if (process == null)
            {
                WriteError("Google Earth Pro is not running");
                Console.ReadKey();
                return;
            }

            string channelName = null;
            EarthRipperHook.ServerInterface serverInterface = new EarthRipperHook.ServerInterface();
            EasyHook.RemoteHooking.IpcCreateServer(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, serverInterface);

            string injectionLibraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EarthRipperHook.dll");

            try
            {
                Console.WriteLine("Attempting to inject into Google Earth Pro (process ID: {0})", process.Id);
                EasyHook.RemoteHooking.Inject(process.Id, injectionLibraryPath, injectionLibraryPath, channelName, settings);
            }
            catch (Exception exception)
            {
                WriteError($"An error occurred while injecting into the process:\n{exception}");
                Console.ReadKey();
            }

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static EarthRipperHook.Settings ParseArguments(string[] arguments)
        {
            EarthRipperHook.Settings settings = new EarthRipperHook.Settings();

            Queue<string> queue = new Queue<string>(arguments);
            string fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            while (queue.Count > 0)
            {
                string argument = queue.Dequeue().ToLower();

                try
                {
                    switch (argument)
                    {
                        case "?":
                        case "/?":
                        case "help":
                        case "-help":
                            Console.WriteLine(
$@"Usage: {fileName} [options]
Options:
-output <path>      Set capture output directory to <path>. Defaults to a subfolder named Output in the executable's directory.
-capture <flags>    Specify which things should be captured and saved to disk. <flags> is a comma-separated list of one or more of these values: color, height, metadata. Defaults to all.");
                            return null;

                        case "-output":
                            settings.OutputDirectory = queue.Dequeue();
                            break;

                        case "-capture":
                            string captureFlagsArgument = queue.Dequeue().ToLower();
                            if (Enum.TryParse(captureFlagsArgument, true, out EarthRipperHook.CaptureFlags captureFlags))
                            {
                                settings.CaptureFlags = captureFlags;
                            }
                            else
                            {
                                WriteError($"Unable to parse capture flags {captureFlagsArgument}. Make sure that values are separated by commas without spaces. Allowed values: color height metadata");
                            }
                            break;

                        default:
                            WriteError($"Unknown argument {argument}. For usage, run {fileName} -help");
                            break;
                    }
                }
                catch (InvalidOperationException exception)
                {
                    WriteError($"Unable to parse launch arguments:\n{exception}");
                }
            }

            return settings;
        }
    }
}
