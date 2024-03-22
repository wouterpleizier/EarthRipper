namespace EarthRipperHook.Native
{
    [FunctionLibrary("IGGfx.dll")]
    internal static class IGGfx
    {
        internal static class IGOglVisualContext
        {
            [FunctionName("?compiledFragmentShader@igOglVisualContext@Gfx@Gap@@QAE_NI@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompiledFragmentShader(nuint igOglVisualContext, uint programHandle);

            [FunctionName("?compiledVertexShader@igOglVisualContext@Gfx@Gap@@QAE_NI@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool CompiledVertexShader(nuint igOglVisualContext, uint programHandle);

            [FunctionName("?genericDraw@igOglVisualContext@Gfx@Gap@@IAEXHHHHH@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void GenericDraw(nuint igOglVisualContext, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5);

            [FunctionName("?getCurrentProgramHandle@igOglVisualContext@Gfx@Gap@@QBEHXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int GetCurrentProgramHandle(nuint igOglVisualContext);
        }
    }
}
