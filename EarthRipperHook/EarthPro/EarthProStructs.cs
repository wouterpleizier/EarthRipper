using System.Runtime.InteropServices;

namespace EarthRipperHook.EarthPro
{
    internal static class EarthProStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct CoordinateData
        {
            internal Coordinate Coordinate;
            internal double Elevation;
        }
    }
}
