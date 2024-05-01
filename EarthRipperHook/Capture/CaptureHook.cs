using EarthRipperHook.RenderPreset;
using Reloaded.Memory.Sources;
using System.IO;
using System.Text;
using static EarthRipperHook.Native.Qt5Core;
using static EarthRipperHook.Native.Qt5Gui;
using static EarthRipperHook.Native.Qt5Widgets;

namespace EarthRipperHook.Capture
{
    internal class CaptureHook
    {
        private readonly Queue<QString> _allocatedQStrings = [];

        public CaptureHook()
        {
            PatchMaximumCaptureSize();

            Hook<QAbstractButton.Clicked>(HandleButtonClicked);
        }

        private static void PatchMaximumCaptureSize()
        {
            // The render destination for saving images is clamped to 5000x5000 pixels by default, and images larger
            // than this are rendered in parts. For example, when the desired image size is 8192x4096, a 5000x4096
            // region is first rendered, followed by the remaining 3192x4096 region. This works fine normally, but we
            // may be using custom shaders that cause geometry to render differently (e.g. with an elevation of 0) than
            // what the frustrum culler anticipated. This can then lead to map tiles disappearing near the borders of
            // each render region.

            // The code below initializes the maximum and minimum values that are provided to the clamping function
            // before the render destination is created. We'll change the maximum value from 5000 to 8192 (the current
            // maximum image size), effectively causing all captures to render in a single pass.

            if (nuint.Size == 4)
            {
                NativeHelper.PatchPattern("googleearth_pro.dll",
                [
                    0xc7, 0x45, 0xe4, 0x88, 0x13, 0x00, 0x00, // mov dword ptr [ebp-0x1c], 0x1388 (5000)
                    0xc7, 0x45, 0xe0, 0x00, 0x04, 0x00, 0x00, // mov dword ptr [ebp-0x20], 0x0400 (1024)
                ],
                [
                    0xc7, 0x45, 0xe4, 0x00, 0x20, 0x00, 0x00, // mov dword ptr [ebp-0x1c], 0x2000 (8192)
                    0xc7, 0x45, 0xe0, 0x00, 0x04, 0x00, 0x00, // mov dword ptr [ebp-0x20], 0x0400 (1024)
                ]);
            }
            else
            {
                NativeHelper.PatchPattern("googleearth_pro.dll",
                [
                    0xc7, 0x44, 0x24, 0x34, 0x88, 0x13, 0x00, 0x00, // mov dword ptr [rsp+34], 0x1388 (5000)
                    0xc7, 0x44, 0x24, 0x38, 0x00, 0x04, 0x00, 0x00, // mov dword ptr [rsp+38], 0x0400 (1024)
                ],
                [
                    0xc7, 0x44, 0x24, 0x34, 0x00, 0x20, 0x00, 0x00, // mov dword ptr [rsp+34], 0x2000 (8192)
                    0xc7, 0x44, 0x24, 0x38, 0x00, 0x04, 0x00, 0x00, // mov dword ptr [rsp+38], 0x0400 (1024)
                ]);
            }
        }

        private void HandleButtonClicked(nuint qAbstractButton, bool isChecked)
        {
            if (InvokeClickAndTryGetSaveImagePath(qAbstractButton, isChecked) is string saveImagePath)
            {
                foreach ((RenderPresetDefinition renderPreset, string outputPath) in GetCaptureTasks(saveImagePath))
                {
                    try
                    {
                        RenderPresetManager.ActivateRenderPreset(renderPreset, RenderPresetContext.Capture);
                        InvokeClickAndPerformCapture(qAbstractButton, isChecked, renderPreset, outputPath);
                    }
                    finally
                    {
                        RenderPresetManager.DeactivateRenderPreset(RenderPresetContext.Capture);
                    }
                }
            }

            // Don't dispose the last allocated QString yet, as it may get accessed the next time the dialog opens.
            while (_allocatedQStrings.Count > 1)
            {
                QString qString = _allocatedQStrings.Dequeue();
                qString.Dispose();
            }
        }

        private string? InvokeClickAndTryGetSaveImagePath(nuint qAbstractButton, bool isChecked)
        {
            string? saveImagePath = null;

            nuint CaptureSaveImagePath(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter)
            {
                // Filters are localized, but we can always count on the extensions being present.
                string filterAsString = new QString(filter).ToString();
                if (filterAsString.Contains("*.jpg") && filterAsString.Contains("*.png"))
                {
                    nuint path = Original<QFileDialog.GetSaveFileName>()(result, parentWidget, caption, dir, filter, selectedFilter);
                    SuppressOriginal<QFileDialog.GetSaveFileName>(path);

                    saveImagePath = new QString(path).ToString();
                }

                return default;
            }

            bool SuppressSaveImage(nuint qImage, nuint fileName, string format, int quality)
            {
                SuppressOriginal<QImage.Save>(true);
                return true;
            }

            using var pathToken = Hook<QFileDialog.GetSaveFileName>(CaptureSaveImagePath, exclusive: true);
            using var saveToken = Hook<QImage.Save>(SuppressSaveImage, exclusive: true);

            // Call these regardless of exclusive access, otherwise no buttons will work anymore.
            Original<QAbstractButton.Clicked>()(qAbstractButton, isChecked);
            SuppressOriginal<QAbstractButton.Clicked>();

            return saveImagePath;
        }

