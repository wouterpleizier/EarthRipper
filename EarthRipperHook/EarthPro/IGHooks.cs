using EasyHook;
using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook.EarthPro
{
    internal class IGHooks : HookContainer
    {
        internal delegate void SetViewportHandler(IntPtr igOglVisualContext, ref int x, ref int y, ref int width, ref int height, ref float minZ, ref float maxZ);
        internal static event SetViewportHandler SetViewport;
        internal static event SetViewportHandler SetViewport_Exclusive;

        internal delegate void GenericDrawHandler(IntPtr igOglVisualContext, ref bool suppress);
        internal static event GenericDrawHandler GenericDraw;
        internal static event GenericDrawHandler GenericDraw_Exclusive;

        internal delegate void SetMatrixHandler(IntPtr igProjectionMatrixAttr, IntPtr matrix);
        internal static event SetMatrixHandler SetMatrix;
        internal static event SetMatrixHandler SetMatrix_Exclusive;

        internal delegate void BindShaderProgramHandler(IntPtr igProgramAttr, IntPtr igVisualContext);
        internal static event BindShaderProgramHandler BindShaderProgram;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void igOglVisualContext_SetViewportHookDelegate(IntPtr @this, int x, int y, int width, int height, float minZ, float maxZ);
        private readonly LocalHook _igOglVisualContextSetViewportHook;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void igOglVisualContext_GenericDrawHookDelegate(IntPtr @this, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5);
        private readonly LocalHook _igOglVisualContextGenericDrawHook;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void igProjectionMatrixAttr_SetMatrixHookDelegate(IntPtr @this, IntPtr matrix);
        private readonly LocalHook _igProjectionMatrixAttrSetMatrixHook;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool igProgramAttr_BindHookDelegate(IntPtr @this, IntPtr igVisualContext);
        private readonly LocalHook _igProgramAttrBindHook;

        public IGHooks()
        {
            _igOglVisualContextSetViewportHook = CreateHook("IGGfx.dll",
                "?setViewport@igOglVisualContext@Gfx@Gap@@UAEXHHHHMM@Z",
                new igOglVisualContext_SetViewportHookDelegate(igOglVisualContext_SetViewportHook));

            _igOglVisualContextGenericDrawHook = CreateHook("IGGfx.dll",
                "?genericDraw@igOglVisualContext@Gfx@Gap@@IAEXHHHHH@Z",
                new igOglVisualContext_GenericDrawHookDelegate(igOglVisualContext_GenericDrawHook));

            _igProjectionMatrixAttrSetMatrixHook = CreateHook("IGAttrs.dll",
                "?setMatrix@igProjectionMatrixAttr@Attrs@Gap@@UAEXABVigMatrix44f@Math@3@@Z",
                new igProjectionMatrixAttr_SetMatrixHookDelegate(igProjectionMatrixAttr_SetMatrixHook));

            _igProgramAttrBindHook = CreateHook("IGAttrs.dll",
                "?bind@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z",
                new igProgramAttr_BindHookDelegate(igProgramAttr_BindHook));
        }

        public override void Dispose()
        {
            _igOglVisualContextSetViewportHook.Dispose();
            _igOglVisualContextGenericDrawHook.Dispose();
            _igProjectionMatrixAttrSetMatrixHook.Dispose();
            _igProgramAttrBindHook.Dispose();
        }

        private void igOglVisualContext_SetViewportHook(IntPtr @this, int x, int y, int width, int height, float minZ, float maxZ)
        {
            if (SetViewport_Exclusive != null)
            {
                SetViewport_Exclusive.Invoke(@this, ref x, ref y, ref width, ref height, ref minZ, ref maxZ);
            }
            else
            {
                SetViewport?.Invoke(@this, ref x, ref y, ref width, ref height, ref minZ, ref maxZ);
            }

            IGMethods.igOglVisualContext_SetViewport(@this, x, y, width, height, minZ, maxZ);
        }

        private void igOglVisualContext_GenericDrawHook(IntPtr @this, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5)
        {
            bool suppress = false;
            if (GenericDraw_Exclusive != null)
            {
                GenericDraw_Exclusive.Invoke(@this, ref suppress);
            }
            else
            {
                GenericDraw?.Invoke(@this, ref suppress);
            }

            if (!suppress)
            {
                IGMethods.igOglVisualContext_GenericDraw(@this, unknown1, unknown2, unknown3, unknown4, unknown5);
            }
        }

        private void igProjectionMatrixAttr_SetMatrixHook(IntPtr @this, IntPtr matrix)
        {
            if (SetMatrix_Exclusive != null)
            {
                SetMatrix_Exclusive.Invoke(@this, matrix);
            }
            else
            {
                SetMatrix?.Invoke(@this, matrix);
            }

            IGMethods.igProjectionMatrixAttr_SetMatrix(@this, matrix);
        }

        private bool igProgramAttr_BindHook(IntPtr @this, IntPtr igVisualContext)
        {
            BindShaderProgram?.Invoke(@this, igVisualContext);

            return IGMethods.igProgramAttr_Bind(@this, igVisualContext);
        }
    }
}
