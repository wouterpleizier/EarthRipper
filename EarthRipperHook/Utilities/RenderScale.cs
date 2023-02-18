using EarthRipperHook.EarthPro;
using EarthRipperHook.Qt5;
using System;

namespace EarthRipperHook.Utilities
{
    internal class RenderScale : Utility
    {
        private int _renderScale = 1;
        private IntPtr _renderWidget;
        private IntPtr _renderWidgetParentWidget;

        public RenderScale()
        {
            Qt5Hooks.WidgetEvent += HandleWidgetEvent;
            Qt5Hooks.RenderWidgetKeyPress += HandleRenderWidgetKeyPress;
            IGHooks.SetViewport += HandleSetViewport;
        }

        public override void Dispose()
        {
            Qt5Hooks.WidgetEvent -= HandleWidgetEvent;
            Qt5Hooks.RenderWidgetKeyPress -= HandleRenderWidgetKeyPress;
            IGHooks.SetViewport -= HandleSetViewport;

            _renderScale = 1;
            UpdateRenderScale();
        }

        private void HandleWidgetEvent(IntPtr qWidget, IntPtr qEvent, bool isRenderWidget, ref bool? handled)
        {
            if (handled.HasValue && handled.Value == true)
            {
                return;
            }

            if (isRenderWidget && (_renderWidget == IntPtr.Zero || _renderWidgetParentWidget == IntPtr.Zero))
            {
                _renderWidget = qWidget;
                _renderWidgetParentWidget = Qt5Methods.QWidget_ParentWidget(_renderWidget);
            }

            QEvent_Type eventType = Qt5Methods.QEvent_Type(qEvent);
            if (eventType == QEvent_Type.Resize)
            {
                UpdateRenderScale();
            }
            else if (isRenderWidget)
            {
                AdjustMouseEventPosition(qEvent, eventType);
            }
        }

        private void HandleRenderWidgetKeyPress(Key key, ref bool? handled)
        {
            if (key == Key.BracketLeft)
            {
                _renderScale = Math.Max(1, _renderScale - 1);
                UpdateRenderScale();

                handled = true;
            }
            else if (key == Key.BracketRight)
            {
                _renderScale++;
                UpdateRenderScale();

                handled = true;
            }
        }

        private void UpdateRenderScale()
        {
            if (_renderWidget != IntPtr.Zero && _renderWidgetParentWidget != IntPtr.Zero)
            {
                int unmodifiedWidth = Qt5Methods.QWidget_Width(_renderWidgetParentWidget);
                int unmodifiedHeight = Qt5Methods.QWidget_Height(_renderWidgetParentWidget);

                int currentWidth = Qt5Methods.QWidget_Width(_renderWidget);
                int currentHeight = Qt5Methods.QWidget_Height(_renderWidget);

                int desiredWidth = Convert.ToInt32(unmodifiedWidth * _renderScale);
                int desiredHeight = Convert.ToInt32(unmodifiedHeight * _renderScale);

                if (currentWidth != desiredWidth || currentHeight != desiredHeight)
                {
                    Logger.LogMessage($"Setting render scale to {_renderScale} ({desiredWidth} x {desiredHeight})", GetType().Name);
                    Qt5Methods.QWidget_Resize(_renderWidget, desiredWidth, desiredHeight);
                }
            }
        }

        private void AdjustMouseEventPosition(IntPtr qEvent, QEvent_Type eventType)
        {
            if (_renderWidget != IntPtr.Zero && _renderWidgetParentWidget != IntPtr.Zero && _renderScale > 1)
            {
                double scale = Qt5Methods.QWidget_Width(_renderWidget) / Qt5Methods.QWidget_Width(_renderWidgetParentWidget);

                switch (eventType)
                {
                    case QEvent_Type.MouseButtonDblClick:
                    case QEvent_Type.MouseButtonPress:
                    case QEvent_Type.MouseButtonRelease:
                    case QEvent_Type.MouseMove:
                        {
                            QMouseEventPosition mousePosition = QMouseEventPosition.FromQMouseEvent(qEvent);
                            mousePosition.LocalX *= scale;
                            mousePosition.LocalY *= scale;
                            mousePosition.WindowX *= scale;
                            mousePosition.WindowY *= scale;

                            mousePosition.ToQMouseEvent(qEvent);
                            break;
                        }

                    case QEvent_Type.Wheel:
                        {
                            QWheelEventPosition wheelPosition = QWheelEventPosition.FromQWheelEvent(qEvent);
                            wheelPosition.LocalX *= scale;
                            wheelPosition.LocalY *= scale;

                            wheelPosition.ToQWheelEvent(qEvent);
                            break;
                        }

                    default:
                        break;
                }
            }
        }

        private void HandleSetViewport(IntPtr igOglVisualContext, ref int x, ref int y, ref int width, ref int height, ref float minZ, ref float maxZ)
        {
            if (_renderWidget != IntPtr.Zero && _renderWidgetParentWidget != IntPtr.Zero && _renderScale > 1)
            {
                // Depending on Windows' display scaling setting, the render size may not match the widget size.
                double scaleMultiplier = (double)width / Qt5Methods.QWidget_Width(_renderWidget);

                int desiredWidth = Convert.ToInt32(Qt5Methods.QWidget_Width(_renderWidgetParentWidget) * scaleMultiplier);
                int desiredHeight = Convert.ToInt32(Qt5Methods.QWidget_Height(_renderWidgetParentWidget) * scaleMultiplier);

                y = height - desiredHeight;
                width = desiredWidth;
                height = desiredHeight;
            }
        }
    }
}
