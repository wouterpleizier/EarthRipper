namespace EarthRipperHook
{
    internal class MenuAction(string text, Action<bool> action, bool checkable = false)
    {
        internal string Text { get; } = text;
        internal bool Checkable { get; } = checkable;
        internal Action<bool> Action { get; } = action;
    }
}
