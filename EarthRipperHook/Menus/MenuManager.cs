using System.Collections.Concurrent;
using static EarthRipperHook.Native.Qt5Core;
using static EarthRipperHook.Native.Qt5Widgets;

namespace EarthRipperHook.Menus
{
    internal static class MenuManager
    {
        private const string MainMenuTitle = "EarthRipper";
        internal static Menu MainMenu { get; } = new Menu(MainMenuTitle);

        private readonly static ConcurrentQueue<Action> _updateQueue = [];
        private readonly static ConcurrentDictionary<nuint, BaseMenuItem> _menuItems = [];
        private readonly static ConcurrentDictionary<nuint, MenuAction> _menuActions = [];

        internal static void Initialize()
        {
            Hook<QMainWindow.Event>(HandleMainWindowEvent);
            Hook<QAction.Activate>(HandleActionActivated);
        }

        internal static Menu AddMenu(Menu parent, string text)
        {
            Menu menu = new Menu(text);
            
            _updateQueue.Enqueue(() =>
            {
                menu.NativePointer = Original<QMenu.AddMenu>()(parent.NativePointer, new QString(menu.Text).NativeQString);
                _menuItems[menu.NativePointer] = menu;
            });

            return menu;
        }

        internal static MenuAction AddAction(Menu parent, string text, MenuActionHandler handler, bool checkable = false)
        {
            MenuAction action = new MenuAction(text, handler, checkable);
            
            _updateQueue.Enqueue(() =>
            {
                action.NativePointer = Original<QMenu.AddAction>()(parent.NativePointer, new QString(action.Text).NativeQString);

                if (action.Checkable)
                {
                    Original<QAction.SetCheckable>()(action.NativePointer, true);
                }

                _menuItems[action.NativePointer] = action;
                _menuActions[action.NativePointer] = action;
            });

            return action;
        }

        internal static bool GetActionIsChecked(MenuAction action)
        {
            return Original<QAction.IsChecked>()(action.NativePointer);
        }

        internal static void SetActionIsChecked(MenuAction action, bool isChecked)
        {
            _updateQueue.Enqueue(() =>
            {
                Original<QAction.SetChecked>()(action.NativePointer, isChecked);
            });
        }

        internal static void RemoveAction(Menu parent, MenuAction action)
        {
            _updateQueue.Enqueue(() =>
            {
                Original<QWidget.RemoveAction>()(parent.NativePointer, action.NativePointer);

                _menuItems.TryRemove(action.NativePointer, out _);
                _menuActions.TryRemove(action.NativePointer, out _);

                action.NativePointer = nuint.Zero;
            });
        }

        internal static MenuSeparator AddSeparator(Menu parent)
        {
            MenuSeparator separator = new MenuSeparator();

            _updateQueue.Enqueue(() =>
            {
                separator.NativePointer = Original<QMenu.AddSeparator>()(parent.NativePointer);
                _menuItems[separator.NativePointer] = separator;
            });

            return separator;
        }

        private static void HandleMainWindowEvent(nuint qMainWindow, nuint qEvent)
        {
            if (MainMenu.NativePointer == nuint.Zero)
            {
                nuint menuBar = Original<QMainWindow.MenuBar>()(qMainWindow);
                MainMenu.NativePointer = Original<QMenuBar.AddMenu>()(menuBar, new QString(MainMenuTitle).NativeQString);
            }

            while (MainMenu.NativePointer != nuint.Zero && _updateQueue.TryDequeue(out Action? updateAction))
            {
                updateAction.Invoke();
            }
        }

        private static void HandleActionActivated(nuint qAction, int actionEvent)
        {
            const int qActionTrigger = 0;

            if (actionEvent == qActionTrigger && _menuActions.TryGetValue(qAction, out MenuAction? action))
            {
                InvokeAfterCompletion<QAction.Activate>(_ =>
                {
                    bool isChecked = Original<QAction.IsChecked>()(qAction);
                    action.Handler.Invoke(action, isChecked);
                });
            }
        }
    }
}
