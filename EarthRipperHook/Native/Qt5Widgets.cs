namespace EarthRipperHook.Native
{
    [FunctionLibrary("Qt5Widgets.dll")]
    internal static class Qt5Widgets
    {
        internal static class QAbstractButton
        {
            [X86FunctionName("?clicked@QAbstractButton@@QAEX_N@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?clicked@QAbstractButton@@QEAAX_N@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Clicked(nuint qAbstractButton, bool isChecked);
        }

        internal static class QAction
        {
            [X86FunctionName("?activate@QAction@@QAEXW4ActionEvent@1@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?activate@QAction@@QEAAXW4ActionEvent@1@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Activate(nuint qAction, int actionEvent);

            [X86FunctionName("?isChecked@QAction@@QBE_NXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?isChecked@QAction@@QEBA_NXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool IsChecked(nuint qAction);

            [X86FunctionName("?setCheckable@QAction@@QAEX_N@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?setCheckable@QAction@@QEAAX_N@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void SetCheckable(nuint qAction, bool checkable);
        }

        internal static class QFileDialog
        {
            [X86FunctionName("?getSaveFileName@QFileDialog@@SA?AVQString@@PAVQWidget@@ABV2@11PAV2@V?$QFlags@W4Option@QFileDialog@@@@@Z"), X86Function(X86CallingConventions.Cdecl)]
            [X64FunctionName("?getSaveFileName@QFileDialog@@SA?AVQString@@PEAVQWidget@@AEBV2@11PEAV2@V?$QFlags@W4Option@QFileDialog@@@@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint GetSaveFileName(nuint result, nuint parentWidget, nuint caption, nuint dir, nuint filter, nuint selectedFilter);
        }

        internal static class QGraphicsView
        {
            [X86FunctionName("?render@QGraphicsView@@QAEXPAVQPainter@@ABVQRectF@@ABVQRect@@W4AspectRatioMode@Qt@@@Z"), X86Function(X86CallingConventions.Cdecl)]
            [X64FunctionName("?render@QGraphicsView@@QEAAXPEAVQPainter@@AEBVQRectF@@AEBVQRect@@W4AspectRatioMode@Qt@@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Render(nuint qGraphicsView, nuint qPainter, nuint targetQRectF, nuint sourceQRect, int aspectRatioMode);
        }

        internal static class QMainWindow
        {
            [X86FunctionName("?event@QMainWindow@@MAE_NPAVQEvent@@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?event@QMainWindow@@MEAA_NPEAVQEvent@@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate void Event(nuint qMainWindow, nuint qEvent);

            [X86FunctionName("?menuBar@QMainWindow@@QBEPAVQMenuBar@@XZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?menuBar@QMainWindow@@QEBAPEAVQMenuBar@@XZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint MenuBar(nuint qMainWindow);
        }

        internal static class QMenu
        {
            [X86FunctionName("?addAction@QMenu@@QAEPAVQAction@@ABVQString@@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?addAction@QMenu@@QEAAPEAVQAction@@AEBVQString@@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint AddAction(nuint qMenu, nuint qString);
        }

        internal static class QMenuBar
        {
            [X86FunctionName("?addMenu@QMenuBar@@QAEPAVQMenu@@ABVQString@@@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?addMenu@QMenuBar@@QEAAPEAVQMenu@@AEBVQString@@@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint AddMenu(nuint qMenuBar, nuint qString);
        }
    }
}
