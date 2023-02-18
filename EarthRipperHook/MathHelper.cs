using System;

namespace EarthRipperHook
{
    internal static class MathHelper
    {
        public static bool ApproximatelyEquals(float a, float b, float maxDifference = 0.01f)
        {
            return (a > b - maxDifference) && (a < b + maxDifference);
        }

        public static ushort RemapToUShort(double value, double fromMin, double fromMax)
        {
            double toMin = ushort.MinValue;
            double toMax = ushort.MaxValue;

            double remapped = toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
            int rounded = Convert.ToInt32(remapped);

            if (rounded > ushort.MaxValue)
            {
                return ushort.MaxValue;
            }
            else if (rounded < ushort.MinValue)
            {
                return ushort.MinValue;
            }
            else
            {
                return (ushort)rounded;
            }
        }
    }
}
