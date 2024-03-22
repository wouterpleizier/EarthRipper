namespace EarthRipperHook.Native
{
    [FunctionLibrary("IGAttrs.dll")]
    internal static class IGAttrs
    {
        internal static class IGClearAttr
        {
            [FunctionName("?setColor@igClearAttr@Attrs@Gap@@QAEXABVigVec4f@Math@3@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetColor(nuint igClearAttr, nuint igVec4f);
        }

        internal static class IGProgramAttr
        {
            [FunctionName("?bind@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool Bind(nuint igProgramAttr, nuint igVisualContext);

            [FunctionName("?compileFragmentStage@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompileFragmentStage(nuint igProgramAttr, nuint igVisualContext);

            [FunctionName("?compileVertexStage@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompileVertexStage(nuint igProgramAttr, nuint igVisualContext);

            [FunctionName("?getFragmentSource@igProgramAttr@Attrs@Gap@@QBEPBDXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetFragmentSource(nuint igProgramAttr);

            [FunctionName("?getName@igProgramAttr@Attrs@Gap@@QBEPBDXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetName(nuint igProgramAttr);

            [FunctionName("?getProgramHandle@igProgramAttr@Attrs@Gap@@QBEHXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int GetProgramHandle(nuint igProgramAttr);

            [FunctionName("?getVertexSource@igProgramAttr@Attrs@Gap@@QBEPBDXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetVertexSource(nuint igProgramAttr);

            [FunctionName("?setFragmentSource@igProgramAttr@Attrs@Gap@@QAEXPBD@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetFragmentSource(nuint igProgramAttr, nint source);

            [FunctionName("?setVertexSource@igProgramAttr@Attrs@Gap@@QAEXPBD@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetVertexSource(nuint igProgramAttr, nint source);
        }
    }
}
