﻿using System.Globalization;
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
        private static readonly object _lock = new object();

        private static readonly StringComparer _renderPresetNameComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly Dictionary<string, RenderPresetDefinition> _renderPresets = new(_renderPresetNameComparer);
        private static readonly SortedDictionary<RenderPresetContext, RenderPresetDefinition> _activeRenderPresets = [];

        private static readonly Dictionary<uint, Shader> _knownShaders = [];

        private static JsonSerializerOptions? _jsonSerializerOptions;
        private static string? _renderPresetsDirectory;

        internal static void Initialize()
        {
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

            // TODO: Move this to RenderPresetPreviewer or something
            foreach (KeyValuePair<string, RenderPresetDefinition> pair in _renderPresets)
            {
                MenuManager.AddAction(pair.Key, (isChecked) =>
                {
                    RenderPresetDefinition? renderPreset = UpdateRenderPreset(pair.Key);
                    if (renderPreset != null)
                    {
                        ActivateRenderPreset(renderPreset, RenderPresetContext.Preview);
                    }
                    else
                    {
                        DeactivateRenderPreset(RenderPresetContext.Preview);
                    }
                });
            }
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
            // Some shaders don't pass through igProgramAttr::bind(), so we don't know their names. In these cases we
            // should only draw when all shaders are allowed, or when only specific (named) shaders are blocked.
            bool shouldDrawUnknownShaders = GetCurrentRenderPreset().DefaultShaderHandling
                is DefaultShaderHandling.AllowAll
                or DefaultShaderHandling.BlockSpecified;

            uint shaderHandle = (uint)Original<IGOglVisualContext.GetCurrentProgramHandle>()(igOglVisualContext);
            if (!_knownShaders.ContainsKey(shaderHandle) && !shouldDrawUnknownShaders)
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
            if (_renderPresetsDirectory == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            string directory = Path.Combine(_renderPresetsDirectory, name);
            string settingsPath = Path.Combine(directory, "Settings.json");
            
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
                foreach (string shaderPath in Directory.EnumerateFiles(directory, "*.glsl"))
                {
                    if (renderPreset.CustomShaders == null)
                    {
                        renderPreset = renderPreset with { CustomShaders = [] };
                    }

                    string shaderName = Path.GetFileNameWithoutExtension(shaderPath);
                    string shader = File.ReadAllText(shaderPath);
                    renderPreset.CustomShaders[shaderName] = shader;
                }

                _renderPresets[name] = renderPreset;
            }
            else
            {
                _renderPresets.Remove(name);
            }

            return renderPreset;
        }

        internal static RenderPresetDefinition? GetRenderPreset(string name)
        {
            lock (_lock)
            {
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
                ApplyCurrentRenderPreset();
            }
        }

        internal static void DeactivateRenderPreset(RenderPresetContext context)
        {
            lock (_lock)
            {
                _activeRenderPresets.Remove(context);
                ApplyCurrentRenderPreset();
            }
        }

        private static void ApplyCurrentRenderPreset()
        {
            lock (_lock)
            {
                RenderPresetDefinition definition = GetCurrentRenderPreset(out RenderPresetContext context);
                CultureInfo culture = CultureInfo.InvariantCulture;
                string format = Shader.Defines.Format;

                if (Shader.Defines.ContextSpecific.TryGetValue(context, out string? contextDefine))
                {
                    contextDefine = string.Format(culture, format, contextDefine);
                }

                foreach (Shader shader in _knownShaders.Values)
                {
                    if (definition?.CustomShaders?.GetValueOrDefault(shader.Name) is string customShaderSource)
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
        }
    }
}