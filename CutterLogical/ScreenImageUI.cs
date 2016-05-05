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
using Brushes = System.Windows.Media.Brushes;
using Clipboard = System.Windows.Clipboard;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using SystemFonts = System.Windows.SystemFonts;

namespace CutterLogical
{
    public class ScreenImageUI : Window
    {

        private readonly MainCaptureScreen _mainCaptureScreenOwner;
        private Bitmap _screenSnapBitmap;
        private MaskingCanvas _maskingCanvas;
        private BitmapSource bmpSource;

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
                bmpSource = _screenSnapBitmap.BitmapToBitmapSource();
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
                var rtbitmap = new RenderTargetBitmap(bmpSource.PixelWidth, bmpSource.PixelHeight, bmpSource.DpiX,
                    bmpSource.DpiY, PixelFormats.Default);    
                Pen pen=new Pen();
                pen.Thickness = 2;
                pen.Brush=new SolidColorBrush(Colors.Red);   
                var drawingVisual=new DrawingVisual();
                using (var dc = drawingVisual.RenderOpen())
                {
                    dc.DrawImage(bmpSource, new Rect(0, 0, bmpSource.Width, bmpSource.Height));
                    foreach (Rect rect in listRectangleRects)
                    {
                        dc.DrawRectangle(new SolidColorBrush(Colors.Transparent),pen,rect);
                    }
                    foreach (Rect ellipseTect in listEllipseTects)
                    {                        
                        dc.DrawEllipse(new SolidColorBrush(Colors.Transparent),pen,new Point((ellipseTect.Left+ellipseTect.Right)/2, (ellipseTect.Top + ellipseTect.Bottom) / 2),(ellipseTect.Right-ellipseTect.Left)/2,(ellipseTect.Bottom-ellipseTect.Top)/2);
                    }
                    foreach (RectToTextParameter rectToTextParameter in listTextRects)
                    {
                        var text=new FormattedText(rectToTextParameter.Text,System.Globalization.CultureInfo.CurrentUICulture,System.Windows.FlowDirection.LeftToRight,new Typeface(SystemFonts.MessageFontFamily,FontStyles.Normal,FontWeights.Normal,FontStretches.Normal),12,Brushes.Red );
                        text.MaxTextHeight = rectToTextParameter.Rect.Height;
                        text.MaxTextWidth = rectToTextParameter.Rect.Width;
                        dc.DrawText(text,new Point(rectToTextParameter.Rect.X,rectToTextParameter.Rect.Y));
                    }                  
                }
                rtbitmap.Render(drawingVisual);


                //截图完毕，放到剪贴板当中
                Bitmap catchedBmp = new Bitmap((int)selectedRegion.Width, (int)selectedRegion.Height);
                Graphics g = Graphics.FromImage(catchedBmp);
                g.DrawImage(rtbitmap.RenderTargetBitmapToBitmap(),
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