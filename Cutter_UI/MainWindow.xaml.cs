using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using CutterLogical;
using CutterLogical.UserControls;

namespace Cutter_UI
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainCaptureScreen _mainCaptureScreen = new MainCaptureScreen();
        private WindowInteropHelper _wndHelper;

        public MainWindow()
        {
            InitializeComponent();

            //获取屏幕的宽度和高度
            var width = (int) SystemParameters.PrimaryScreenWidth;
            var height = (int) SystemParameters.PrimaryScreenHeight;
            Top = 0;
            Left = width/2 - 200;
        }

        private void CutterBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            Thread.Sleep(300);
            _mainCaptureScreen.StartToCapture();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //指定事件
            _mainCaptureScreen.NotifyEventHanlder += _mainCaptureScreen_NotifyEventHanlder;
            var ctrlHotKey = (uint) (KeyFlags.MOD_ALT | KeyFlags.MOD_CTRL);
            _wndHelper = new WindowInteropHelper(this);
            if (!HotKey.RegisterHotKey(_wndHelper.Handle, 100, ctrlHotKey, Keys.A))
            {
                MessageBoxDiy.Show("提示", "热键已被注册，已不能使用!\r\n请点击按钮进行操作!");
            }
        }

        private void _mainCaptureScreen_NotifyEventHanlder(object sender, EventArgs e)
        {
            //设置窗口状态
            WindowState = WindowState.Normal;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            HotKey.UnregisterHotKey(_wndHelper.Handle, 100);
        }

        private void GlobalKeyProcess()
        {
            WindowState = WindowState.Minimized;
            Thread.Sleep(300);
            _mainCaptureScreen.StartToCapture();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (wparam.ToInt32() == 100)
            {
                GlobalKeyProcess();
            }
            return IntPtr.Zero;
        }
    }
}