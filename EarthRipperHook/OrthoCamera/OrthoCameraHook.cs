using Reloaded.Hooks.Definitions.Helpers;
using System.Numerics;
using System.Runtime.InteropServices;
using static EarthRipperHook.Native.IGAttrs;

namespace EarthRipperHook.OrthoCamera
{
    internal class OrthoCameraHook
    {
        private bool _enableOrthographicCamera;

        public OrthoCameraHook()
        {
            MenuManager.AddAction("Orthographic camera", ToggleOrthographicCamera, true);

            Hook<IGProjectionMatrixAttr.SetMatrix>(HandleSetMatrix);
        }

        private void ToggleOrthographicCamera(bool enable)
        {
            _enableOrthographicCamera = enable;
        }

        private void HandleSetMatrix(nuint igProjectionMatrixAttr, nuint igMatrix44f)
        {
            if (_enableOrthographicCamera)
            {
                Matrix4x4 matrix = Marshal.PtrToStructure<Matrix4x4>(igMatrix44f.ToSigned());

                if (Math.Abs(matrix.M34 - (-1f)) < 0.01f)
                {
                    float near = matrix.M43 / (matrix.M33 - 1);
                    float far = matrix.M43 / (matrix.M33 + 1);
                    float bottom = near * (matrix.M32 - 1) / matrix.M22;
                    float top = near * (matrix.M32 + 1) / matrix.M22;
                    float left = near * (matrix.M31 - 1) / matrix.M11;
                    float right = near * (matrix.M31 + 1) / matrix.M11;

                    float scale = 3f;
                    matrix = Matrix4x4.CreateOrthographicOffCenter(left * scale, right * scale, bottom * scale, top * scale, near, far);

                    Marshal.StructureToPtr(matrix, igMatrix44f.ToSigned(), false);
                }
            }
        }
    }
}
