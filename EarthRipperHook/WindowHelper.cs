using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace EarthRipperHook
{
    internal static class WindowHelper
    {
        private delegate bool EnumChildWindowsDelegate(IntPtr windowHandle, ArrayList windowHandles);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildWindowsDelegate lpEnumFunc, ArrayList lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public static IntPtr FindChildWindow(IntPtr parentWindowHandle, string name)
        {
            ArrayList windowHandles = new ArrayList();
            EnumChildWindows(parentWindowHandle, GetWindowHandle, windowHandles);

            foreach (IntPtr childWindowHandle in windowHandles)
            {
                int length = GetWindowTextLength(childWindowHandle);
                StringBuilder stringBuilder = new StringBuilder(length + 1);
                GetWindowText(childWindowHandle, stringBuilder, stringBuilder.Capacity);

                string childWindowName = stringBuilder.ToString();
                if (childWindowName == name)
                {
                    return childWindowHandle;
                }
            }

            return IntPtr.Zero;
        }

        private static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
        {
            windowHandles.Add(windowHandle);
            return true;
        }
    }
}
