using Reloaded.Memory.Sources;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static EarthRipperHook.Native.Qt5Core;
using static EarthRipperHook.Native.Qt5Gui;

namespace EarthRipperHook.Capture
{
    internal static class CaptureWriter
    {
        internal static bool Write(CaptureTask captureTask, nuint qImage, nuint fileName, string format, int quality)
        {
            return captureTask.Format switch
            {
                OutputFormat.UserSpecified => Original<QImage.Save>()(qImage, fileName, format, captureTask.Quality ?? quality),
                OutputFormat.JPG => Original<QImage.Save>()(qImage, fileName, "JPG", captureTask.Quality ?? quality),
                OutputFormat.PNG => Original<QImage.Save>()(qImage, fileName, "PNG", -1),
                OutputFormat.PNG_Gray16 => Write16BitGrayscalePNG(qImage, fileName),
                _ => throw new NotImplementedException()
            };
        }

        private static bool Write16BitGrayscalePNG(nuint qImage, nuint fileName)
        {
            string path = new QString(fileName).ToString();
            int width = Original<QImage.Width>()(qImage);
            int height = Original<QImage.Height>()(qImage);
            int bytesPerLine = Original<QImage.BytesPerLine>()(qImage);
            int bytesPerPixel = bytesPerLine / width;
            int byteCount = bytesPerLine * height;

            nuint data = Original<QImage.ConstBits>()(qImage);
            Memory.CurrentProcess.ReadRaw(data, out byte[] bgraBytes, byteCount);

            List<byte> grayscaleBytes = new List<byte>(width * height * sizeof(ushort));
            for (int i = 0; i < byteCount; i += bytesPerPixel)
            {
                grayscaleBytes.Add(bgraBytes[i + 1]);
                grayscaleBytes.Add(bgraBytes[i + 2]);
            }

            BitmapSource bitmapSource = BitmapSource.Create(
                pixelWidth: width, pixelHeight: height,
                dpiX: 96.0, dpiY: 96.0,
                pixelFormat: PixelFormats.Gray16,
                palette: null,
                pixels: grayscaleBytes.ToArray(),
                stride: width * sizeof(ushort));

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using FileStream fileStream = File.OpenWrite(path);
            pngEncoder.Save(fileStream);

            return true;
        }
    }
}
