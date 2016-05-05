using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CutterLogical.EventArgsDefinition;
using Brush = System.Drawing.Brush;
using Clipboard = System.Windows.Clipboard;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

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
            Grid grid=new Grid();
            grid.Children.Add(_maskingCanvas);
            Content = grid;
        }


        /// <summary>
        /// 进行截图操作
        /// </summary>
        /// <param name="selectedRegion"></param>
        public void SnapshotClipToBoard(Rect selectedRegion,List<Rect> listRectangleRects,List<Rect> listEllipseTects,List<RectToTextParameter> listTextRects)
        {
            if (!selectedRegion.IsEmpty)
            {
                //截图完毕，放到剪贴板当中
                Bitmap catchedBmp = new Bitmap((int) selectedRegion.Width, (int) selectedRegion.Height);
                Graphics g = Graphics.FromImage(catchedBmp);
                Brush brush=new SolidBrush(Color.Red);
                Pen pen=new Pen(brush, 2);               
                foreach (Rect rect in listRectangleRects)
                {
                    g.DrawRectangle(pen,new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
                }
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