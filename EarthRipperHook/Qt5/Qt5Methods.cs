using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook.Qt5
{
    internal static class Qt5Methods
    {
        [DllImport("Qt5Widgets.dll", EntryPoint = "?event@QWidget@@MAE_NPAVQEvent@@@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern bool QWidget_Event(IntPtr @this, IntPtr qEvent);

        [DllImport("Qt5Core.dll", EntryPoint = "?type@QEvent@@QBE?AW4Type@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern QEvent_Type QEvent_Type(IntPtr @this);

        [DllImport("Qt5Core.dll", EntryPoint = "?postEvent@QCoreApplication@@SAXPAVQObject@@PAVQEvent@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool QCoreApplication_PostEvent(IntPtr qObjectReceiver, IntPtr qEvent, int priority = 0);

        [DllImport("Qt5Gui.dll", EntryPoint = "?key@QKeyEvent@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern Key QKeyEvent_Key(IntPtr @this);

        [DllImport("Qt5Widgets.dll", EntryPoint = "?winId@QWidget@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern IntPtr QWidget_WinId(IntPtr @this);

        [DllImport("Qt5Widgets.dll", EntryPoint = "?parentWidget@QWidget@@QBEPAV1@XZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern IntPtr QWidget_ParentWidget(IntPtr @this);

        [DllImport("Qt5Widgets.dll", EntryPoint = "?width@QWidget@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern int QWidget_Width(IntPtr @this);

        [DllImport("Qt5Widgets.dll", EntryPoint = "?height@QWidget@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern int QWidget_Height(IntPtr @this);

        [DllImport("Qt5Widgets.dll", EntryPoint = "?resize@QWidget@@QAEXHH@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern int QWidget_Resize(IntPtr @this, int width, int height);
    }
}
