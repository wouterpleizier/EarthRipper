using EarthRipperHook.EarthPro;
using EarthRipperHook.OpenGL;
using EarthRipperHook.Qt5;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EarthRipperHook.Utilities
{
    internal class Capture : Utility
    {
        private CaptureState _captureState = CaptureState.WaitingForInput;
        private CaptureResult _captureResult;
        private string _outputDirectory;
        private CaptureFlags _captureFlags;

        private object _lock = new object();
        private float _near;
        private float _far;
        int _groundShaderProgramHandle = -1;
        private IntPtr _coordinateDataAddress;

        public Capture(string outputDirectory, CaptureFlags captureFlags)
        {
            _outputDirectory = outputDirectory;
            _captureFlags = captureFlags;

            Qt5Hooks.RenderWidgetKeyPress += HandleRenderWidgetKeyPress;

            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName == "googleearth_pro.dll")
                {
                    IntPtr address = Marshal.ReadIntPtr(module.BaseAddress, 0x01F3AB8C);
                    address = Marshal.ReadIntPtr(address, 0x50);
                    address = Marshal.ReadIntPtr(address, 0x114);
                    address = IntPtr.Add(address, 0x40);

                    _coordinateDataAddress = address;
                }
            }
        }

        public override void Dispose()
        {
            Qt5Hooks.RenderWidgetKeyPress -= HandleRenderWidgetKeyPress;

            
        }

        private void Reset()
        {
            _captureState = CaptureState.WaitingForInput;
            _captureResult = new CaptureResult() { CaptureFlags = _captureFlags };

            OpenGLHooks.WGLSwapBuffers -= WaitForEndOfFrame;
            IGHooks.SetViewport_Exclusive -= PreCaptureSetViewport;
            IGHooks.SetMatrix_Exclusive -= PreCaptureSetMatrix;
            IGHooks.BindShaderProgram -= PreCaptureBindShaderProgram;
            IGHooks.GenericDraw_Exclusive -= PreCaptureGenericDraw;
            OpenGLHooks.WGLSwapBuffers -= CaptureBuffer;
        }

        private void HandleRenderWidgetKeyPress(Key key, ref bool? handled)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.WaitingForInput && key == Key.C)
                {
                    try
                    {
                        Logger.LogMessage("Beginning capture, waiting for end of current frame", GetType().Name);
                        _captureState = CaptureState.WaitingForBufferSwap;
                        _captureResult = new CaptureResult() { CaptureFlags = _captureFlags };
                        OpenGLHooks.WGLSwapBuffers += WaitForEndOfFrame;
                        RenderWidgetHelper.ForceRedraw();
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void WaitForEndOfFrame(IntPtr hdc)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.WaitingForBufferSwap)
                {
                    try
                    {
                        Logger.LogMessage("Preparing to capture next frame", GetType().Name);
                        _captureState = CaptureState.CapturingBuffer;

                        OpenGLHooks.WGLSwapBuffers -= WaitForEndOfFrame;

                        IGHooks.SetViewport_Exclusive += PreCaptureSetViewport;
                        IGHooks.SetMatrix_Exclusive += PreCaptureSetMatrix;
                        IGHooks.BindShaderProgram += PreCaptureBindShaderProgram;
                        IGHooks.GenericDraw_Exclusive += PreCaptureGenericDraw;
                        OpenGLHooks.WGLSwapBuffers += CaptureBuffer;
                        RenderWidgetHelper.ForceRedraw();
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void PreCaptureSetViewport(IntPtr igOglVisualContext, ref int x, ref int y, ref int width, ref int height, ref float minZ, ref float maxZ)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.CapturingBuffer)
                {
                    try
                    {
                        _captureResult.ImageWidthInPixels = width;
                        _captureResult.ImageHeightInPixels = height;
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void PreCaptureSetMatrix(IntPtr igProjectionMatrixAttr, IntPtr matrix)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.CapturingBuffer)
                {
                    try
                    {
                        Matrix4x4 managedMatrix = Marshal.PtrToStructure<Matrix4x4>(matrix);

                        if (MathHelper.ApproximatelyEquals(managedMatrix.M34, -1f))
                        {
                            _near = managedMatrix.M43 / (managedMatrix.M33 - 1);
                            _far = managedMatrix.M43 / (managedMatrix.M33 + 1);
                        }
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void PreCaptureBindShaderProgram(IntPtr igProgramAttr, IntPtr igVisualContext)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.CapturingBuffer && _groundShaderProgramHandle < 0)
                {
                    try
                    {
                        string shaderProgramName = Marshal.PtrToStringAnsi(IGMethods.igProgramAttr_GetName(igProgramAttr));
                        if (shaderProgramName.Contains("ground"))
                        {
                            _groundShaderProgramHandle = IGMethods.igProgramAttr_GetProgramHandle(igProgramAttr);
                        }
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void PreCaptureGenericDraw(IntPtr igOglVisualContext, ref bool suppress)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.CapturingBuffer)
                {
                    try
                    {
                        int currentShaderProgramHandle = IGMethods.igOglVisualContext_GetCurrentProgramHandle(igOglVisualContext);
                        suppress = currentShaderProgramHandle != _groundShaderProgramHandle;
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void CaptureBuffer(IntPtr hdc)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.CapturingBuffer)
                {
                    try
                    {
                        IGHooks.SetViewport_Exclusive -= PreCaptureSetViewport;
                        IGHooks.SetMatrix_Exclusive -= PreCaptureSetMatrix;
                        IGHooks.BindShaderProgram -= PreCaptureBindShaderProgram;
                        IGHooks.GenericDraw_Exclusive -= PreCaptureGenericDraw;
                        OpenGLHooks.WGLSwapBuffers -= CaptureBuffer;

                        if (_captureFlags.HasFlag(CaptureFlags.Height))
                        {
                            Logger.LogMessage("Capturing height", GetType().Name);
                            _captureResult.Height = CaptureHeight();
                        }

                        if (_captureFlags.HasFlag(CaptureFlags.Color))
                        {
                            Logger.LogMessage("Capturing color", GetType().Name);
                            _captureResult.Color = CaptureColor();
                        }

                        Logger.LogMessage("Sampling coordinates and elevation", GetType().Name);
                        _captureState = CaptureState.SamplingCoordinatesAndElevation;
                        Qt5Hooks.WidgetEvent += SampleCoordinatesAndElevation;
                        RenderWidgetHelper.RepeatLastMouseMoveEvent();
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private ushort[] CaptureHeight()
        {
            int pixelCount = _captureResult.ImageWidthInPixels * _captureResult.ImageHeightInPixels;
            IntPtr pixelBuffer = Marshal.AllocHGlobal(pixelCount * Marshal.SizeOf<float>());
            OpenGLMethods.glReadPixels(0, 0, _captureResult.ImageWidthInPixels, _captureResult.ImageHeightInPixels, OpenGL.PixelFormat.GL_DEPTH_COMPONENT, DataType.GL_FLOAT, pixelBuffer);

            float[] managedPixelBuffer = new float[pixelCount];
            Marshal.Copy(pixelBuffer, managedPixelBuffer, 0, pixelCount);
            
            Marshal.FreeHGlobal(pixelBuffer);

            float lastCorrectDepth = 0.5f;
            float minDepth = float.MaxValue;
            float maxDepth = float.MinValue;
            for (int i = 0; i < pixelCount; i++)
            {
                float depth = managedPixelBuffer[i];

                // Discard erroneous values.
                if (depth < 0.001f || depth > 0.999f)
                {
                    depth = lastCorrectDepth;
                }
                else
                {
                    lastCorrectDepth = depth;
                }

                // Linearize.
                depth = _near * _far / (_far - depth * (_far - _near));
                managedPixelBuffer[i] = depth;

                minDepth = Math.Min(minDepth, depth);
                maxDepth = Math.Max(maxDepth, depth);
            }

            ushort[] heightValues = new ushort[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                // The depth buffer is inverted (white is far away, black is near), so we'll flip the min and max here.
                ushort height = MathHelper.RemapToUShort(managedPixelBuffer[i], maxDepth, minDepth);

                int x = i % _captureResult.ImageWidthInPixels;
                int y = _captureResult.ImageHeightInPixels - 1 - (i / _captureResult.ImageWidthInPixels);
                int targetIndex = (y * _captureResult.ImageWidthInPixels) + x;

                heightValues[targetIndex] = height;
            }

            return heightValues;
        }

        private Color[] CaptureColor()
        {
            try
            {
                int pixelCount = _captureResult.ImageWidthInPixels * _captureResult.ImageHeightInPixels;
                IntPtr pixelBuffer = Marshal.AllocHGlobal(pixelCount * Marshal.SizeOf<byte>() * 4);
                OpenGLMethods.glReadPixels(0, 0, _captureResult.ImageWidthInPixels, _captureResult.ImageHeightInPixels, PixelFormat.GL_RGBA, DataType.GL_UNSIGNED_BYTE, pixelBuffer);

                byte[] managedPixelBuffer = new byte[pixelCount * 4];
                Marshal.Copy(pixelBuffer, managedPixelBuffer, 0, pixelCount * 4);
                Marshal.FreeHGlobal(pixelBuffer);

                Color[] colors = new Color[pixelCount];
                for (int i = 0; i < managedPixelBuffer.Length; i += 4)
                {
                    Color color = new Color(managedPixelBuffer[i], managedPixelBuffer[i + 1], managedPixelBuffer[i + 2]);

                    int x = (i / 4) % _captureResult.ImageWidthInPixels;
                    int y = _captureResult.ImageHeightInPixels - 1 - ((i / 4) / _captureResult.ImageWidthInPixels);
                    int targetIndex = (y * _captureResult.ImageWidthInPixels) + x;

                    colors[targetIndex] = color;
                }

                return colors;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }

        private void SampleCoordinatesAndElevation(IntPtr qWidget, IntPtr qEvent, bool isRenderWidget, ref bool? handled)
        {
            lock (_lock)
            {
                if (_captureState == CaptureState.SamplingCoordinatesAndElevation
                    && Qt5Methods.QEvent_Type(qEvent) == QEvent_Type.MouseMove
                    && isRenderWidget)
                {
                    try
                    {
                        Qt5Hooks.WidgetEvent -= SampleCoordinatesAndElevation;

                        QMouseEventPosition originalMousePosition = QMouseEventPosition.FromQMouseEvent(qEvent);
                        int screenXOffset = Convert.ToInt32(originalMousePosition.ScreenX - originalMousePosition.LocalX);
                        int screenYOffset = Convert.ToInt32(originalMousePosition.ScreenY - originalMousePosition.LocalY);

                        Logger.LogMessage("Sampling bounds", GetType().Name);
                        SampleBounds(qWidget, qEvent, screenXOffset, screenYOffset);

                        if (_captureFlags.HasFlag(CaptureFlags.Height))
                        {
                            Logger.LogMessage("Sampling min/max elevation", GetType().Name);
                            SampleMinMaxElevation(qWidget, qEvent, screenXOffset, screenYOffset);
                        }

                        originalMousePosition.ToQMouseEvent(qEvent);

                        _captureState = CaptureState.WritingCapture;
                        WriteCapture();
                    }
                    catch (Exception exception)
                    {
                        Reset();
                        Logger.LogException(exception);
                    }
                }
            }
        }

        private void SampleBounds(IntPtr qWidget, IntPtr qEvent, int screenXOffset, int screenYOffset)
        {
            EarthProStructs.CoordinateData topLeft = GetCoordinateDataAtScreenPosition(qWidget, qEvent, 0, 0, screenXOffset, screenYOffset);
            EarthProStructs.CoordinateData topRight = GetCoordinateDataAtScreenPosition(qWidget, qEvent, _captureResult.ImageWidthInPixels - 1, 0, screenXOffset, screenYOffset);
            EarthProStructs.CoordinateData bottomLeft = GetCoordinateDataAtScreenPosition(qWidget, qEvent, 0, _captureResult.ImageHeightInPixels - 1, screenXOffset, screenYOffset);
            EarthProStructs.CoordinateData bottomRight = GetCoordinateDataAtScreenPosition(qWidget, qEvent, _captureResult.ImageWidthInPixels - 1, _captureResult.ImageHeightInPixels - 1, screenXOffset, screenYOffset);

            _captureResult.Bounds = new Coordinate[]
            {
                topLeft.Coordinate,
                topRight.Coordinate,
                bottomLeft.Coordinate,
                bottomRight.Coordinate
            };

            _captureResult.ImageWidthInMeters = Coordinate.GetDistance(topLeft.Coordinate, topRight.Coordinate);
            _captureResult.ImageHeightInMeters = Coordinate.GetDistance(topLeft.Coordinate, bottomLeft.Coordinate);
        }

        private void SampleMinMaxElevation(IntPtr qWidget, IntPtr qEvent, int screenXOffset, int screenYOffset)
        {
            ushort minHeight = ushort.MaxValue;
            ushort maxHeight = ushort.MinValue;
            int minHeightX = 0, minHeightY = 0;
            int maxHeightX = 0, maxHeightY = 0;
            for (int i = 0; i < _captureResult.Height.Length; i++)
            {
                int x = i % _captureResult.ImageWidthInPixels;
                int y = i / _captureResult.ImageWidthInPixels;

                ushort depth = _captureResult.Height[i];

                if (depth < minHeight)
                {
                    minHeight = depth;
                    minHeightX = x;
                    minHeightY = y;
                }

                if (depth > maxHeight)
                {
                    maxHeight = depth;
                    maxHeightX = x;
                    maxHeightY = y;
                }
            }

            EarthProStructs.CoordinateData max = GetCoordinateDataAtScreenPosition(qWidget, qEvent, maxHeightX, maxHeightY, screenXOffset, screenYOffset);
            EarthProStructs.CoordinateData min = GetCoordinateDataAtScreenPosition(qWidget, qEvent, minHeightX, minHeightY, screenXOffset, screenYOffset);

            _captureResult.MinElevation = min.Elevation;
            _captureResult.MaxElevation = max.Elevation;
        }

        private EarthProStructs.CoordinateData GetCoordinateDataAtScreenPosition(IntPtr qWidget, IntPtr qMouseEvent, int x, int y, int screenXOffset, int screenYOffset)
        {
            QMouseEventPosition mousePosition = new QMouseEventPosition()
            {
                LocalX = x,
                LocalY = y,
                WindowX = x,
                WindowY = y,
                ScreenX = x + screenXOffset,
                ScreenY = y + screenYOffset
            };

            mousePosition.ToQMouseEvent(qMouseEvent);
            Qt5Methods.QWidget_Event(qWidget, qMouseEvent);

            return Marshal.PtrToStructure<EarthProStructs.CoordinateData>(_coordinateDataAddress);
        }

        private void WriteCapture()
        {
            if (_captureState == CaptureState.WritingCapture)
            {
                string fileName = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");

                Logger.LogMessage($"Capture complete, writing to {Path.Combine(_outputDirectory, fileName)}", GetType().Name);

                CaptureWriter.WriteToFile(_captureResult, _outputDirectory, fileName);
                _captureState = CaptureState.WaitingForInput;
            }
        }
    }

    internal enum CaptureState
    {
        WaitingForInput,
        WaitingForBufferSwap,
        CapturingBuffer,
        SamplingCoordinatesAndElevation,
        WritingCapture
    }

    internal struct CaptureResult
    {
        public CaptureFlags CaptureFlags;

        public ushort[] Height;
        public Color[] Color;

        public int ImageWidthInPixels;
        public int ImageHeightInPixels;

        public Coordinate[] Bounds;
        public double ImageWidthInMeters;
        public double ImageHeightInMeters;

        public double MinElevation;
        public double MaxElevation;
    }
}
