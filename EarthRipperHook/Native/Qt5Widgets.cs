namespace EarthRipperHook.Native
{
    [FunctionLibrary("Qt5Widgets.dll")]
    internal static class Qt5Widgets
    {
        internal static class QAbstractButton
        {
            [FunctionName("?clicked@QAbstractButton@@QAEX_N@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Clicked(nuint qAbstractButton, bool isChecked);
        }

        internal static class QFileDialog
        {
            [FunctionName("?getSaveFileName@QFileDialog@@SA?AVQString@@PAVQWidget@@ABV2@11PAV2@V?$QFlags@W4Option@QFileDialog@@@@@Z")]
            [X86Function(X86CallingConventions.Cdecl), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetSaveFileName(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter);
        }

        internal static class QGraphicsView
        {
            [FunctionName("?render@QGraphicsView@@QAEXPAVQPainter@@ABVQRectF@@ABVQRect@@W4AspectRatioMode@Qt@@@Z")]
            [X86Function(X86CallingConventions.Cdecl), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Render(nuint qGraphicsView, nuint qPainter, nuint targetQRectF, nuint sourceQRect, int aspectRatioMode);
        }
    }
}
