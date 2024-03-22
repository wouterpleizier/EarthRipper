using Reloaded.Memory.Sources;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static EarthRipperHook.Native.IGAttrs;
using static EarthRipperHook.Native.IGGfx;
using static EarthRipperHook.Native.IGMath;
using static EarthRipperHook.Native.Qt5Core;
using static EarthRipperHook.Native.Qt5Gui;
using static EarthRipperHook.Native.Qt5Widgets;

namespace EarthRipperHook.Capture
{
    internal class CaptureHook
    {
        private readonly string _captureTemplatesPath;
        private readonly Queue<QString> _allocatedQStrings;

        public CaptureHook()
        {
            string hookPath = Path.GetDirectoryName(typeof(CaptureHook).Assembly.Location)
                ?? throw new Exception("Unable to determine capture templates path");

            _captureTemplatesPath = Path.Combine(hookPath, "CaptureTemplates");
            _allocatedQStrings = [];

            Hook<QAbstractButton.Clicked>(HandleButtonClicked);
        }

        private void HandleButtonClicked(nuint qAbstractButton, bool isChecked)
        {
            Queue<CaptureTask>? queuedCaptures = InvokeClickAndPrepareCapture(qAbstractButton, isChecked);

            while (queuedCaptures?.Count > 0)
            {
                CaptureTask captureTask = queuedCaptures.Dequeue();
                InvokeClickAndPerformCapture(qAbstractButton, isChecked, captureTask);
            }

            // Don't dispose the last allocated QString yet, as it may get accessed the next time the dialog opens.
            while (_allocatedQStrings.Count > 1)
            {
                QString qString = _allocatedQStrings.Dequeue();
                qString.Dispose();
            }
        }

        private Queue<CaptureTask>? InvokeClickAndPrepareCapture(nuint qAbstractButton, bool isChecked)
        {
            Queue<CaptureTask>? queuedCaptures = null;
            nuint InvokeOriginalAndGetCaptureQueue(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter)
            {
                // Filters are localized, but we can always count on the extensions being present.
                string filterAsString = new QString(filter, 1).ToString();
                if (filterAsString.Contains("*.jpg") && filterAsString.Contains("*.png"))
                {
                    nuint path = Original<QFileDialog.GetSaveFileName>()(result, parentWidget, caption, dir, filter, selectedFilter);
                    SuppressOriginal<QFileDialog.GetSaveFileName>(path);

                    string saveImagePath = new QString(path, 1).ToString();
                    queuedCaptures = new Queue<CaptureTask>(GetCaptureTasks(saveImagePath));
                }

                return default;
            }

            bool SuppressSaveIfCapturesAreQueued(nuint qImage, nuint fileName, string format, int quality)
            {
                if (queuedCaptures?.Count > 0)
                {
                    SuppressOriginal<QImage.Save>(true);
                    return true;
                }
                else
                {
                    Log.Information("No capture template(s) specified, saving image with default behavior instead");
                    return default;
                }
            }

            using var pathToken = Hook<QFileDialog.GetSaveFileName>(InvokeOriginalAndGetCaptureQueue, exclusive: true);
            using var saveToken = Hook<QImage.Save>(SuppressSaveIfCapturesAreQueued, exclusive: true);

            // Call these regardless of exclusive access, otherwise no buttons will work anymore.
            Original<QAbstractButton.Clicked>()(qAbstractButton, isChecked);
            SuppressOriginal<QAbstractButton.Clicked>();

            return queuedCaptures;
        }

        private IEnumerable<CaptureTask> GetCaptureTasks(string saveImagePath)
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

            foreach (string captureTemplateName in fileNameParts.Skip(1))
            {
                CaptureTask? captureTask = null;
                string captureTemplateDirectory = Path.Combine(_captureTemplatesPath, captureTemplateName);
                string captureTemplatePath = Path.Combine(captureTemplateDirectory, "Settings.json");
                if (File.Exists(captureTemplatePath))
                {
                    try
                    {
                        JsonSerializerOptions options = new() { AllowTrailingCommas = true };
                        options.Converters.Add(new JsonStringEnumConverter());

                        captureTask = JsonSerializer.Deserialize<CaptureTask>(File.ReadAllText(captureTemplatePath),
                            options);
                    }
                    catch (Exception exception)
                    {
                        Log.Error($"Unable to parse capture template {captureTemplateName}; skipping...");
                        Log.Error(exception);
                    }
                }
                else
                {
                    Log.Warning($"Capture template {captureTemplateName} does not exist; skipping...");
                }

                if (captureTask != null)
                {
                    string path = new StringBuilder(captureTask.Path)
                        .Replace(CaptureTask.DirectoryIdentifier, baseDirectory)
                        .Replace(CaptureTask.FileNameIdentifier, baseName)
                        .Append(captureTask.Format switch
                        {
                            OutputFormat.JPG => ".jpg",
                            OutputFormat.PNG or OutputFormat.PNG_Gray16 => ".png",
                            _ => baseExtension
                        })
                        .ToString();

                    Dictionary<string, string> customShaders = [];
                    foreach (string customShaderPath in Directory.EnumerateFiles(captureTemplateDirectory, "*.glsl"))
                    {
                        string shaderName = Path.GetFileNameWithoutExtension(customShaderPath);
                        string shaderSource = File.ReadAllText(customShaderPath);
                        customShaders[shaderName] = shaderSource;
                    }

                    yield return captureTask with { Path = path, CustomShaders = customShaders };
                }
            }
        }

