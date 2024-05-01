using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static EarthRipperHook.Native.IGAttrs;
using static EarthRipperHook.Native.IGGfx;
using static EarthRipperHook.Native.IGMath;
using static EarthRipperHook.Native.Qt5Widgets;

namespace EarthRipperHook.RenderPreset
{
    internal static class RenderPresetManager
    {
        internal delegate void RenderPresetEventHandler(RenderPresetDefinition renderPreset);
        internal delegate void RenderPresetContextEventHandler(RenderPresetDefinition renderPreset, RenderPresetContext context);

        internal static event RenderPresetEventHandler? RenderPresetAdded;
        internal static event RenderPresetEventHandler? RenderPresetUpdated;
        internal static event RenderPresetEventHandler? RenderPresetRemoved;
        internal static event RenderPresetContextEventHandler? RenderPresetActivated;
        internal static event RenderPresetContextEventHandler? RenderPresetApplied;
        internal static event RenderPresetContextEventHandler? RenderPresetDeactivated;

        private static readonly object _lock = new object();

        private static readonly StringComparer _renderPresetNameComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly Dictionary<string, RenderPresetDefinition> _renderPresets = new(_renderPresetNameComparer);
        private static readonly SortedDictionary<RenderPresetContext, RenderPresetDefinition> _activeRenderPresets = [];

        private static readonly Dictionary<uint, Shader> _knownShaders = [];

        private static RenderPresetPreviewer? _renderPresetPreviewer;
        private static JsonSerializerOptions? _jsonSerializerOptions;
        private static string? _renderPresetsDirectory;

        internal static void Initialize()
        {
            _renderPresetPreviewer = new RenderPresetPreviewer();

            _jsonSerializerOptions = new JsonSerializerOptions() { AllowTrailingCommas = true };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            _renderPresetsDirectory = Path.Combine(
                Path.GetDirectoryName(typeof(RenderPresetManager).Assembly.Location)
                    ?? throw new Exception("Unable to determine RenderPresets path"),
                "RenderPresets");

            foreach (string renderPresetDirectory in Directory.EnumerateDirectories(_renderPresetsDirectory))
            {
                string name = new DirectoryInfo(renderPresetDirectory).Name;
                UpdateRenderPreset(name);
            }

            if (!_renderPresets.ContainsKey(RenderPresetDefinition.DefaultName))
            {
                _renderPresets[RenderPresetDefinition.DefaultName] = RenderPresetDefinition.DefaultFallback;
            }

            // Initializing these when we first need them results in stack corruption, so do it here.
            EnsureInitialized<IGProgramAttr.SetVertexSource>();
            EnsureInitialized<IGProgramAttr.SetFragmentSource>();

            Hook<IGProgramAttr.Bind>(InitializeShaderOnBind);
            Hook<IGAttrContext.SetClearColor>(OverrideClearColor);
            Hook<IGOglVisualContext.GenericDraw>(Suppress3DRendering);
            Hook<QGraphicsView.Render>(SuppressImageOverlay);
        }

        private static bool InitializeShaderOnBind(nuint igProgramAttr, nuint igVisualContext)
        {
            lock (_lock)
            {
                uint handle = (uint)Original<IGProgramAttr.GetProgramHandle>()(igProgramAttr);
                if (!_knownShaders.TryGetValue(handle, out Shader? shader))
                {
                    shader = new Shader(igProgramAttr, handle);
                    _knownShaders[handle] = shader;
                }

                return default;
            }
        }

        private static void OverrideClearColor(nuint igAttrContext, nuint igVec4f)
        {
            if (GetCurrentRenderPreset().ClearColor is double[] values && values.Length == 3)
            {
                Original<IGVec4f.Set>()(igVec4f, (float)values[0], (float)values[1], (float)values[2], 1f);
            }
        }

        private static void Suppress3DRendering(nuint igOglVisualContext, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5)
        {
            RenderPresetDefinition currentRenderPreset = GetCurrentRenderPreset();

            bool shouldDraw;
            uint shaderHandle = (uint)Original<IGOglVisualContext.GetCurrentProgramHandle>()(igOglVisualContext);
            if (_knownShaders.TryGetValue(shaderHandle, out Shader? knownShader))
            {
                if (currentRenderPreset.CustomShaders?.ContainsKey(knownShader.Name) == true)
                {
                    shouldDraw = true;
                }
                else
                {
                    shouldDraw = currentRenderPreset.DefaultShaderHandling switch
                    {
                        DefaultShaderHandling.AllowAll => true,
                        DefaultShaderHandling.AllowSpecified => currentRenderPreset.DefaultShaders?.Contains(knownShader.Name) is true,
                        DefaultShaderHandling.BlockSpecified => currentRenderPreset.DefaultShaders?.Contains(knownShader.Name) is false or null,
                        DefaultShaderHandling.BlockAll => false,
                        _ => throw new NotImplementedException()
                    };
                }
            }
            else
            {
                // Some shaders don't pass through igProgramAttr::bind(), so we don't know their names. In these cases
                // we should only draw when all shaders are allowed, or when only specific (named) shaders are blocked.
                shouldDraw = currentRenderPreset.DefaultShaderHandling
                    is DefaultShaderHandling.AllowAll
                    or DefaultShaderHandling.BlockSpecified;
            }

            if (!shouldDraw)
            {
                SuppressOriginal<IGOglVisualContext.GenericDraw>();
            }
        }

        private static void SuppressImageOverlay(nuint qGraphicsView, nuint qPainter, nuint targetQRectF, nuint sourceQRect, int aspectRatioMode)
        {
            if (!GetCurrentRenderPreset().ShowImageOverlays)
            {
                SuppressOriginal<QGraphicsView.Render>();
            }
        }

