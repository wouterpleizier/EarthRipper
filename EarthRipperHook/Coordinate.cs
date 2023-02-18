using System;

namespace EarthRipperHook
{
    internal struct Coordinate
    {
        public double Longitude;
        public double Latitude;

        public Coordinate(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public static double GetDistance(Coordinate from, Coordinate to)
        {
            double degreesToRadians = Math.PI / 180.0;

            double d1 = from.Latitude * degreesToRadians;
            double num1 = from.Longitude * degreesToRadians;
            double d2 = to.Latitude * degreesToRadians;
            double num2 = to.Longitude * degreesToRadians - num1;
            double d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
