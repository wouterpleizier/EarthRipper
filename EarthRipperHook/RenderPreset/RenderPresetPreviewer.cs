using EarthRipperHook.Menus;

namespace EarthRipperHook.RenderPreset
{
    internal class RenderPresetPreviewer
    {
        private readonly Menu _renderPresetsMenu;
        private readonly Dictionary<string, MenuAction> _renderPresetActions = new(StringComparer.OrdinalIgnoreCase);

        public RenderPresetPreviewer()
        {
            _renderPresetsMenu = MenuManager.MainMenu.AddMenu("Render presets");

            MenuAction defaultAction = _renderPresetsMenu.AddAction(
                text: RenderPresetDefinition.DefaultName,
                handler: HandleRenderPresetAction,
                checkable: true);

            _renderPresetActions[RenderPresetDefinition.DefaultName] = defaultAction;
            defaultAction.IsChecked = true;
            
            _renderPresetsMenu.AddSeparator();

            RenderPresetManager.RenderPresetAdded += HandleRenderPresetAdded;
            RenderPresetManager.RenderPresetRemoved += HandleRenderPresetRemoved;
        }

        private void HandleRenderPresetAdded(RenderPresetDefinition renderPreset)
        {
            string name = renderPreset.Name;
            if (name != RenderPresetDefinition.DefaultName)
            {
                MenuAction action = _renderPresetsMenu.AddAction(name, HandleRenderPresetAction, checkable: true);
                _renderPresetActions[renderPreset.Name] = action;
            }
        }

        private void HandleRenderPresetRemoved(RenderPresetDefinition renderPreset)
        {
            if (renderPreset.Name != RenderPresetDefinition.DefaultName
                && _renderPresetActions.TryGetValue(renderPreset.Name, out MenuAction? action))
            {
                _renderPresetsMenu.RemoveAction(action);
            }
        }

        private void HandleRenderPresetAction(MenuAction action, bool isChecked)
        {
            if (RenderPresetManager.GetRenderPreset(action.Text, true) is RenderPresetDefinition renderPreset)
            {
                RenderPresetManager.ActivateRenderPreset(renderPreset, RenderPresetContext.Preview);
                CheckSingleAction(action);
            }
            else
            {
                RenderPresetManager.DeactivateRenderPreset(RenderPresetContext.Preview);
                UncheckAllActions();
            }
        }

        private void CheckSingleAction(MenuAction action)
        {
            foreach (MenuAction otherAction in _renderPresetActions.Values)
            {
                otherAction.IsChecked = otherAction == action;
            }
        }

        private void UncheckAllActions()
        {
            foreach (MenuAction otherAction in _renderPresetActions.Values)
            {
                otherAction.IsChecked = false;
            }
        }
    }
}
