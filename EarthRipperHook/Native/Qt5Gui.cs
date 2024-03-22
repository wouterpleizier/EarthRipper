using System.Runtime.InteropServices;

namespace EarthRipperHook.Native
{
    [FunctionLibrary("Qt5Gui.dll")]
    internal static class Qt5Gui
    {
        internal static class QImage
        {
            [FunctionName("?bytesPerLine@QImage@@QBEHXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int BytesPerLine(nuint qImage);

            [FunctionName("?constBits@QImage@@QBEPBEXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint ConstBits(nuint qImage);

            [FunctionName("?height@QImage@@QBEHXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int Height(nuint qImage);

            [FunctionName("?save@QImage@@QBE_NABVQString@@PBDH@Z")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool Save(nuint qImage, nuint fileName, [MarshalAs(UnmanagedType.LPStr)] string format, int quality);

            [FunctionName("?width@QImage@@QBEHXZ")]
            [X86Function(X86CallingConventions.MicrosoftThiscall), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int Width(nuint qImage);
        }
    }
}
