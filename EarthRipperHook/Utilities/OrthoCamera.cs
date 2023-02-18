using EarthRipperHook.EarthPro;
using EarthRipperHook.Qt5;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EarthRipperHook.Utilities
{
    internal class OrthoCamera : Utility
    {
        private bool _enableOrthographicCamera;

        public OrthoCamera()
        {
            Qt5Hooks.RenderWidgetKeyPress += HandleRenderWidgetKeyPress;
            IGHooks.SetMatrix += HandleSetMatrix;
        }

        public override void Dispose()
        {
            Qt5Hooks.RenderWidgetKeyPress -= HandleRenderWidgetKeyPress;
            IGHooks.SetMatrix -= HandleSetMatrix;
        }

        private void HandleRenderWidgetKeyPress(Key key, ref bool? handled)
        {
            if (key == Key.O)
            {
                if (_enableOrthographicCamera)
                {
                    Logger.LogMessage("Disabling orthographic camera", GetType().Name);
                }
                else
                {
                    Logger.LogMessage("Enabling orthographic camera", GetType().Name);
                }

                _enableOrthographicCamera = !_enableOrthographicCamera;
                RenderWidgetHelper.ForceRedraw();
            }
        }
        
        private void HandleSetMatrix(IntPtr igProjectionMatrixAttr, IntPtr matrix)
        {
            if (_enableOrthographicCamera)
            {
                Matrix4x4 managedMatrix = Marshal.PtrToStructure<Matrix4x4>(matrix);

                if (Math.Abs(managedMatrix.M34 - (-1f)) < 0.01f)
                {
                    float near = managedMatrix.M43 / (managedMatrix.M33 - 1);
                    float far = managedMatrix.M43 / (managedMatrix.M33 + 1);
                    float bottom = near * (managedMatrix.M32 - 1) / managedMatrix.M22;
                    float top = near * (managedMatrix.M32 + 1) / managedMatrix.M22;
                    float left = near * (managedMatrix.M31 - 1) / managedMatrix.M11;
                    float right = near * (managedMatrix.M31 + 1) / managedMatrix.M11;

                    float scale = 3f;
                    managedMatrix = Matrix4x4.CreateOrthographicOffCenter(left * scale, right * scale, bottom * scale, top * scale, near, far);

                    Marshal.StructureToPtr(managedMatrix, matrix, false);
                }
            }
        }
    }
}
