using System.Collections.Concurrent;
using static EarthRipperHook.Native.Qt5Core;
using static EarthRipperHook.Native.Qt5Widgets;

namespace EarthRipperHook
{
    internal static class MenuManager
    {
        private const string MenuTitle = "EarthRipper";

        private readonly static ConcurrentQueue<MenuAction> _enqueuedActions = [];
        private readonly static ConcurrentDictionary<nuint, MenuAction> _actions = [];

        private static nuint _menu;

        internal static void Initialize()
        {
            Hook<QMainWindow.Event>(HandleMainWindowEvent);
            Hook<QAction.Activate>(HandleActionActivated);
        }

        internal static MenuAction AddAction(string name, Action<bool> action, bool checkable = false)
        {
            MenuAction menuAction = new MenuAction(name, action, checkable);
            _enqueuedActions.Enqueue(menuAction);

            return menuAction;
        }

        private static void HandleMainWindowEvent(nuint qMainWindow, nuint qEvent)
        {
            if (_menu == nuint.Zero)
            {
                nuint menuBar = Original<QMainWindow.MenuBar>()(qMainWindow);
                _menu = Original<QMenuBar.AddMenu>()(menuBar, new QString(MenuTitle).NativeQString);
            }

            while (_enqueuedActions.TryDequeue(out MenuAction? menuAction))
            {
                nuint qAction = Original<QMenu.AddAction>()(_menu, new QString(menuAction.Text).NativeQString);

                if (menuAction.Checkable)
                {
                    Original<QAction.SetCheckable>()(qAction, true);
                }

                _actions[qAction] = menuAction;
            }
        }

        private static void HandleActionActivated(nuint qAction, int actionEvent)
        {
            if (actionEvent == 0 && _actions.TryGetValue(qAction, out MenuAction? menuAction))
            {
                InvokeAfterCompletion<QAction.Activate>(() =>
                {
                    bool isChecked = Original<QAction.IsChecked>()(qAction);
                    menuAction.Action.Invoke(isChecked);
                });
            }
        }
    }
}
