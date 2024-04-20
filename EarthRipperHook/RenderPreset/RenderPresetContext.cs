namespace EarthRipperHook.RenderPreset
{
    internal enum RenderPresetContext
    {
        // Contexts must be defined in order of importance. Different render presets (or even the same render preset)
        // may be active for multiple contexts simultaneously, but only the context with the smallest value is used.

        Capture,
        Preview,
        Original
    }
}
