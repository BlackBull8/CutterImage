using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CutterLogical.EventArgsDefinition;
using CutterLogical.UserControls;

namespace CutterLogical
{
    public class MaskingCanvas : Canvas
    {
        #region 字段
        public ScreenImageUI MaskingCanvasOwner { get; set; }
        //private bool _isMaskDraging;
        //判断截图是否结束
        private bool _catchFinished;
        //判断截图是否开始
        private bool _catchStart;
        //判断是否开始移动
        private bool _startMove;
        //判断是否结束移动
        private bool _endMove;
        //开始坐标
        private Point? _selectedStartPoint;
        //结束坐标
        private Point? _selectedEndPoint;
        //选择区域
        private Rect _selectedRegion = Rect.Empty;
        //private SelectedRegionElement _selectedRegion =new SelectedRegionElement();

        //操作窗体
        private PopupControl _operationWindow;

        //遮罩层
        private readonly Rectangle _maskRectangleTop = new Rectangle();
        private readonly Rectangle _maskRectangleBottom = new Rectangle();
        private readonly Rectangle _maskRectangleLeft = new Rectangle();
        private readonly Rectangle _maskRectangleRight = new Rectangle();
        //选择框
        private Rectangle _selectingRectangle;

        private readonly Brush _maskRectangleBackground = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
        private readonly Brush _selectingRectangleBackground = new SolidColorBrush(Color.FromArgb(255, 49, 106, 196));

        //添加红色选择框的标志
        private string _operation = "";
        private bool _isDraw;

        //添加矩形所需要的字段
        private Rect _drawRect = Rect.Empty;
        private Rectangle _drawRectangle;
        private List<Rect> _listRects=new List<Rect>();
        private List<Rectangle> _listRectangles=new List<Rectangle>();

        //添加椭圆所需要的字段
        private Ellipse _drawEllipse;
        private List<Rect> _listRectEllipses=new List<Rect>();
        private List<Ellipse> _listEllipses = new List<Ellipse>();
        #endregion


        #region 构造函数
        public MaskingCanvas()
        {
            Loaded += MaskingCanvas_Loaded;
        }
        #endregion


        #region Loaded加载函数
        /// <summary>
        /// 画布加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaskingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            //todo:将鼠标换成彩色图标
            Background = Brushes.Transparent;
            //遮罩层的矩形进行颜色赋值
            _maskRectangleTop.Fill = _maskRectangleBackground;
            _maskRectangleBottom.Fill = _maskRectangleBackground;
            _maskRectangleLeft.Fill = _maskRectangleBackground;
            _maskRectangleRight.Fill = _maskRectangleBackground;

            //放在MaskingCanvas上
            SetLeft(_maskRectangleLeft, 0);
            SetTop(_maskRectangleTop, 0);
            SetRight(_maskRectangleRight, 0);
            SetBottom(_maskRectangleBottom, 0);
            _maskRectangleLeft.Height = ActualHeight;

            Children.Add(_maskRectangleLeft);
            Children.Add(_maskRectangleRight);
            Children.Add(_maskRectangleTop);
            Children.Add(_maskRectangleBottom);

            //一直执行刷新界面
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            //将生成的操作窗体放入到MaskingCanvas窗体中
            if (_operationWindow == null)
            {
                _operationWindow = new PopupControl() { Width = 190, Height = 30 };
                _operationWindow.Visibility = Visibility.Collapsed;
                _operationWindow.StartOperationEvent += _operationWindow_StartOperationEvent;
                _operationWindow.CancelOperationEvent += _operationWindow_CancelOperationEvent;
                Grid grid = MaskingCanvasOwner.Content as Grid;
                ((grid.Children[0]) as MaskingCanvas).Children.Add(_operationWindow);
            }



        }

        private void _operationWindow_StartOperationEvent(object sender, string operation)
        {
            _operation = operation;
        }


        private void _operationWindow_CancelOperationEvent(object sender, string operation)
        {
            _operation = "None";
        }

        #endregion