        private static RenderPresetDefinition? UpdateRenderPreset(string name)
        {
            lock (_lock)
            {
                if (_renderPresetsDirectory == null || string.IsNullOrEmpty(name))
                {
                    return null;
                }

                string directory = Path.Combine(_renderPresetsDirectory, name);
                string settingsPath = Path.Combine(directory, "Settings.json");

                RenderPresetDefinition? previousRenderPreset = _renderPresets.GetValueOrDefault(name);
                RenderPresetDefinition? renderPreset = null;
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        string json = File.ReadAllText(settingsPath);
                        renderPreset = JsonSerializer.Deserialize<RenderPresetDefinition>(json, _jsonSerializerOptions);
                    }
                    catch (Exception exception)
                    {
                        Log.Error($"Unable to parse {settingsPath}");
                        Log.Error(exception);
                    }
                }

                if (renderPreset != null)
                {
                    Dictionary<string, string> customShaders = renderPreset.CustomShaders?.ToDictionary() ?? [];
                    foreach (string shaderPath in Directory.EnumerateFiles(directory, "*.glsl"))
                    {
                        string shaderName = Path.GetFileNameWithoutExtension(shaderPath);
                        string shader = File.ReadAllText(shaderPath);
                        customShaders[shaderName] = shader;
                    }

                    renderPreset = renderPreset with
                    {
                        Name = previousRenderPreset?.Name ?? name,
                        CustomShaders = customShaders.ToImmutableDictionary()
                    };

                    _renderPresets[name] = renderPreset;
                }
                else
                {
                    _renderPresets.Remove(name);
                }

                if (renderPreset != null && previousRenderPreset != null
                    && !RenderPresetDefinition.AreEqual(renderPreset, previousRenderPreset))
                {
                    Log.Information($"Render preset {renderPreset.Name} updated");
                    RenderPresetUpdated?.Invoke(renderPreset);
                }
                else if (renderPreset != null && previousRenderPreset == null)
                {
                    Log.Information($"Render preset {renderPreset.Name} added");
                    RenderPresetAdded?.Invoke(renderPreset);
                }
                else if (renderPreset == null && previousRenderPreset != null)
                {
                    Log.Information($"Render preset {previousRenderPreset.Name} removed");
                    RenderPresetRemoved?.Invoke(previousRenderPreset);
                }

                return renderPreset;
            }
        }

        internal static RenderPresetDefinition? GetRenderPreset(string name, bool forceUpdate = false)
        {
            lock (_lock)
            {
                if (forceUpdate)
                {
                    UpdateRenderPreset(name);
                }

                return _renderPresets.GetValueOrDefault(name);
            }
        }

        internal static RenderPresetDefinition GetCurrentRenderPreset() => GetCurrentRenderPreset(out _);

        internal static RenderPresetDefinition GetCurrentRenderPreset(out RenderPresetContext context)
        {
            lock (_lock)
            {
                if (_activeRenderPresets.Count > 0)
                {
                    var current = _activeRenderPresets.First();
                    context = current.Key;
                    return current.Value;
                }
                else
                {
                    context = RenderPresetContext.Original;
                    return RenderPresetDefinition.DefaultFallback;
                }
            }
        }

        internal static void ActivateRenderPreset(RenderPresetDefinition renderPreset, RenderPresetContext context)
        {
            lock (_lock)
            {
                _activeRenderPresets[context] = renderPreset;
            }

            Log.Information($"Render preset {renderPreset.Name} activated ({context.ToString().ToLower()})");
            RenderPresetActivated?.Invoke(renderPreset, context);
            ApplyCurrentRenderPreset();
        }

        internal static void DeactivateRenderPreset(RenderPresetContext context)
        {
            RenderPresetDefinition? renderPreset = null;
            lock (_lock)
            {
                _activeRenderPresets.Remove(context, out renderPreset);
            }

            if (renderPreset != null)
            {
                RenderPresetDeactivated?.Invoke(renderPreset, context);
            }

            ApplyCurrentRenderPreset();
        }

        private static void ApplyCurrentRenderPreset()
        {
            RenderPresetDefinition renderPreset;
            RenderPresetContext context;

            lock (_lock)
            {
                renderPreset = GetCurrentRenderPreset(out context);
                CultureInfo culture = CultureInfo.InvariantCulture;
                string format = Shader.Defines.Format;

                if (Shader.Defines.ContextSpecific.TryGetValue(context, out string? contextDefine))
                {
                    contextDefine = string.Format(culture, format, contextDefine);
                }

                foreach (Shader shader in _knownShaders.Values)
                {
                    if (renderPreset.CustomShaders?.GetValueOrDefault(shader.Name) is string customShaderSource)
                    {
                        shader.DesiredVertexSource = new StringBuilder()
                            .AppendFormat(culture, format, Shader.Defines.VertexShader)
                            .Append(contextDefine)
                            .Append(customShaderSource)
                            .ToString();

                        shader.DesiredFragmentSource = new StringBuilder()
                            .AppendFormat(culture, format, Shader.Defines.PixelShader)
                            .Append(contextDefine)
                            .Append(customShaderSource)
                            .ToString();
                    }
                    else
                    {
                        shader.DesiredVertexSource = shader.OriginalVertexSource;
                        shader.DesiredFragmentSource = shader.OriginalFragmentSource;
                    }
                }
            }

            RenderPresetApplied?.Invoke(renderPreset, context);
        }
    }
}
