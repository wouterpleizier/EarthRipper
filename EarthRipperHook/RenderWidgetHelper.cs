using EarthRipperHook.Qt5;
using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook
{
    internal static class RenderWidgetHelper
    {
        private static IntPtr _renderWidget;
        private static byte[] _mouseMoveEventTemplate;
        private static QMouseEventPosition _lastMousePositionInRenderWidget;

        static RenderWidgetHelper()
        {
            Qt5Hooks.WidgetEvent += HandleWidgetEvent;
        }

        private static void HandleWidgetEvent(IntPtr qWidget, IntPtr qEvent, bool isRenderWidget, ref bool? handled)
        {
            if (isRenderWidget)
            {
                if (_renderWidget == IntPtr.Zero)
                {
                    _renderWidget = qWidget;
                }

                QEvent_Type eventType = Qt5Methods.QEvent_Type(qEvent);
                if (eventType == QEvent_Type.MouseMove)
                {
                    if (_mouseMoveEventTemplate == null)
                    {
                        // We'll copy the event data so we can reuse and modify it later when simulating additional
                        // mouse move events. We don't know exactly how big a QMouseEvent is, so just to be safe we'll
                        // take more than what's probably necessary. Native code will only use the parts it knows about.
                        _mouseMoveEventTemplate = new byte[256];
                        Marshal.Copy(qEvent, _mouseMoveEventTemplate, 0, _mouseMoveEventTemplate.Length);
                    }

                    _lastMousePositionInRenderWidget = QMouseEventPosition.FromQMouseEvent(qEvent);
                }
            }
        }

        public static void ForceRedraw()
        {
            // Not the nicest way to trigger a redraw, but it works.

            QMouseEventPosition offsetMousePositionInRenderWidget = _lastMousePositionInRenderWidget;
            offsetMousePositionInRenderWidget.LocalX++;
            offsetMousePositionInRenderWidget.WindowX++;
            offsetMousePositionInRenderWidget.ScreenX++;

            SimulateMouseMoveEvent(offsetMousePositionInRenderWidget);
            SimulateMouseMoveEvent(_lastMousePositionInRenderWidget);
        }

        public static void RepeatLastMouseMoveEvent()
        {
            SimulateMouseMoveEvent(_lastMousePositionInRenderWidget);
        }

        public static void SimulateMouseMoveEvent(QMouseEventPosition mousePosition)
        {
            if (_renderWidget != IntPtr.Zero && _mouseMoveEventTemplate != null)
            {
                IntPtr qMouseEvent = Marshal.AllocHGlobal(_mouseMoveEventTemplate.Length);
                Marshal.Copy(_mouseMoveEventTemplate, 0, qMouseEvent, _mouseMoveEventTemplate.Length);

                mousePosition.ToQMouseEvent(qMouseEvent);

                Qt5Methods.QCoreApplication_PostEvent(_renderWidget, qMouseEvent);

                // Qt should free the allocated memory after processing the event.
            }
        }
    }
}
