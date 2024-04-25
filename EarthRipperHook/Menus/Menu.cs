namespace EarthRipperHook.Menus
{
    internal class Menu(string text) : BaseMenuItem
    {
        internal string Text { get; } = text;

        public Menu AddMenu(string text) =>
            MenuManager.AddMenu(this, text);

        public MenuAction AddAction(string text, MenuActionHandler handler, bool checkable = false) =>
            MenuManager.AddAction(this, text, handler, checkable);

        public void RemoveAction(MenuAction action) =>
            MenuManager.RemoveAction(this, action);

        public MenuSeparator AddSeparator() =>
            MenuManager.AddSeparator(this);
    }
}
