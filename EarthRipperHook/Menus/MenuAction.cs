namespace EarthRipperHook.Menus
{
    internal class MenuAction(string text, MenuActionHandler handler, bool checkable = false) : BaseMenuItem
    {
        internal string Text { get; } = text;
        internal bool Checkable { get; } = checkable;
        internal MenuActionHandler Handler { get; } = handler;

        internal bool IsChecked
        {
            get => MenuManager.GetActionIsChecked(this);
            set => MenuManager.SetActionIsChecked(this, value);
        }
    }

    internal delegate void MenuActionHandler(MenuAction action, bool isChecked);
}
