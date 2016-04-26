using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CutterLogical;

namespace Cutter_UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainCaptureScreen _mainCaptureScreen = new MainCaptureScreen();
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
        }

        private void _mainCaptureScreen_NotifyEventHanlder(object sender, EventArgs e)
        {
            //设置窗口状态
            WindowState=WindowState.Normal;
        }
    }
}
