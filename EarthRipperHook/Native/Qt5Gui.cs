using System.Runtime.InteropServices;

namespace EarthRipperHook.Native
{
    [FunctionLibrary("Qt5Gui.dll")]
    internal static class Qt5Gui
    {
        internal static class QImage
        {
            [X86FunctionName("?bytesPerLine@QImage@@QBEHXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?bytesPerLine@QImage@@QEBAHXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int BytesPerLine(nuint qImage);

            [X86FunctionName("?constBits@QImage@@QBEPBEXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?constBits@QImage@@QEBAPEBEXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate nuint ConstBits(nuint qImage);

            [X86FunctionName("?height@QImage@@QBEHXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?height@QImage@@QEBAHXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int Height(nuint qImage);

            [X86FunctionName("?save@QImage@@QBE_NABVQString@@PBDH@Z"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?save@QImage@@QEBA_NAEBVQString@@PEBDH@Z"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate bool Save(nuint qImage, nuint fileName, [MarshalAs(UnmanagedType.LPStr)] string format, int quality);

            [X86FunctionName("?width@QImage@@QBEHXZ"), X86Function(X86CallingConventions.MicrosoftThiscall)]
            [X64FunctionName("?width@QImage@@QEBAHXZ"), X64Function(X64CallingConventions.Microsoft)]
            internal delegate int Width(nuint qImage);
        }
    }
}
