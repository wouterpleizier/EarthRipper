namespace EarthRipperHook.Native
{
    [FunctionLibrary("IGMath.dll")]
    internal static class IGMath
    {
        internal static class IGVec4f
        {
            [FunctionName("?set@igVec4f@Math@Gap@@QAEXMMMM@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Set(nuint igVec4f, float x, float y, float z, float w);
        }
    }
}