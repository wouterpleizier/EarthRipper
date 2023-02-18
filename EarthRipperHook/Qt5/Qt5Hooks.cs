using EasyHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EarthRipperHook.Qt5
{
    internal class Qt5Hooks : HookContainer
    {
        internal delegate void WidgetEventHandler(IntPtr qWidget, IntPtr qEvent, bool isRenderWidget, ref bool? handled);
        internal static event WidgetEventHandler WidgetEvent;

        internal delegate void RenderWidgetKeyPressHandler(Key key, ref bool? handled);
        internal static event RenderWidgetKeyPressHandler RenderWidgetKeyPress;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool QWidget_EventHookDelegate(IntPtr qWidget, IntPtr qEvent);
        private readonly LocalHook _qWidgetEventHook;

        private IntPtr _renderWidgetWindowHandle;
        private IntPtr _renderWidget;

        public Qt5Hooks()
        {
            _qWidgetEventHook = CreateHook("Qt5Widgets.dll",
                "?event@QWidget@@MAE_NPAVQEvent@@@Z",
                new QWidget_EventHookDelegate(QWidget_EventHook));

            _renderWidgetWindowHandle = WindowHelper.FindChildWindow(Process.GetCurrentProcess().MainWindowHandle, "RenderWidgetWindow");
        }

        public override void Dispose()
        {
            _qWidgetEventHook.Dispose();
        }

        private bool QWidget_EventHook(IntPtr qWidget, IntPtr qEvent)
        {
            if (_renderWidget == IntPtr.Zero)
            {
                IntPtr widgetHandle = Qt5Methods.QWidget_WinId(qWidget);
                if (widgetHandle == _renderWidgetWindowHandle)
                {
                    _renderWidget = qWidget;
                }
            }

            bool isRenderWidget = _renderWidget != IntPtr.Zero && qWidget == _renderWidget;
            QEvent_Type eventType = Qt5Methods.QEvent_Type(qEvent);
            if (isRenderWidget && eventType == QEvent_Type.KeyPress)
            {
                Key key = Qt5Methods.QKeyEvent_Key(qEvent);

                bool? keyPressHandled = null;
                RenderWidgetKeyPress?.Invoke(key, ref keyPressHandled);

                if (keyPressHandled == true)
                {
                    return true;
                }
            }

            bool? eventHandled = null;
            WidgetEvent?.Invoke(qWidget, qEvent, isRenderWidget, ref eventHandled);

            if (eventHandled.HasValue)
            {
                return eventHandled.Value;
            }
            else
            {
                return Qt5Methods.QWidget_Event(qWidget, qEvent);
            }
        }
    }
}
