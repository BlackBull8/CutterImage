using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CutterLogical;
using Cutter_UI.UserControls;
using MessageBox = System.Windows.MessageBox;

namespace Cutter_UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainCaptureScreen _mainCaptureScreen = new MainCaptureScreen();
        private WindowInteropHelper _wndHelper;
        public MainWindow()
        {
            InitializeComponent();
            
            //获取屏幕的宽度和高度
            int width= (int)SystemParameters.PrimaryScreenWidth;
            int height=(int)SystemParameters.PrimaryScreenHeight;
            this.Top = 0;
            this.Left = width/2-200;
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
            uint ctrlHotKey = (uint) (KeyFlags.MOD_ALT | KeyFlags.MOD_CTRL);
            _wndHelper = new WindowInteropHelper(this);
            if (!HotKey.RegisterHotKey(_wndHelper.Handle, 100, ctrlHotKey, Keys.A))
            {
                //MessageBox.Show("热键已被注册，已不能使用！！请点击按钮进行操作");
                MessageBoxDiy.Show("提示", "热键已被注册，已不能使用!\r\n请点击按钮进行操作!");
            }
        }

        private void _mainCaptureScreen_NotifyEventHanlder(object sender, EventArgs e)
        {
            //设置窗口状态
            WindowState=WindowState.Normal;
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
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
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
