namespace EarthRipperHook.Native
{
    [FunctionLibrary("IGGfx.dll")]
    internal static class IGGfx
    {
        /// <remarks>
        /// Note: this type is not just used in OpenGL mode as its name suggests, but in DirectX mode too.
        /// </remarks>
        internal static class IGOglVisualContext
        {
            [X86FunctionName("?compileFragmentShader@igOglVisualContext@Gfx@Gap@@QAE_NIPBD@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?compileFragmentShader@igOglVisualContext@Gfx@Gap@@QEAA_NIPEBD@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompileFragmentShader(nuint igOglVisualContext, uint programHandle, nuint fragmentSource);

            [X86FunctionName("?compileVertexShader@igOglVisualContext@Gfx@Gap@@QAE_NIPBD@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?compileVertexShader@igOglVisualContext@Gfx@Gap@@QEAA_NIPEBD@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompileVertexShader(nuint igOglVisualContext, uint programHandle, nuint vertexSource);

            [X86FunctionName("?compiledFragmentShader@igOglVisualContext@Gfx@Gap@@QAE_NI@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?compiledFragmentShader@igOglVisualContext@Gfx@Gap@@QEAA_NI@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompiledFragmentShader(nuint igOglVisualContext, uint programHandle);

            [X86FunctionName("?compiledVertexShader@igOglVisualContext@Gfx@Gap@@QAE_NI@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?compiledVertexShader@igOglVisualContext@Gfx@Gap@@QEAA_NI@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompiledVertexShader(nuint igOglVisualContext, uint programHandle);

            [X86FunctionName("?genericDraw@igOglVisualContext@Gfx@Gap@@IAEXHHHHH@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?genericDraw@igOglVisualContext@Gfx@Gap@@IEAAXHHHHH@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void GenericDraw(nuint igOglVisualContext, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5);

            [X86FunctionName("?getCurrentProgramHandle@igOglVisualContext@Gfx@Gap@@QBEHXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?getCurrentProgramHandle@igOglVisualContext@Gfx@Gap@@QEBAHXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int GetCurrentProgramHandle(nuint igOglVisualContext);
        }
    }
}
