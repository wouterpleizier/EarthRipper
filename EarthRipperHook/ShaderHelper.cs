using System.Diagnostics;
using System.Runtime.InteropServices;
using static EarthRipperHook.Native.IGAttrs;
using static EarthRipperHook.Native.IGGfx;

namespace EarthRipperHook
{
    internal static class ShaderHelper
    {
        private record ShaderSource(string FragmentSource, string VertexSource);
        private class ShaderInfo(nuint igProgramAttr, uint handle, string name, ShaderSource originalSource)
        {
            internal nuint IGProgramAttr { get; } = igProgramAttr;
            internal uint Handle { get; } = handle;
            internal string Name { get; } = name;
            internal ShaderSource Original { get; } = originalSource;
            internal ShaderSource? Override { get; private set; } = null;
            internal bool FragmentShaderNeedsRecompile { get; set; }
            internal bool VertexShaderNeedsRecompile { get; set; }

            private ShaderOverrideToken? _overrideToken = null;

            internal ShaderOverrideToken? SetOverride(ShaderSource overrideSource)
            {
                if (Override == null && _overrideToken == null)
                {
                    _overrideToken = new ShaderOverrideToken(token => ClearOverride((ShaderOverrideToken)token));
                    Override = overrideSource;

                    FragmentShaderNeedsRecompile = true;
                    VertexShaderNeedsRecompile = true;

                    return _overrideToken;
                }
                else
                {
                    Log.Warning($"Attempting to override shader {Name} while it is already overridden");
                    return null;
                }
            }

            internal void ClearOverride(ShaderOverrideToken token)
            {
                if (_overrideToken == null)
                {
                    Log.Warning($"Attempting to restore non-overridden shader {Name}");
                }
                else if (_overrideToken != token)
                {
                    Log.Warning($"Attempting to restore shader {Name} using invalid token");
                }
                else
                {
                    _overrideToken = null;
                    Override = null;

                    FragmentShaderNeedsRecompile = true;
                    VertexShaderNeedsRecompile = true;
                }
            }
        }

        private static readonly object _lock = new object();
        private static readonly Dictionary<uint, ShaderInfo> _shadersByHandle = [];
        private static readonly Dictionary<string, ShaderInfo> _shadersByName = [];

        internal static void Initialize()
        {
            // Initializing these during a Bind call results in stack corruption, so do it here.
            EnsureInitialized<IGProgramAttr.SetFragmentSource>();
            EnsureInitialized<IGProgramAttr.SetVertexSource>();

            Hook<IGProgramAttr.Bind>(HandleBind);

            Hook<IGOglVisualContext.CompiledFragmentShader>(HandleCompiledFragmentShader, exclusive: true);
            Hook<IGOglVisualContext.CompiledVertexShader>(HandleCompiledVertexShader, exclusive: true);
        }

        internal static DisposableToken? OverrideShader(string name, string fragmentSource, string vertexSource)
        {
            lock (_lock)
            {
                if (!_shadersByName.TryGetValue(name, out ShaderInfo? shaderInfo))
                {
                    Log.Warning($"Attempting to override unknown shader {name}");
                    return null;
                }

                return shaderInfo.SetOverride(new ShaderSource(fragmentSource, vertexSource));
            }
        }

        internal static DisposableToken? OverrideShader(string name, string vertexAndFragmentSource)
        {
            lock (_lock)
            {
                return OverrideShader(name,
                    "#define GE_PIXEL_SHADER\n" + vertexAndFragmentSource,
                    "#define GE_VERTEX_SHADER\n" + vertexAndFragmentSource);
            }
        }

        private static bool HandleBind(nuint igProgramAttr, nuint igVisualContext)
        {
            lock (_lock)
            {
                uint handle = (uint)Original<IGProgramAttr.GetProgramHandle>()(igProgramAttr);
                string? name = Original<IGProgramAttr.GetName>()(igProgramAttr).AsAnsiString();

                if (_shadersByHandle.TryGetValue(handle, out ShaderInfo? shaderInfo))
                {
                    if (shaderInfo.Handle != handle || shaderInfo.Name != name)
                    {
                        Debugger.Break();
                    }
                }
                else
                {
                    Log.Information("Found shader " + name);

                    string? fragmentSource = Original<IGProgramAttr.GetFragmentSource>()(igProgramAttr).AsAnsiString();
                    string? vertexSource = Original<IGProgramAttr.GetVertexSource>()(igProgramAttr).AsAnsiString();

                    shaderInfo = new ShaderInfo(igProgramAttr, handle, name, new ShaderSource(fragmentSource, vertexSource));
                    _shadersByHandle[handle] = shaderInfo;
                    _shadersByName[name] = shaderInfo;
                }

                return default;
            }
        }

        private static bool HandleCompiledFragmentShader(nuint igOglVisualContext, uint programHandle)
        {
            lock (_lock)
            {
                if (_shadersByHandle.TryGetValue(programHandle, out ShaderInfo? shaderInfo)
                    && shaderInfo.FragmentShaderNeedsRecompile)
                {
                    nint ptr = Marshal.StringToHGlobalAnsi(shaderInfo.Override?.FragmentSource ?? shaderInfo.Original.FragmentSource);
                    Original<IGProgramAttr.SetFragmentSource>()(shaderInfo.IGProgramAttr, ptr);

                    shaderInfo.FragmentShaderNeedsRecompile = false;
                    SuppressOriginal<IGOglVisualContext.CompiledFragmentShader>(false);
                    return false;
                }
                else
                {
                    return default;
                }
            }
        }

        private static bool HandleCompiledVertexShader(nuint igOglVisualContext, uint programHandle)
        {
            lock (_lock)
            {
                if (_shadersByHandle.TryGetValue(programHandle, out ShaderInfo? shaderInfo)
                    && shaderInfo.VertexShaderNeedsRecompile)
                {
                    nint ptr = Marshal.StringToHGlobalAnsi(shaderInfo.Override?.VertexSource ?? shaderInfo.Original.VertexSource);
                    Original<IGProgramAttr.SetVertexSource>()(shaderInfo.IGProgramAttr, ptr);

                    shaderInfo.VertexShaderNeedsRecompile = false;
                    SuppressOriginal<IGOglVisualContext.CompiledVertexShader>(false);
                    return false;
                }
                else
                {
                    return default;
                }
            }
        }
    }
}
