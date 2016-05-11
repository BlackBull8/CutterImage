using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CutterLogical.EventArgsDefinition;
using CutterLogical.UserControls;
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
        private BitmapSource _bmpSource;
        private WindowInteropHelper _wndHelper;

        public ScreenImageUI(MainCaptureScreen mainCaptureScreen)
        {
            _mainCaptureScreenOwner = mainCaptureScreen;
            Init();
            Loaded += ScreenImageUI_Loaded;
            Closing += ScreenImageUI_Closing;
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
                _bmpSource = _screenSnapBitmap.BitmapToBitmapSource();
                _bmpSource.Freeze();
                //设置背景
                Background = new ImageBrush(_bmpSource);
            }

            //设置完背景之后要把遮罩层弹出来
            _maskingCanvas = new MaskingCanvas() {MaskingCanvasOwner = this};
            Grid grid=new Grid();
            grid.Children.Add(_maskingCanvas);
            Content = grid;        
        }

        /// <summary>
        /// 窗体加载完毕执行事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenImageUI_Loaded(object sender, RoutedEventArgs e)
        {
            var ctrlHotKey = (uint)(KeyFlags.MOD_NONE);
            _wndHelper = new WindowInteropHelper(this);
            if (!HotKey.RegisterHotKey(_wndHelper.Handle, 99, ctrlHotKey, Keys.Escape))
            {
                MessageBoxDiy.Show("提示", "热键已被注册，已不能使用!\r\n请点击按钮进行操作!");
            }
        }

        private void GlobalKeyProcess()
        {
            DialogResult = true;
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        /// <summary>
        /// 监视Windows消息
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wparam"></param>
        /// <param name="lparam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (wparam.ToInt32() == 99)
            {
                GlobalKeyProcess();
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 窗体关闭时执行的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenImageUI_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HotKey.UnregisterHotKey(_wndHelper.Handle, 99);
        }


        /// <summary>
        /// 进行截图操作
        /// </summary>
        /// <param name="selectedRegion">截图选中的区域</param>
        /// <param name="listRectangleRects">添加的矩形位置</param>
        /// <param name="listEllipseRects">添加的椭圆位置</param>
        /// <param name="listTextRects">添加的文字位置</param>        
        /// <param name="listArrowLineRects">添加的箭头直线位置</param>
        /// /// <param name="type">类型</param>
        public void SnapshotClipToBoard(Rect selectedRegion,List<Rect> listRectangleRects,List<Rect> listEllipseRects,List<RectToTextParameter> listTextRects, List<Arrow> listArrowLineRects,int type)
        {
            if (!selectedRegion.IsEmpty)
            {
                var rtbitmap = new RenderTargetBitmap(_bmpSource.PixelWidth, _bmpSource.PixelHeight, _bmpSource.DpiX,
                _bmpSource.DpiY, PixelFormats.Default);
                Pen pen = new Pen();
                pen.Thickness = 2;
                pen.Brush = new SolidColorBrush(Colors.Red);
                //使用DrawingVisual和DrawingContext进行绘画
                var drawingVisual = new DrawingVisual();
                using (var dc = drawingVisual.RenderOpen())
                {
                    //输出图像
                    dc.DrawImage(_bmpSource, new Rect(0, 0, _bmpSource.Width, _bmpSource.Height));
                    //输出添加的矩形
                    foreach (Rect rect in listRectangleRects)
                    {
                        dc.DrawRectangle(new SolidColorBrush(Colors.Transparent), pen, rect);
                    }
                    //输出添加的椭圆
                    foreach (Rect ellipseTect in listEllipseRects)
                    {
                        dc.DrawEllipse(new SolidColorBrush(Colors.Transparent), pen,
                            new Point((ellipseTect.Left + ellipseTect.Right) / 2, (ellipseTect.Top + ellipseTect.Bottom) / 2),
                            (ellipseTect.Right - ellipseTect.Left) / 2, (ellipseTect.Bottom - ellipseTect.Top) / 2);
                    }
                    //输出添加的文字
                    foreach (RectToTextParameter rectToTextParameter in listTextRects)
                    {
                        var text = new FormattedText(rectToTextParameter.Text, System.Globalization.CultureInfo.CurrentUICulture,
                            System.Windows.FlowDirection.LeftToRight,
                            new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            12, Brushes.Red);
                        text.MaxTextHeight = rectToTextParameter.Rect.Height;
                        text.MaxTextWidth = rectToTextParameter.Rect.Width;
                        dc.DrawText(text, new Point(rectToTextParameter.Rect.X, rectToTextParameter.Rect.Y));
                    }
                    //输出添加的箭头直线
                    foreach (Arrow arrow in listArrowLineRects)
                    {
                        dc.DrawGeometry(new SolidColorBrush(Colors.Transparent),new Pen(Brushes.Red ,3), arrow.ArrowLineGeometry);
                    }
                }
                //调用RenderTargetBitmap的Render方法，并传入刚才创建的drawingVisual对象
                rtbitmap.Render(drawingVisual);

                //截图完毕，放到剪贴板当中
                Bitmap catchedBmp = new Bitmap((int)selectedRegion.Width, (int)selectedRegion.Height);
                Graphics g = Graphics.FromImage(catchedBmp);
                //将RenderTransform转变成Bitmap
                g.DrawImage(rtbitmap.RenderTargetBitmapToBitmap(),
                    new Rectangle(0, 0, (int) selectedRegion.Width, (int) selectedRegion.Height),
                    new Rectangle((int) selectedRegion.X, (int) selectedRegion.Y,
                        (int) selectedRegion.Width, (int) selectedRegion.Height), GraphicsUnit.Pixel);

                //再将Bitmap转变成BitmapSource，并放入剪切板
                BitmapSource bs = catchedBmp.BitmapToBitmapSource();
                //粘贴到剪切板
                if (type == 1)
                {
                    Clipboard.SetImage(bs);
                }
                //保存为文件
                else if (type == 2)
                {
                    SaveFileDialog saveFileDialog=new SaveFileDialog();
                    saveFileDialog.Title = "图片保存";
                    //设置文件类型
                    saveFileDialog.Filter = "png|*.png|jpg|*.jpg|jpeg|*.jpeg|bmp|*.bmp|gif|*.gif";
                    //设置默认文件类型显示顺序
                    saveFileDialog.FilterIndex = 1;
                    //保存对话框是否记忆上次打开的目录
                    saveFileDialog.RestoreDirectory = true;
                    //文件的默认名字
                    saveFileDialog.FileName = "Helius"+DateTime.Now.ToString("yyyyMMddHHmmss")+".png";
                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string fileName = saveFileDialog.FileName;
                        int index = fileName.LastIndexOf('.');
                        //获取文件格式
                        string extion = fileName.Substring(index + 1, fileName.Length - index - 1);
                        extion = extion.ToLower();
                        ImageFormat imageFormat = ImageFormat.Png;
                        switch (extion)
                        {
                            case "jpg":
                            case "jepg":
                                imageFormat = ImageFormat.Jpeg;
                                break;
                            case "png":
                                imageFormat = ImageFormat.Png;
                                break;
                            case "bmp":
                                imageFormat = ImageFormat.Bmp;
                                break;
                            case "gif":
                                imageFormat = ImageFormat.Gif;
                                break;
                        }
                        //保存
                        catchedBmp.Save(saveFileDialog.FileName, imageFormat);
                    }
                    else
                    {
                        return;
                    }
                }
                g.Dispose();
                catchedBmp.Dispose();
                DialogResult = true;
                Close();
            }
        }
    }
}