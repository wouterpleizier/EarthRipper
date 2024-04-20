using EarthRipperHook.RenderPreset;
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
        internal static bool Write(RenderPresetDefinition renderPreset, nuint qImage, nuint fileName, string format, int quality)
        {
            int renderWidth = Original<QImage.Width>()(qImage);
            int renderHeight = Original<QImage.Height>()(qImage);
            int bytesPerLine = Original<QImage.BytesPerLine>()(qImage);
            int bytesPerPixel = bytesPerLine / renderWidth;
            int byteCount = bytesPerLine * renderHeight;

            nuint data = Original<QImage.ConstBits>()(qImage);
            Memory.CurrentProcess.ReadRaw(data, out byte[] bgraBytes, byteCount);

            List<byte> bytes = [];
            if (renderPreset.OutputFormat == OutputFormat.PNG_Gray16)
            {
                for (int i = 0; i < byteCount; i += bytesPerPixel)
                {
                    bytes.Add(bgraBytes[i + 1]);
                    bytes.Add(bgraBytes[i + 2]);
                }
            }
            else
            {
                for (int i = 0; i < byteCount; i += bytesPerPixel)
                {
                    bytes.Add(bgraBytes[i + 2]);
                    bytes.Add(bgraBytes[i + 1]);
                    bytes.Add(bgraBytes[i]);
                }
            }

            (PixelFormat pixelFormat, int stride) = renderPreset.OutputFormat switch
            {
                OutputFormat.UserSpecified or OutputFormat.JPG or OutputFormat.PNG =>
                    (PixelFormats.Rgb24,
                    renderWidth * sizeof(byte) * 3),

                OutputFormat.PNG_Gray16 =>
                    (PixelFormats.Gray16,
                    renderWidth * sizeof(byte) * 2),

                _ => throw new NotImplementedException()
            };

            BitmapSource bitmapSource = BitmapSource.Create(
                pixelWidth: renderWidth, pixelHeight: renderHeight,
                dpiX: 96.0, dpiY: 96.0,
                pixelFormat: pixelFormat,
                palette: null,
                pixels: bytes.ToArray(),
                stride: stride);

            (int outputWidth, int outputHeight) = GetScaledOutputSize(renderPreset, renderWidth, renderHeight);
            if (outputWidth != renderWidth || outputHeight != renderHeight)
            {
                double scaleX = (double)outputWidth / renderWidth;
                double scaleY = (double)outputHeight / renderHeight;
                bitmapSource = new TransformedBitmap(bitmapSource, new ScaleTransform(scaleX, scaleY));
            }

            BitmapEncoder encoder;
            if ((renderPreset.OutputFormat is OutputFormat.UserSpecified && format == "JPG")
                || renderPreset.OutputFormat is OutputFormat.JPG)
            {
                encoder = new JpegBitmapEncoder() { QualityLevel = renderPreset.OutputQuality ?? quality };
            }
            else if ((renderPreset.OutputFormat is OutputFormat.UserSpecified && format == "PNG")
                || renderPreset.OutputFormat is OutputFormat.PNG or OutputFormat.PNG_Gray16)
            {
                encoder = new PngBitmapEncoder();
            }
            else
            {
                throw new NotImplementedException();
            }

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            string outputPath = new QString(fileName).ToString();
            using FileStream fileStream = File.OpenWrite(outputPath);
            
            encoder.Save(fileStream);

            return true;
        }

        private static (int width, int height) GetScaledOutputSize(RenderPresetDefinition renderPreset,
                                                                   int renderWidth,
                                                                   int renderHeight)
        {
            double width = renderWidth;
            double height = renderHeight;

            if (renderPreset.OutputScaleFactor is not null and > 0.0 and < 1.0)
            {
                width *= renderPreset.OutputScaleFactor.Value;
                height *= renderPreset.OutputScaleFactor.Value;
            }

            if (renderPreset.OutputMaxWidth is not null and > 0 && width > renderPreset.OutputMaxWidth)
            {
                double scaleFactor = renderPreset.OutputMaxWidth.Value / width;
                width *= scaleFactor;
                height *= scaleFactor;
            }

            if (renderPreset.OutputMaxHeight is not null and > 0 && height > renderPreset.OutputMaxHeight)
            {
                double scaleFactor = renderPreset.OutputMaxHeight.Value / height;
                width *= scaleFactor;
                height *= scaleFactor;
            }

            return (Convert.ToInt32(width), Convert.ToInt32(height));
        }
    }
}
