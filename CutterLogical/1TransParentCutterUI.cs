using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace CutterLogical
{
    /// <summary>
    /// 透明窗体，背景是屏幕的静态图
    /// </summary>
    public class TransParentCutterUI:Window
    {
        //用来保存原始图像
        public Bitmap _originBmp;
        //判断截图是否结束
        private bool _catchFinished = false;
        //判断截图是否开始
        private bool _catchStart = false;
        //用来保存截图的矩形
        private Rect _catchRectangle; 
        //记录鼠标按下的坐标点
        private Point _downPoint;
        //窗体的Grid布局控件
        private Grid _gridContent;
        //圈定范围的矩形
        private RectangleElement _rectangleElementre;
        //无参构造函数
        public TransParentCutterUI()
        {
            Init();
        }

        /// <summary>
        /// 初始化程序界面函数
        /// </summary>
        private void Init()
        {
            //设置窗体的Style
            this.WindowStyle=WindowStyle.None;
            //设置窗体最大化
            this.WindowState=WindowState.Maximized;
            this.Background=new SolidColorBrush(Colors.White);
            //显示在最前面
            this.Topmost = true;
            
            //初始化Grid控件
            _gridContent = new Grid() {Name="Grid1"};
            //往Grid控件中添加Image控件
            _gridContent.Children.Add(new System.Windows.Controls.Image() {Name = "ImageScreen"});
            //添加到界面上
            this.AddChild(_gridContent);

            //添加事件
            this.Loaded += TransParentCutterUI_Loaded;
            this.MouseLeftButtonDown += TransParentCutterUI_MouseLeftButtonDown;
            this.MouseRightButtonDown += TransParentCutterUI_MouseRightButtonDown;
            this.MouseLeftButtonUp += TransParentCutterUI_MouseLeftButtonUp;
            this.MouseMove += TransParentCutterUI_MouseMove;     
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransParentCutterUI_MouseMove(object sender, MouseEventArgs e)
        {
            if (_catchStart)
            {
                try
                {
                    Point newPoint = _downPoint;
                    //计算矩形的宽度和高度
                    int width = Math.Abs((int)(e.GetPosition(this).X - _downPoint.X));
                    int height = Math.Abs((int)(e.GetPosition(this).Y - _downPoint.Y));
                    //判断鼠标初始点是否比移动到的点大，如果大，则对换
                    if (e.GetPosition(this).X < _downPoint.X)
                    {
                        newPoint.X = (int)e.GetPosition(this).X;
                    }
                    if (e.GetPosition(this).Y < _downPoint.Y)
                    {
                        newPoint.Y = (int)e.GetPosition(this).Y;
                    }

                    //给矩形对象赋值
                    _rectangleElementre.StartX = (int)newPoint.X;
                    _rectangleElementre.StartY = (int)newPoint.Y;
                    _rectangleElementre.Width = width;
                    _rectangleElementre.Height = height;
                    _rectangleElementre.InvalidateVisual();
                    _catchRectangle = new Rect(newPoint, new Size(width, height));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }

            }
        }

        private void TransParentCutterUI_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_catchStart)
            {
                _catchStart = false;
                _catchFinished = true;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void TransParentCutterUI_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransParentCutterUI_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //点击1次
            if (e.ClickCount == 1 && _rectangleElementre==null)
            {
                if (!_catchStart)
                {                    
                    _rectangleElementre = new RectangleElement();
                    this._gridContent.Children.Add(_rectangleElementre);
                    _rectangleElementre.MouseLeftButtonUp += TransParentCutterUI_MouseLeftButtonUp;
                    _catchStart = true;
                    _downPoint = new Point((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y);
                    Console.WriteLine("1");
                }
            }
            else if (e.ClickCount > 1)
            {
                bool isInRectX = e.GetPosition(this).X > _catchRectangle.X &&
                                e.GetPosition(this).X < _catchRectangle.Width + _catchRectangle.X;
                bool isInRectY = e.GetPosition(this).Y > _catchRectangle.Y &&
                                 e.GetPosition(this).Y < _catchRectangle.Height + _catchRectangle.Y;
                if (_catchFinished && isInRectX && isInRectY)
                {
                    //截图完毕，放到剪贴板当中
                    Bitmap catchedBmp = new Bitmap((int)_catchRectangle.Width, (int)_catchRectangle.Height);
                    Graphics g = Graphics.FromImage(catchedBmp);
                    g.DrawImage(_originBmp, new System.Drawing.Rectangle(0, 0, (int)_catchRectangle.Width, (int)_catchRectangle.Height), new System.Drawing.Rectangle((int)_catchRectangle.X, (int)_catchRectangle.Y, (int)_catchRectangle.Width, (int)_catchRectangle.Height), GraphicsUnit.Pixel);
                    BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(catchedBmp.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    Clipboard.SetImage(bs);
                    g.Dispose();
                    _catchFinished = false;
                    catchedBmp.Dispose();
                    this.DialogResult = true;
                    Close();
                }
            }
        }

        private void TransParentCutterUI_Loaded(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Cross;
           
        }
    }


    /// <summary>
    /// 界面显示的截图框
    /// </summary>
    internal class RectangleElement : UIElement
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Transparent, new System.Windows.Media.Pen(Brushes.Red, 2), new Rect(StartX, StartY, Width, Height));
            base.OnRender(drawingContext);
        }
    }
}
