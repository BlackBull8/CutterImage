using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CutterLogical
{
    /// <summary>
    ///     热键
    /// </summary>
    public class HotKey
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, Keys virtualKey);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    public enum KeyFlags
    {
        MOD_NONE = 0,
        MOD_ALT = 1,
        MOD_CTRL = 2,
        MOD_SHIFT = 4,
        MOD_WIN = 8
    }
}