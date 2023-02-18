using System;
using System.Runtime.InteropServices;

namespace EarthRipperHook.EarthPro
{
    internal static class IGMethods
    {
        [DllImport("IGGfx.dll", EntryPoint = "?setViewport@igOglVisualContext@Gfx@Gap@@UAEXHHHHMM@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern void igOglVisualContext_SetViewport(IntPtr @this, int x, int y, int width, int height, float minZ, float maxZ);

        [DllImport("IGGfx.dll", EntryPoint = "?genericDraw@igOglVisualContext@Gfx@Gap@@IAEXHHHHH@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern void igOglVisualContext_GenericDraw(IntPtr @this, int unknown1, int unknown2, int unknown3, int unknown4, int unknown5);

        [DllImport("IGGfx.dll", EntryPoint = "?getCurrentProgramHandle@igOglVisualContext@Gfx@Gap@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern int igOglVisualContext_GetCurrentProgramHandle(IntPtr @this);

        [DllImport("IGAttrs.dll", EntryPoint = "?setMatrix@igProjectionMatrixAttr@Attrs@Gap@@UAEXABVigMatrix44f@Math@3@@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern void igProjectionMatrixAttr_SetMatrix(IntPtr @this, IntPtr matrix);

        [DllImport("IGAttrs.dll", EntryPoint = "?bind@igProgramAttr@Attrs@Gap@@QAE_NPAVigVisualContext@Gfx@3@@Z", CallingConvention = CallingConvention.ThisCall)]
        internal static extern bool igProgramAttr_Bind(IntPtr @this, IntPtr igVisualContext);

        [DllImport("IGAttrs.dll", EntryPoint = "?getName@igProgramAttr@Attrs@Gap@@QBEPBDXZ", CallingConvention = CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        internal static extern IntPtr igProgramAttr_GetName(IntPtr @this);

        [DllImport("IGAttrs.dll", EntryPoint = "?getProgramHandle@igProgramAttr@Attrs@Gap@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        internal static extern int igProgramAttr_GetProgramHandle(IntPtr @this);
    }
}
