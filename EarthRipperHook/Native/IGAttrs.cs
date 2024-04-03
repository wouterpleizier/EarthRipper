namespace EarthRipperHook.Native
{
    [FunctionLibrary("IGAttrs.dll")]
    internal static class IGAttrs
    {
        internal static class IGClearAttr
        {
            [X86FunctionName("?setColor@igClearAttr@Attrs@Gap@@QAEXABVigVec4f@Math@3@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?setColor@igClearAttr@Attrs@Gap@@QEAAXAEBVigVec4f@Math@3@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetColor(nuint igClearAttr, nuint igVec4f);
        }

        internal static class IGProgramAttr
        {
            [X86FunctionName("?bind@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?bind@igProgramAttr@Attrs@Gap@@QEAA_NPEAVigVisualContext@Gfx@3@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool Bind(nuint igProgramAttr, nuint igVisualContext);

            [X86FunctionName("?compileFragmentStage@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?compileFragmentStage@igProgramAttr@Attrs@Gap@@QEAA_NPEAVigVisualContext@Gfx@3@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompileFragmentStage(nuint igProgramAttr, nuint igVisualContext);

            [X86FunctionName("?compileVertexStage@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?compileVertexStage@igProgramAttr@Attrs@Gap@@QEAA_NPEAVigVisualContext@Gfx@3@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompileVertexStage(nuint igProgramAttr, nuint igVisualContext);

            [X86FunctionName("?getFragmentSource@igProgramAttr@Attrs@Gap@@QBEPBDXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?getFragmentSource@igProgramAttr@Attrs@Gap@@QEBAPEBDXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetFragmentSource(nuint igProgramAttr);

            [X86FunctionName("?getName@igProgramAttr@Attrs@Gap@@QBEPBDXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?getName@igProgramAttr@Attrs@Gap@@QEBAPEBDXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetName(nuint igProgramAttr);

            [X86FunctionName("?getProgramHandle@igProgramAttr@Attrs@Gap@@QBEHXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?getProgramHandle@igProgramAttr@Attrs@Gap@@QEBAHXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int GetProgramHandle(nuint igProgramAttr);

            [X86FunctionName("?getVertexSource@igProgramAttr@Attrs@Gap@@QBEPBDXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?getVertexSource@igProgramAttr@Attrs@Gap@@QEBAPEBDXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetVertexSource(nuint igProgramAttr);

            [X86FunctionName("?setFragmentSource@igProgramAttr@Attrs@Gap@@QAEXPBD@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?setFragmentSource@igProgramAttr@Attrs@Gap@@QEAAXPEBD@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetFragmentSource(nuint igProgramAttr, nint source);

            [X86FunctionName("?setVertexSource@igProgramAttr@Attrs@Gap@@QAEXPBD@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?setVertexSource@igProgramAttr@Attrs@Gap@@QEAAXPEBD@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetVertexSource(nuint igProgramAttr, nint source);
        }

        internal static class IGProjectionMatrixAttr
        {
            [X86FunctionName("?setMatrix@igProjectionMatrixAttr@Attrs@Gap@@UAEXABVigMatrix44f@Math@3@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?setMatrix@igProjectionMatrixAttr@Attrs@Gap@@UEAAXAEBVigMatrix44f@Math@3@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetMatrix(nuint igProjectionMatrixAttr, nuint igMatrix44f);
        }
    }
}
