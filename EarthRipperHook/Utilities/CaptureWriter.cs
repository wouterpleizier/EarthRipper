using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarthRipperHook.Utilities
{
    internal static class CaptureWriter
    {
        public static void WriteToFile(CaptureResult capture, string outputDirectory, string fileName)
        {
            Directory.CreateDirectory(outputDirectory);

            if (capture.CaptureFlags.HasFlag(CaptureFlags.Height))
            {
                WriteHeightImage(capture, outputDirectory, fileName);
            }

            if (capture.CaptureFlags.HasFlag(CaptureFlags.Color))
            {
                WriteColorImage(capture, outputDirectory, fileName);
            }

            if (capture.CaptureFlags.HasFlag(CaptureFlags.MetaData))
            {
                WriteMetaData(capture, outputDirectory, fileName);
            }
        }

        private static void WriteHeightImage(CaptureResult capture, string outputDirectory, string fileName)
        {
            byte[] bytes = new byte[capture.ImageWidthInPixels * capture.ImageHeightInPixels * sizeof(ushort)];
            for (int i = 0; i < capture.Height.Length; i++)
            {
                ushort height = capture.Height[i];
                BitConverter.GetBytes(height).CopyTo(bytes, i * sizeof(ushort));
            }

            BitmapSource bitmapSource = BitmapSource.Create(capture.ImageWidthInPixels, capture.ImageHeightInPixels, 96, 96, PixelFormats.Gray16, null, bytes, capture.ImageWidthInPixels * sizeof(ushort));
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            string outputPath = Path.Combine(outputDirectory, $"{fileName}_height.png");
            using (FileStream fileStream = File.OpenWrite(outputPath))
            {
                pngEncoder.Save(fileStream);
            }
        }

        private static void WriteColorImage(CaptureResult capture, string outputDirectory, string fileName)
        {
            byte[] bytes = new byte[capture.ImageWidthInPixels * capture.ImageHeightInPixels * 3];
            for (int i = 0; i < capture.Color.Length; i++)
            {
                Color color = capture.Color[i];

                bytes[(i * 3)] = color.R;
                bytes[(i * 3) + 1] = color.G;
                bytes[(i * 3) + 2] = color.B;
            }

            BitmapSource bitmapSource = BitmapSource.Create(capture.ImageWidthInPixels, capture.ImageHeightInPixels, 96, 96, PixelFormats.Rgb24, null, bytes, capture.ImageWidthInPixels * sizeof(byte) * 3);
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            string outputPath = Path.Combine(outputDirectory, $"{fileName}_color.png");
            using (FileStream fileStream = File.OpenWrite(outputPath))
            {
                pngEncoder.Save(fileStream);
            }
        }

        private static void WriteMetaData(CaptureResult capture, string outputDirectory, string fileName)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (capture.CaptureFlags.HasFlag(CaptureFlags.Height))
            {
                stringBuilder.AppendLine($"Elevation in meters:     {capture.MinElevation.ToString(CultureInfo.InvariantCulture)} to {capture.MaxElevation.ToString(CultureInfo.InvariantCulture)}");
            }
            else
            {
                stringBuilder.AppendLine($"Elevation in meters:     unknown, no height captured");
            }

            stringBuilder.AppendLine($"Size in meters:          {capture.ImageWidthInMeters.ToString(CultureInfo.InvariantCulture)} x {capture.ImageHeightInMeters.ToString(CultureInfo.InvariantCulture)}");
            stringBuilder.AppendLine($"Top-left coordinate:     lat {capture.Bounds[0].Latitude.ToString(CultureInfo.InvariantCulture)}, lon {capture.Bounds[0].Longitude.ToString(CultureInfo.InvariantCulture)}");
            stringBuilder.AppendLine($"Top-right coordinate:    lat {capture.Bounds[1].Latitude.ToString(CultureInfo.InvariantCulture)}, lon {capture.Bounds[1].Longitude.ToString(CultureInfo.InvariantCulture)}");
            stringBuilder.AppendLine($"Bottom-left coordinate:  lat {capture.Bounds[2].Latitude.ToString(CultureInfo.InvariantCulture)}, lon {capture.Bounds[2].Longitude.ToString(CultureInfo.InvariantCulture)}");
            stringBuilder.AppendLine($"Bottom-right coordinate: lat {capture.Bounds[3].Latitude.ToString(CultureInfo.InvariantCulture)}, lon {capture.Bounds[3].Longitude.ToString(CultureInfo.InvariantCulture)}");

            string outputPath = Path.Combine(outputDirectory, $"{fileName}.txt");
            File.WriteAllText(outputPath, stringBuilder.ToString());
        }
    }
}
