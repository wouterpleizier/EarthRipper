namespace EarthRipperHook.Capture
{
    public record CaptureTask
    (
        string Path,
        OutputFormat Format,
        int? Quality,

        double? ScaleFactor,
        int? MaxWidth,
        int? MaxHeight,

        bool AllowImageOverlays,
        DefaultShaderHandling DefaultShaderHandling,
        HashSet<string>? DefaultShaders,
        Dictionary<string, string>? CustomShaders
    )
    {
        internal const string DirectoryIdentifier = "{Directory}";
        internal const string FileNameIdentifier = "{FileName}";
    }

    public enum OutputFormat
    {
        UserSpecified,
        JPG,
        PNG,
        PNG_Gray16,
    }

    public enum DefaultShaderHandling
    {
        AllowAll,
        BlockAll,
        AllowSpecified,
        BlockSpecified,
    }
}
