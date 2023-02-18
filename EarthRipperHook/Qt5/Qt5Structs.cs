using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook.Qt5
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct QMouseEventPosition
    {
        internal double LocalX;
        internal double LocalY;
        internal double WindowX;
        internal double WindowY;
        internal double ScreenX;
        internal double ScreenY;

        internal static QMouseEventPosition FromQMouseEvent(IntPtr qMouseEvent)
        {
            return Marshal.PtrToStructure<QMouseEventPosition>(qMouseEvent + 24);
        }

        internal void ToQMouseEvent(IntPtr qMouseEvent)
        {
            Marshal.StructureToPtr(this, qMouseEvent + 24, false);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct QWheelEventPosition
    {
        internal double LocalX;
        internal double LocalY;
        internal double GlobalX;
        internal double GlobalY;

        internal static QWheelEventPosition FromQWheelEvent(IntPtr qWheelEvent)
        {
            return Marshal.PtrToStructure<QWheelEventPosition>(qWheelEvent + 24);
        }

        internal void ToQWheelEvent(IntPtr qWheelEvent)
        {
            Marshal.StructureToPtr(this, qWheelEvent + 24, false);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct QSize
    {
        internal int Width;
        internal int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct QRect
    {
        internal int X1;
        internal int Y1;
        internal int X2;
        internal int Y2;
    }
}