        private static IEnumerable<(RenderPresetDefinition renderPreset, string outputPath)> GetCaptureTasks(string saveImagePath)
        {
            string[] fileNameParts = Path.GetFileNameWithoutExtension(saveImagePath)
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string? baseDirectory = Path.GetDirectoryName(saveImagePath);
            string? baseExtension = Path.GetExtension(saveImagePath);
            string? baseName = fileNameParts.FirstOrDefault();
            if (baseDirectory == null || baseName == null || baseExtension == null)
            {
                Log.Error($"Unable to parse path {saveImagePath}");
                yield break;
            }

            bool validRenderPresetWasSpecified = false;
            foreach (string renderPresetName in fileNameParts.Skip(1))
            {
                if (RenderPresetManager.GetRenderPreset(renderPresetName, true) is RenderPresetDefinition renderPreset)
                {
                    string outputPath = new StringBuilder(renderPreset.OutputPath)
                        .Replace(RenderPresetDefinition.DirectoryIdentifier, baseDirectory)
                        .Replace(RenderPresetDefinition.FileNameIdentifier, baseName)
                        .Append(renderPreset.OutputFormat switch
                        {
                            OutputFormat.JPG => ".jpg",
                            OutputFormat.PNG or OutputFormat.PNG_Gray16 => ".png",
                            _ => baseExtension
                        })
                        .ToString();

                    validRenderPresetWasSpecified = true;
                    yield return (renderPreset, outputPath);
                }
                else
                {
                    Log.Warning($"Render preset {renderPresetName} not found; ignoring...");
                }
            }

            // If no valid render preset was specified, use the current one. We still need to reactivate/deactivate it
            // in that case, as it may use custom shaders that rely on context-specific defines.
            if (!validRenderPresetWasSpecified)
            {
                RenderPresetDefinition renderPreset = RenderPresetManager.GetCurrentRenderPreset();

                string? providedExtension = Path.GetExtension(saveImagePath);
                string? expectedExtension = renderPreset.OutputFormat switch
                {
                    OutputFormat.JPG => ".jpg",
                    OutputFormat.PNG or OutputFormat.PNG_Gray16 => ".png",
                    _ => null
                };

                if (expectedExtension != null && providedExtension != expectedExtension)
                {
                    Log.Warning($"File extension {providedExtension} is not allowed by render preset {renderPreset.Name}; using {expectedExtension} instead");
                    saveImagePath = Path.ChangeExtension(saveImagePath, expectedExtension);
                }

                yield return (RenderPresetManager.GetCurrentRenderPreset(), saveImagePath);
            }
        }

        private bool InvokeClickAndPerformCapture(nuint qAbstractButton, bool isChecked, RenderPresetDefinition renderPreset, string outputPath)
        {
            bool imageWasSaved = false;

            nuint OverrideSaveImagePath(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter)
            {
                QString pathAsQString = new QString(outputPath);
                _allocatedQStrings.Enqueue(pathAsQString);

                SuppressOriginal<QFileDialog.GetSaveFileName>(pathAsQString.NativeQStringData);
                Memory.CurrentProcess.Write(result, pathAsQString.NativeQStringData);
                return result;
            }

            bool OverrideSaveImage(nuint qImage, nuint fileName, string format, int quality)
            {
                CaptureWriter.Write(renderPreset, qImage, fileName, format, quality);

                SuppressOriginal<QImage.Save>(true);
                imageWasSaved = true;
                return true;
            }

            try
            {
                using var pathToken = Hook<QFileDialog.GetSaveFileName>(OverrideSaveImagePath, exclusive: true, throwOnFailure: true);
                using var saveToken = Hook<QImage.Save>(OverrideSaveImage, exclusive: true, throwOnFailure: true);

                Original<QAbstractButton.Clicked>()(qAbstractButton, isChecked);
                SuppressOriginal<QAbstractButton.Clicked>();
            }
            catch (Exception exception)
            {
                Log.Error("Capture failed:");
                Log.Error(exception);
            }

            return imageWasSaved;
        }
    }
}
