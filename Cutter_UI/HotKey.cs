using System;
using System.Collections;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Cutter_UI
{
    /// <summary>
    /// 热键
    /// </summary>
    public class HotKey
    {
        [System.Runtime.InteropServices.DllImport("user32.dll",SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, Keys virtualKey);

        [System.Runtime.InteropServices.DllImport("user32.dll",SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    public enum KeyFlags
    {
        MOD_NONE=0,
        MOD_ALT = 1,
        MOD_CTRL = 2,
        MOD_SHIFT = 4,
        MOD_WIN = 8
    }
}