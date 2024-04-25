using System.Collections.Immutable;

namespace EarthRipperHook.RenderPreset
{
    public record RenderPresetDefinition
    (
        string Name = RenderPresetDefinition.DefaultName,
        bool ShowImageOverlays = true,
        DefaultShaderHandling DefaultShaderHandling = DefaultShaderHandling.AllowAll,
        ImmutableHashSet<string>? DefaultShaders = null,
        ImmutableDictionary<string, string>? CustomShaders = null,
        double[]? ClearColor = null,
        string OutputPath = $"{RenderPresetDefinition.DirectoryIdentifier}/{RenderPresetDefinition.FileNameIdentifier}",
        OutputFormat OutputFormat = OutputFormat.UserSpecified,
        int? OutputQuality = 95,
        double? OutputScaleFactor = 1.0,
        int? OutputMaxWidth = 8192,
        int? OutputMaxHeight = 8192
    )
    {
        internal const string DirectoryIdentifier = "{Directory}";
        internal const string FileNameIdentifier = "{FileName}";

        internal const string DefaultName = "Default";
        internal static RenderPresetDefinition DefaultFallback { get; } = new RenderPresetDefinition();
    }

    public enum DefaultShaderHandling
    {
        AllowAll,
        BlockAll,
        AllowSpecified,
        BlockSpecified,
    }

    public enum OutputFormat
    {
        UserSpecified,
        JPG,
        PNG,
        PNG_Gray16,
    }
}
