namespace EarthRipperHook.Hooks
{
    [FunctionLibrary("IGGfx.dll")]
    internal static class IGGfx
    {
        internal static class IgOglVisualContext
        {
            [X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64Function(X64CallingConventions.Microsoft)]
            [FunctionName("?genericDraw@igOglVisualContext@Gfx@Gap@@IAEXHHHHH@Z")]
            internal delegate void GenericDraw(nuint igOglVisualContext, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5);
        }
    }
}