        private Dictionary<string, DisposableToken> _activeShaderOverrides = [];
        private bool InvokeClickAndPerformCapture(nuint qAbstractButton, bool isChecked, CaptureTask captureTask)
        {
            foreach (var shaderOverride in _activeShaderOverrides.Values)
            {
                shaderOverride.Dispose();
            }
            _activeShaderOverrides.Clear();

            Dictionary<int, bool> shouldDrawKnownShaders = [];

            // Some shaders don't pass through igProgramAttr::bind(), so we don't know their names. In these cases we
            // should only draw when all shaders are allowed, or when only specific (named) shaders are blocked.
            bool shouldDrawUnknownShaders = captureTask.DefaultShaderHandling
                is DefaultShaderHandling.AllowAll
                or DefaultShaderHandling.BlockSpecified;
            
            bool imageWasSaved = false;

            nuint OverrideSaveImagePath(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter)
            {
                QString pathAsQString = new QString(captureTask.Path);
                _allocatedQStrings.Enqueue(pathAsQString);

                SuppressOriginal<QFileDialog.GetSaveFileName>(pathAsQString.NativeQStringData);
                Memory.CurrentProcess.Write(result, pathAsQString.NativeQStringData);
                return result;
            }

            bool OverrideBind(nuint igProgramAttr, nuint igVisualContext)
            {
                int shaderHandle = Original<IGProgramAttr.GetProgramHandle>()(igProgramAttr);
                string shaderName = Original<IGProgramAttr.GetName>()(igProgramAttr).AsAnsiString();

                if (captureTask.CustomShaders?.GetValueOrDefault(shaderName) is string customShaderSource)
                {
                    shouldDrawKnownShaders[shaderHandle] = true;

                    if (!_activeShaderOverrides.ContainsKey(shaderName))
                    {
                        Log.Information("Overriding shader " + shaderName);
                        var shaderOverride = ShaderHelper.OverrideShader(shaderName, customShaderSource);
                        if (shaderOverride != null)
                        {
                            _activeShaderOverrides[shaderName] = shaderOverride;
                        }
                        else
                        {
                            Log.Error("Unable to override shader " + shaderName);
                        }
                    }
                }
                else
                {
                    shouldDrawKnownShaders[shaderHandle] = captureTask.DefaultShaderHandling switch
                    {
                        DefaultShaderHandling.AllowAll => true,
                        DefaultShaderHandling.AllowSpecified => captureTask.DefaultShaders?.Contains(shaderName) is true,
                        DefaultShaderHandling.BlockSpecified => captureTask.DefaultShaders?.Contains(shaderName) is false or null,
                        DefaultShaderHandling.BlockAll => false,

                        _ => throw new NotImplementedException()
                    };
                }

                return default;
            }

            void OverrideClearColor(nuint igClearAttr, nuint igVec4f)
            {
                // The default clear color is a dark gray, but black makes more sense when capturing elevation.
                Original<IGVec4f.Set>()(igVec4f, 0f, 0f, 0f, 0f);
            }

            void OverrideGenericDraw(nuint igOglVisualContext, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5)
            {
                int shaderHandle = Original<IGOglVisualContext.GetCurrentProgramHandle>()(igOglVisualContext);
                if (!shouldDrawKnownShaders.TryGetValue(shaderHandle, out bool shouldDraw))
                {
                    shouldDraw = shouldDrawUnknownShaders;
                }

                if (!shouldDraw)
                {
                    SuppressOriginal<IGOglVisualContext.GenericDraw>();
                }
            }

            void OverrideRender(nuint qGraphicsView, nuint qPainter, nuint targetQRectF, nuint sourceQRect, int aspectRatioMode)
            {
                if (!captureTask.AllowImageOverlays)
                {
                    SuppressOriginal<QGraphicsView.Render>();
                }
            }

            bool OverrideSaveImage(nuint qImage, nuint fileName, string format, int quality)
            {
                CaptureWriter.Write(captureTask, qImage, fileName, format, quality);

                SuppressOriginal<QImage.Save>(true);
                imageWasSaved = true;
                return true;
            }

            try
            {
                using var bindToken = Hook<IGProgramAttr.Bind>(OverrideBind, exclusive: false, throwOnFailure: true);

                using var clearToken = Hook<IGClearAttr.SetColor>(OverrideClearColor, exclusive: true, throwOnFailure: true);
                using var drawToken = Hook<IGOglVisualContext.GenericDraw>(OverrideGenericDraw, exclusive: true, throwOnFailure: true);
                using var drawImageToken = Hook<QGraphicsView.Render>(OverrideRender, exclusive: true, throwOnFailure: true);
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
