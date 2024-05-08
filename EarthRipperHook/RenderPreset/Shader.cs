using Reloaded.Hooks.Definitions.Helpers;
using System.Runtime.InteropServices;
using static EarthRipperHook.Native.IGAttrs;
using static EarthRipperHook.Native.IGGfx;

namespace EarthRipperHook.RenderPreset
{
    internal class Shader
    {
        internal class Defines
        {
            internal const string Format = "#define {0}\n";
            internal const string VertexShader = "GE_VERTEX_SHADER";
            internal const string PixelShader = "GE_PIXEL_SHADER";

            internal static Dictionary<RenderPresetContext, string> ContextSpecific = new()
            {
                { RenderPresetContext.Capture, "EARTHRIPPER_CAPTURE" }
            };
        }

        internal nuint IGProgramAttr { get; }
        internal uint Handle { get; }
        internal string Name { get; }

        internal string OriginalVertexSource { get; }
        internal string DesiredVertexSource { get; set; }
        private string _lastCompiledVertexSource;

        internal string OriginalFragmentSource { get; }
        internal string DesiredFragmentSource { get; set; }
        private string _lastCompiledFragmentSource;

        private string? _lastVertexSourceLog;

        internal Shader(nuint igProgramAttr, uint handle)
        {
            IGProgramAttr = igProgramAttr;
            Handle = handle;

            Name = Original<IGProgramAttr.GetName>()(igProgramAttr).AsAnsiString();

            string vertexSource = Original<IGProgramAttr.GetVertexSource>()(IGProgramAttr).AsAnsiString();
            _lastCompiledVertexSource = vertexSource;
            OriginalVertexSource = vertexSource;
            DesiredVertexSource = vertexSource;

            string fragmentSource = Original<IGProgramAttr.GetFragmentSource>()(IGProgramAttr).AsAnsiString();
            _lastCompiledFragmentSource = fragmentSource;
            OriginalFragmentSource = fragmentSource;
            DesiredFragmentSource = fragmentSource;

            Hook<IGOglVisualContext.CompiledVertexShader>(UpdateVertexSource);
            Hook<IGOglVisualContext.CompiledFragmentShader>(UpdateFragmentSource);
            Hook<IGOglVisualContext.CompileVertexShader>(CompileVertexSource);
            Hook<IGOglVisualContext.CompileFragmentShader>(CompileFragmentSource);
        }

        private bool UpdateVertexSource(nuint igOglVisualContext, uint programHandle)
        {
            if (programHandle == Handle
                && _lastCompiledVertexSource != DesiredVertexSource)
            {
                nint ptr = Marshal.StringToHGlobalAnsi(DesiredVertexSource);
                Original<IGProgramAttr.SetVertexSource>()(IGProgramAttr, ptr);
                Marshal.FreeHGlobal(ptr);

                SuppressOriginal<IGOglVisualContext.CompiledVertexShader>(false);
                return false;
            }

            return default;
        }

        private bool UpdateFragmentSource(nuint igOglVisualContext, uint programHandle)
        {
            if (programHandle == Handle
                && _lastCompiledFragmentSource != DesiredFragmentSource)
            {
                nint ptr = Marshal.StringToHGlobalAnsi(DesiredFragmentSource);
                Original<IGProgramAttr.SetFragmentSource>()(IGProgramAttr, ptr);
                Marshal.FreeHGlobal(ptr);

                SuppressOriginal<IGOglVisualContext.CompiledFragmentShader>(false);
                return false;
            }

            return default;
        }

        private bool CompileVertexSource(nuint igOglVisualContext, uint programHandle, nuint vertexSource)
        {
            if (programHandle == Handle)
            {
                InvokeAfterCompletion<IGOglVisualContext.CompileVertexShader>(_ =>
                {
                    // CompileVertexShader appears to return true even when compilation has failed, so we'll rely on
                    // InfoLog instead. This returns a pointer to the compilation log when errors have occurred, and
                    // null otherwise. (Sadly, this also means that warnings aren't logged when no errors are present)

                    nuint logPtr = Original<IGProgramAttr.InfoLog>()(IGProgramAttr);

                    if (logPtr != nuint.Zero
                        && Marshal.PtrToStringAnsi(logPtr.ToSigned()) is string log)
                    {
                        Log.Warning($"\nVertex shader of {Name} failed to compile:");
                        foreach (string line in log.Split("\n"))
                        {
                            if (line.Contains("error", StringComparison.OrdinalIgnoreCase))
                            {
                                Log.Error(line);
                            }
                            else
                            {
                                Log.Warning(line);
                            }
                        }

                        _lastVertexSourceLog = log;
                        DesiredVertexSource = OriginalVertexSource;
                    }
                    else
                    {
                        _lastVertexSourceLog = null;
                        _lastCompiledVertexSource = DesiredVertexSource;
                    }
                });
            }

            return default;
        }

        private bool CompileFragmentSource(nuint igOglVisualContext, uint programHandle, nuint vertexSource)
        {
            if (programHandle == Handle)
            {
                InvokeAfterCompletion<IGOglVisualContext.CompileFragmentShader>(_ =>
                {
                    nuint logPtr = Original<IGProgramAttr.InfoLog>()(IGProgramAttr);

                    // Same as the vertex shader stage, but with an extra wrinkle: when vertex shader compilation fails
                    // but fragment shader compilation succeeds, InfoLog returns the same error log. We don't need to
                    // log it again in that case, but we should probably still revert to the original source.
                    if (logPtr != nuint.Zero && Marshal.PtrToStringAnsi(logPtr.ToSigned()) is string log)
                    {
                        if (log != _lastVertexSourceLog)
                        {
                            Log.Warning($"\nFragment shader of {Name} failed to compile:");
                            foreach (string line in log.Split("\n"))
                            {
                                if (line.Contains("error", StringComparison.OrdinalIgnoreCase))
                                {
                                    Log.Error(line);
                                }
                                else
                                {
                                    Log.Warning(line);
                                }
                            }
                        }

                        DesiredFragmentSource = OriginalFragmentSource;
                    }
                    else
                    {
                        _lastCompiledFragmentSource = DesiredFragmentSource;
                    }
                });
            }

            return default;
        }
    }
}
