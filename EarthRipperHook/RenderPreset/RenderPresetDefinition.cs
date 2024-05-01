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

        internal static bool AreEqual(RenderPresetDefinition first, RenderPresetDefinition second)
        {
            if (!first.Name.Equals(second.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!Equals(first.DefaultShaders, second.DefaultShaders)
                && first.DefaultShaders != null && second.DefaultShaders != null
                && !first.DefaultShaders.SetEquals(second.DefaultShaders))
            {
                return false;
            }

            if (!Equals(first.CustomShaders, second.CustomShaders)
                && first.CustomShaders != null && second.CustomShaders != null
                && (first.CustomShaders.Count != second.CustomShaders.Count
                    || first.CustomShaders.Except(second.CustomShaders).Any()))
            {
                return false;
            }

            if (!Equals(first.ClearColor, second.ClearColor)
                && first.ClearColor != null && second.ClearColor != null
                && !first.ClearColor.SequenceEqual(second.ClearColor))
            {
                return false;
            }

            return first == second with
            {
                Name = first.Name,
                DefaultShaders = first.DefaultShaders,
                CustomShaders = first.CustomShaders,
                ClearColor = first.ClearColor
            };
        }
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
