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
        }

        private bool UpdateVertexSource(nuint igOglVisualContext, uint programHandle)
        {
            if (programHandle == Handle
                && _lastCompiledVertexSource != DesiredVertexSource)
            {
                nint ptr = Marshal.StringToHGlobalAnsi(DesiredVertexSource);
                Original<IGProgramAttr.SetVertexSource>()(IGProgramAttr, ptr);
                Marshal.FreeHGlobal(ptr);

                _lastCompiledVertexSource = DesiredVertexSource;

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

                _lastCompiledFragmentSource = DesiredFragmentSource;

                SuppressOriginal<IGOglVisualContext.CompiledFragmentShader>(false);
                return false;
            }

            return default;
        }
    }
}
