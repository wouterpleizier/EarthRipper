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

        internal static class QAction
        {
            [FunctionName("?activate@QAction@@QAEXW4ActionEvent@1@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Activate(nuint qAction, int actionEvent);

            [FunctionName("?isChecked@QAction@@QBE_NXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool IsChecked(nuint qAction);

            [FunctionName("?setCheckable@QAction@@QAEX_N@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetCheckable(nuint qAction, bool checkable);
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

        internal static class QMainWindow
        {
            [FunctionName("?event@QMainWindow@@MAE_NPAVQEvent@@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Event(nuint qMainWindow, nuint qEvent);

            [FunctionName("?menuBar@QMainWindow@@QBEPAVQMenuBar@@XZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint MenuBar(nuint qMainWindow);
        }

        internal static class QMenu
        {
            [FunctionName("?addAction@QMenu@@QAEPAVQAction@@ABVQString@@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint AddAction(nuint qMenu, nuint qString);
        }

        internal static class QMenuBar
        {
            [FunctionName("?addMenu@QMenuBar@@QAEPAVQMenu@@ABVQString@@@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint AddMenu(nuint qMenuBar, nuint qString);
        }
    }
}
