using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Clipboard = System.Windows.Clipboard;

namespace CutterLogical
{
    public class ScreenImageUI : Window
    {

        private readonly MainCaptureScreen _mainCaptureScreenOwner;
        private Bitmap _screenSnapBitmap;
        private MaskingCanvas _maskingCanvas;

        public ScreenImageUI(MainCaptureScreen mainCaptureScreen)
        {
            this._mainCaptureScreenOwner = mainCaptureScreen;
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            //设置窗体属性
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;

            //设置窗体的显式位置（有可能是双屏）
            var rect = SystemInformation.VirtualScreen;
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            //获取截屏的图片
            _screenSnapBitmap = HelperClass.GetScreenCutter();
            if (_screenSnapBitmap != null)
            {
                BitmapSource bmpSource = _screenSnapBitmap.BitmapToBitmapSource();
                bmpSource.Freeze();
                //设置背景
                Background = new ImageBrush(bmpSource);
            }

            //设置完背景之后要把遮罩层弹出来
            _maskingCanvas = new MaskingCanvas() {MaskingCanvasOwner = this};
            Content = _maskingCanvas;
        }


        /// <summary>
        /// 进行截图操作
        /// </summary>
        /// <param name="selectedRegion"></param>
        public void SnapshotClipToBoard(Rect selectedRegion)
        {
            if (!selectedRegion.IsEmpty)
            {
                //截图完毕，放到剪贴板当中
                Bitmap catchedBmp = new Bitmap((int) selectedRegion.Width, (int) selectedRegion.Height);
                Graphics g = Graphics.FromImage(catchedBmp);
                g.DrawImage(_screenSnapBitmap,
                    new System.Drawing.Rectangle(0, 0, (int) selectedRegion.Width, (int) selectedRegion.Height),
                    new System.Drawing.Rectangle((int) selectedRegion.X, (int) selectedRegion.Y,
                        (int) selectedRegion.Width, (int) selectedRegion.Height), GraphicsUnit.Pixel);
                BitmapSource bs = catchedBmp.BitmapToBitmapSource();
                Clipboard.SetImage(bs);
                g.Dispose();
                catchedBmp.Dispose();
                this.DialogResult = true;
                Close();
            }
        }

        
    }
}