using System;
using System.IO;
using System.Reflection;

namespace EarthRipperHook
{
    [Serializable]
    public class Settings : MarshalByRefObject
    {
        public string OutputDirectory { get; set; }
        public CaptureFlags CaptureFlags { get; set; }

        public Settings()
        {
            OutputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Output");
            CaptureFlags = CaptureFlags.All;
        }
    }

    [Flags]
    public enum CaptureFlags
    {
        Color = 1,
        Height = 2,
        MetaData = 4,

        All = Color | Height | MetaData
    }
}
