using System;
using System.Collections;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Cutter_UI
{
    /// <summary>
    /// 直接创建类实例就可以实现注册
    /// 自动会完成注销
    /// 注意注册时会抛出异常
    /// </summary>
    public class HotKey
    {
        //热键编号
        private int _keyId;
        //窗体句柄
        private IntPtr _handle;
        //热键所在窗体
        private Window _window;
        //热键控制键
        private uint _controlKey;
        //热键主键
        private uint _key;
        //热键事件委托
        public delegate void OnHotKeyEventHandler();
        //热键事件
        public event OnHotKeyEventHandler OnHotKey = null;
        //热键哈希表
        static Hashtable _keyPair=new Hashtable();
        //热键信息编号
        private const int WM_HOTKTY = 0x0312;
        //控制键编码
        public enum KeyFlags
        {
            MOD_ALT=0x1,
            MOD_CONTROL=0x2,
            MOD_SHIFT=0x4,
            MOD_WIN=0x8
        }

        /// <summary>
        /// 构造函数，注册热键
        /// </summary>
        /// <param name="win">注册窗体</param>
        /// <param name="control">控制键</param>
        /// <param name="key">主键</param>
        public HotKey(Window win, HotKey.KeyFlags control, Keys key)
        {
            _handle = new WindowInteropHelper(win).Handle;
            _window = win;
            _controlKey = (uint) control;
            _key = (uint) key;
            _keyId = (int) _controlKey + (int) _key*10;
            if (HotKey._keyPair.ContainsKey(_keyId))
            {
                throw new Exception("热键已经被注册！");
            }
            //注册热键
            if (HotKey.RegisterHotKey(_handle, _keyId, _controlKey, _key)==false)
            {
                //消息挂钩只能连接一次
                throw new Exception("热键注册失败！");
            }
            if (HotKey._keyPair.Count == 0)
            {
                if (InstallHotKeyHook(this) == false)
                {
                    throw new Exception("消息挂钩连接失败！");
                }
            }
            HotKey._keyPair.Add(_keyId,this);
        }

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

        [System.Runtime.InteropServices.DllImport("usr32")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static bool InstallHotKeyHook(HotKey hk)
        {
            if (hk._window == null || hk._handle == IntPtr.Zero)
            {
                return false;
            }
            //获得消息源
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(hk._handle);
            if (source == null) return false;

            //挂接事件
            source.AddHook(HotKey.HotKeyHook);
            return true;
        }

        //热键处理过程
        private static IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_HOTKTY)
            {
                HotKey hk = (HotKey) HotKey._keyPair[(int) wparam];
                if (hk.OnHotKey != null) hk.OnHotKey();
            }
            return IntPtr.Zero;
        }

        ~HotKey()
        {
            HotKey.UnregisterHotKey(_handle, _keyId);
        }
    }
}