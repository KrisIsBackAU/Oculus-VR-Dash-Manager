using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OVR_Dash_Manager.Functions
{
    public static class Native_Functions
    {

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOW = 5;

        [DllImport("User32")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        public static void ShowExternalWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
                ShowWindow(hwnd, SW_SHOW);
        }

        public static void HideExternalWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
                ShowWindow(hwnd, SW_HIDE);
        }

        public static void MinimizeExternalWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
                ShowWindow(hwnd, SW_SHOWMINIMIZED);
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        public static String GetWindowText(IntPtr pHandle)
        {
            String pReturn = String.Empty;
            Int32 pDataLength = GetWindowTextLength(pHandle);
            pDataLength++; // Increase 1 for saftey
            StringBuilder Buff = new StringBuilder(pDataLength);

            if (GetWindowText(pHandle, Buff, pDataLength) > 0)
                pReturn = Buff.ToString();

            Buff.Clear();
            return pReturn;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern Int32 GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder lpString, Int32 nMaxCount);
    }
}
