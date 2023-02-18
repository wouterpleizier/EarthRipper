using EarthRipperHook.EarthPro;
using EarthRipperHook.Qt5;
using System;
using System.Collections.Generic;

namespace EarthRipperHook.Utilities
{
    internal class HideUI : Utility
    {
        private bool _hideUI;
        private HashSet<int> _knownShaderProgramHandles = new HashSet<int>();

        public HideUI()
        {
            Qt5Hooks.RenderWidgetKeyPress += HandleRenderWidgetKeyPress;
            IGHooks.BindShaderProgram += HandleBindShaderProgram;
            IGHooks.GenericDraw += HandleGenericDraw;
        }

        public override void Dispose()
        {
            Qt5Hooks.RenderWidgetKeyPress -= HandleRenderWidgetKeyPress;
            IGHooks.BindShaderProgram -= HandleBindShaderProgram;
            IGHooks.GenericDraw -= HandleGenericDraw;
        }

        private void HandleRenderWidgetKeyPress(Key key, ref bool? handled)
        {
            if (key == Key.H)
            {
                if (_hideUI)
                {
                    Logger.LogMessage("Showing UI", GetType().Name);
                }
                else
                {
                    Logger.LogMessage("Hiding UI", GetType().Name);
                }

                _hideUI = !_hideUI;
                RenderWidgetHelper.ForceRedraw();
            }
        }

        private void HandleBindShaderProgram(IntPtr igProgramAttr, IntPtr igVisualContext)
        {
            int shaderProgramHandle = IGMethods.igProgramAttr_GetProgramHandle(igProgramAttr);
            _knownShaderProgramHandles.Add(shaderProgramHandle);
        }

        private void HandleGenericDraw(IntPtr igOglVisualContext, ref bool suppress)
        {
            if (_hideUI)
            {
                int handle = IGMethods.igOglVisualContext_GetCurrentProgramHandle(igOglVisualContext);

                // The UI shader appears to be the only one that doesn't pass through igProgramAttr::Bind().
                if (!_knownShaderProgramHandles.Contains(handle))
                {
                    suppress = true;
                }
            }
        }
    }
}