        #region 刷新界面函数
        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //设置选择框的位置
            //UpdateSelectingRectangleLayout();
            //更新整个界面的布局
            UpdateMaskRectangleLayout();
        }

        /// <summary>
        /// 更新整个界面的布局
        /// </summary>
        private void UpdateMaskRectangleLayout()
        {
            double actualHeight = ActualHeight;
            double actualWidth = ActualWidth;


            //如果还没开始进行选择，就先把_maskRectangleLeft覆盖全屏幕
            if (_selectedRegion.IsEmpty)
            {
                SetLeft(_maskRectangleLeft, 0);
                SetTop(_maskRectangleLeft, 0);
                _maskRectangleLeft.Width = actualWidth;
                _maskRectangleLeft.Height = actualHeight;

                _maskRectangleTop.Width =
                    _maskRectangleTop.Height =
                        _maskRectangleBottom.Height =
                            _maskRectangleBottom.Width = _maskRectangleRight.Height = _maskRectangleRight.Width = 0;
            }
            //如果已经有开始进行选择，对四个矩形进行位置设定
            else
            {
                double temp = _selectedRegion.Left;
                if (_maskRectangleLeft.Width != temp)
                {
                    //以选择的区域为主，如果左边的遮罩矩形不等于选择框X轴的坐标时
                    _maskRectangleLeft.Width = temp < 0 ? 0 : temp;
                }
                temp = ActualWidth - _selectedRegion.Right;
                if (_maskRectangleRight.Width != temp)
                {
                    _maskRectangleRight.Width = temp < 0 ? 0 : temp;
                }
                if (_maskRectangleRight.Height != actualHeight)
                {
                    _maskRectangleRight.Height = actualHeight;
                }

                SetLeft(_maskRectangleTop, _maskRectangleLeft.Width);
                SetLeft(_maskRectangleBottom, _maskRectangleLeft.Width);
                temp = actualWidth - _maskRectangleLeft.Width - _maskRectangleRight.Width;
                if (_maskRectangleTop.Width != temp)
                {
                    _maskRectangleTop.Width = temp < 0 ? 0 : temp;
                }
                temp = _selectedRegion.Top;
                if (_maskRectangleTop.Height != temp)
                {
                    _maskRectangleTop.Height = temp < 0 ? 0 : temp;
                }

                _maskRectangleBottom.Width = _maskRectangleTop.Width;
                temp = actualHeight - _selectedRegion.Bottom;
                if (_maskRectangleBottom.Height != temp)
                {
                    _maskRectangleBottom.Height = temp < 0 ? 0 : temp;
                }
            }
        }
        #endregion


        #region override事件

        #region 鼠标左键按下事件
        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //判断鼠标是否点击在定义的四个矩形上面，如果符合，就把选择框弹出，把画布锁住，并得到初始点，最后设置截图开始
            if (IsMouseOnThis(e)&&_operation=="")
            {
                if (!this.Children.Contains(_selectingRectangle))
                {
                    _selectingRectangle = new Rectangle();
                    //选择框的Border颜色与厚度
                    _selectingRectangle.Stroke = _selectingRectangleBackground;
                    _selectingRectangle.StrokeThickness = 2.0;
                    Children.Add(_selectingRectangle);
                }
                //StartToShowMask(e.GetPosition(this));
                _selectedStartPoint = new Point?(e.GetPosition(this));
                if (!IsMouseCaptured)
                {
                    CaptureMouse();
                }
                _catchStart = true;
                if (_operationWindow.Visibility == Visibility.Visible)
                {
                    _operationWindow.Visibility = Visibility.Collapsed;
                }


            }
            //如果点击在画布上面，就代表是移动
            else if (e.Source.Equals(this))
            {
                if (e.ClickCount >= 2)
                {
                    Console.WriteLine("点击了两次");
                    //todo:进行截图
                    if (MaskingCanvasOwner != null)
                    {
                        MaskingCanvasOwner.SnapshotClipToBoard(_selectedRegion);
                    }
                }
                else
                {
                    if (_operation == "")
                    {
                        _startMove = true;
                    }
                    else
                    {
                        _isDraw = true;
                        if (_operation == "Rectangle")
                        {
                            _drawRectangle = new Rectangle();
                            _drawRectangle.Stroke = new SolidColorBrush(Colors.Red);
                            _drawRectangle.StrokeThickness = 2;
                            //_drawRectangle.Visibility = Visibility.Collapsed;
                            Children.Add(_drawRectangle);
                            _listRectangles.Add(_drawRectangle);
                        }
                        else if (_operation == "Ellipse")
                        {
                            _drawEllipse=new Ellipse();
                            _drawEllipse.Stroke=new SolidColorBrush(Colors.Red);
                            _drawEllipse.StrokeThickness = 2;
                            Children.Add(_drawEllipse);
                            _listEllipses.Add(_drawEllipse);
                        }
                    }
                    Console.WriteLine("点击了1次");
                    _selectedStartPoint = e.GetPosition(this);
                    if (!IsMouseCaptured)
                    {
                        CaptureMouse();
                    }
                }
            }
            base.OnMouseLeftButtonDown(e);
        }

        //判断鼠标是否点在定义的四个矩形上面
        private bool IsMouseOnThis(RoutedEventArgs e)
        {
            return e.Source.Equals(_maskRectangleLeft) || e.Source.Equals(_maskRectangleRight) ||
                   e.Source.Equals(_maskRectangleTop) || e.Source.Equals(_maskRectangleBottom);
        }
        #endregion


        #region 鼠标移动事件
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Source.Equals(this) && _catchFinished && (_operation == ""|| _operation=="None"))
            {
                Cursor = Cursors.Hand;
            }
            else if (e.Source.Equals(this) && _catchFinished && _operation != "")
            {
                Cursor = Cursors.Cross;
            }
            else if (IsMouseOnThis(e) || e.Source.Equals(_operationWindow))
            {
                Cursor = Cursors.Arrow;
            }


            if (e.Source.Equals(this) && _catchStart)
            {
                UpdateSelectedRegion(e);
                e.Handled = true;
            }
            else if (e.Source.Equals(this) && _startMove)
            {
                //根据鼠标得到的_selectedStartPoint，并与鼠标移动的结果坐标进行计算，判断位移
                MoveSelectedRegion(e);
                e.Handled = true;
            }
            else if (e.Source.Equals(this) && _isDraw && _operation!="None")
            {
                DrawShape(e);
                e.Handled = true;
            }
            base.OnMouseMove(e);
        }

        private void DrawShape(MouseEventArgs e)
        {
            if (_selectedStartPoint.HasValue)
            {
                _selectedEndPoint = e.GetPosition(this);
                Point startPoint = (Point)_selectedEndPoint;
                if (startPoint.X > _selectedRegion.X + _selectedRegion.Width)
                {
                    startPoint.X = _selectedRegion.X + _selectedRegion.Width-2;
                }
                else if (startPoint.X < _selectedRegion.X)
                {
                    startPoint.X = _selectedRegion.X-2;
                }

                if (startPoint.Y > _selectedRegion.Y + _selectedRegion.Height)
                {
                    startPoint.Y = _selectedRegion.Y + _selectedRegion.Height-2;
                }
                else if (startPoint.Y < _selectedRegion.Y)
                {
                    startPoint.Y = _selectedRegion.Y-2;
                }

                Point endPoint = (Point)_selectedStartPoint;

                double startX = startPoint.X;
                double startY = startPoint.Y;
                double endX = endPoint.X;
                double endY = endPoint.Y;
                double width = endX - startX;
                double height = endY - startY;

                if (Math.Abs(width) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(height) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    double x = startX < endX ? startX : endX;
                    double y = startY < endY ? startY : endY;
                    double w = width < 0 ? -width : width;
                    double h = height < 0 ? -height : height;
                    _drawRect = new Rect(x, y, w, h);
                    if (_operation == "Rectangle")
                    {
                       
                        _listRects.Add(_drawRect);

                        _drawRectangle.Width = w;
                        _drawRectangle.Height = h;
                        SetLeft(_drawRectangle, x);
                        SetTop(_drawRectangle, y);
                        //_drawRectangle.Visibility = Visibility.Visible;
                    }
                    else if (_operation == "Ellipse")
                    {
                        //Console.WriteLine("开始画椭圆");
                        _listRectEllipses.Add(_drawRect);
                        _drawEllipse.Width = w;
                        _drawEllipse.Height = h;
                        SetLeft(_drawEllipse,x);
                        SetTop(_drawEllipse,y);
                    }
                }
            }
            else
            {
                MessageBox.Show("_selectedStartPoint点为空");
            }
        }

        /// <summary>
        /// 移动——更新选择区域
        /// </summary>
        /// <param name="e"></param>
        private void MoveSelectedRegion(MouseEventArgs e)
        {
            if (_selectedStartPoint.HasValue)
            {
                _selectedEndPoint = e.GetPosition(this);
                Point startPoint = (Point)_selectedStartPoint;
                Point endPoint = (Point)_selectedEndPoint;

                double disX = endPoint.X - startPoint.X;
                double disY = endPoint.Y - startPoint.Y;
                double x = _selectedRegion.Left + disX;
                double y = _selectedRegion.Top + disY;
                _selectedRegion = new Rect(x, y, _selectedRegion.Width, _selectedRegion.Height);
                SetLeft(_selectingRectangle, x);
                SetTop(_selectingRectangle, y);
                _selectedStartPoint = _selectedEndPoint;
                SetTop(_operationWindow, _selectedRegion.Bottom);
                SetLeft(_operationWindow, _selectedRegion.Right - _operationWindow.Width);

            }

        }

        /// <summary>
        /// 刚开始拉伸——更新选择区域
        /// </summary>
        /// <param name="e"></param>
        private void UpdateSelectedRegion(MouseEventArgs e)
        {
            if (_selectedStartPoint.HasValue)
            {
                _selectedEndPoint = e.GetPosition(this);

                Point startPoint = (Point)_selectedEndPoint;
                Point endPoint = (Point)_selectedStartPoint;

                double startX = startPoint.X;
                double startY = startPoint.Y;
                double endX = endPoint.X;
                double endY = endPoint.Y;

                double width = endX - startX;
                double height = endY - startY;

                if (Math.Abs(width) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(height) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    //_isMaskDraging = true;

                    double x = startX < endX ? startX : endX;
                    double y = startY < endY ? startY : endY;
                    double w = width < 0 ? -width : width;
                    double h = height < 0 ? -height : height;
                    _selectedRegion = new Rect(x, y, w, h);

                    _selectingRectangle.Width = w;
                    _selectingRectangle.Height = h;
                    SetLeft(_selectingRectangle, x);
                    SetTop(_selectingRectangle, y);
                }
            }
            else
            {
                MessageBox.Show("_selectedStartPoint点为空");
            }
        }
        #endregion


        #region 鼠标左键弹起事件
        /// <summary>
        /// 鼠标左键弹起事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //如果是截图弹起
            if (e.Source.Equals(this) && _catchStart)
            {
                _catchStart = false;
                _catchFinished = true;
                FinishShowMask();
            }
            //移动弹起
            else if (e.Source.Equals(this) && _startMove)
            {
                if (IsMouseCaptured)
                {
                    ReleaseMouseCapture();
                }
                _startMove = false;
                _endMove = true;
                FinishMoveMask();

            }
            else if (e.Source.Equals(this) && _isDraw)
            {
                if (IsMouseCaptured)
                {
                    ReleaseMouseCapture();
                }
                _isDraw = false;
            }
            base.OnMouseLeftButtonUp(e);
        }

        /// <summary>
        /// 选定区域结束事件
        /// </summary>
        private void FinishShowMask()
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            if (_catchFinished)
            {
                //给选择框添加一个Adorner用来拉伸
                MyCanvasAdorner myCanvasAdorner = new MyCanvasAdorner(_selectingRectangle);
                myCanvasAdorner.HoriEventHandler += MyCanvasAdorner_HoriEventHandler;
                myCanvasAdorner.VerticEventHandler += MyCanvasAdorner_VerticEventHandler;
                var layer = AdornerLayer.GetAdornerLayer(this);
                layer.Add(myCanvasAdorner);
               
                //操作窗体的显示位置
                _operationWindow.Visibility = Visibility.Visible;
                SetTop(_operationWindow, _selectedRegion.Bottom);
                SetLeft(_operationWindow, _selectedRegion.Right - _operationWindow.Width);
            }
        }

        /// <summary>
        /// 移动结束事件
        /// </summary>
        private void FinishMoveMask()
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
            if (_endMove)
            {
                //ClearSelectionData();
            }
        }
        #endregion


        #region 鼠标右键弹起事件
        /// <summary>
        /// 鼠标右键弹起事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            //分成三种情况
            //1.一种情况是没还没有开始截图，直接退出
            //2.已经开始截图，鼠标右键点击的落点是在四个自定义的矩形上面，则表示把重新截图
            //3.已经开始截图，鼠标邮件点击的落点是在选择框之内，则弹出菜单让用户选择操作
            if (_selectedRegion.IsEmpty)
            {
                MaskingCanvasOwner.DialogResult = true;
                MaskingCanvasOwner.Close();
            }
            else if (!_selectedRegion.IsEmpty && IsMouseOnThis(e))
            {
                ClearCanvasData();
            }
            else if (!_selectedRegion.IsEmpty && e.Source.Equals(this))
            {
                //todo:右键菜单弹出
                Console.WriteLine("弹出菜单");
            }           
            base.OnMouseRightButtonUp(e);
        }

        //删除画布上的所有元素
        private void ClearCanvasData()
        {
            _selectedRegion = Rect.Empty;
            Children.Remove(_selectingRectangle);
            //_selectingRectangle = null;
            foreach (Rectangle rectangle in _listRectangles)
            {
                Children.Remove(rectangle);
            }
            _listRectangles.Clear();
            _listRects.Clear();
            foreach (Ellipse ellipse in _listEllipses)
            {
                Children.Remove(ellipse);
            }
            _listEllipses.Clear();
            _listRectEllipses.Clear();
            _operationWindow.DrawEllipseTbn.IsChecked =
                _operationWindow.DrawRectangleTbn.IsChecked = _operationWindow.DrawTextTbn.IsChecked = false;
            _operation = "";
            _catchFinished = false;
            _selectedStartPoint = null;
            _selectedEndPoint = null;
            _operationWindow.Visibility = Visibility.Collapsed;
        }

        #endregion


        #region MyCanvasAdorner事件（更新选择区域）
        private void MyCanvasAdorner_VerticEventHandler(object sender, VerticalAlignmentEventArgs e)
        {
            if (e.VerticalType == VerticalAlignment.Bottom)
            {
                _selectedRegion = new Rect(_selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width, _selectedRegion.Height + e.Dist);
            }
            else if (e.VerticalType == VerticalAlignment.Top)
            {
                _selectedRegion = new Rect(_selectedRegion.X, _selectedRegion.Y + e.Dist, _selectedRegion.Width, _selectedRegion.Height - e.Dist);
            }
            SetTop(_operationWindow, _selectedRegion.Bottom);
            
        }

        private void MyCanvasAdorner_HoriEventHandler(object sender, HorizontalAlignmentEventArgs e)
        {
            if (e.HorizontalType == HorizontalAlignment.Right)
            {
                _selectedRegion = new Rect(_selectedRegion.X, _selectedRegion.Y, _selectedRegion.Width + e.Dist, _selectedRegion.Height);
            }
            else if (e.HorizontalType == HorizontalAlignment.Left)
            {
                _selectedRegion = new Rect(_selectedRegion.X + e.Dist, _selectedRegion.Y, _selectedRegion.Width - e.Dist, _selectedRegion.Height);
            }
            SetLeft(_operationWindow, _selectedRegion.Right-_operationWindow.ActualWidth);
        }
        #endregion

        #endregion
    }
}