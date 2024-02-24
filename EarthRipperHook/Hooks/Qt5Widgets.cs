namespace EarthRipperHook.Hooks
{
    [FunctionLibrary("Qt5Widgets.dll")]
    internal static class Qt5Widgets
    {
        internal static class QFileDialog
        {
            [X86Function(X86CallingConventions.Cdecl)]
            [X64Function(X64CallingConventions.Microsoft)]
            [FunctionName("?getSaveFileName@QFileDialog@@SA?AVQString@@PAVQWidget@@ABV2@11PAV2@V?$QFlags@W4Option@QFileDialog@@@@@Z")]
            internal delegate nuint GetSaveFileName(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter);
        }
    }
}
