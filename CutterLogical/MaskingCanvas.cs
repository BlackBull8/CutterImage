﻿using System;
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

        
        //遮罩层
        private readonly Rectangle _maskRectangleTop = new Rectangle();
        private readonly Rectangle _maskRectangleBottom = new Rectangle();
        private readonly Rectangle _maskRectangleLeft = new Rectangle();
        private readonly Rectangle _maskRectangleRight = new Rectangle();
        //选择框
        private Rectangle _selectingRectangle;

        private readonly Brush _maskRectangleBackground = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
        private readonly Brush _selectingRectangleBackground = new SolidColorBrush(Color.FromArgb(255, 49, 106, 196));
        public MaskingCanvas()
        {
            Loaded += MaskingCanvas_Loaded;
        }

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

            
            //Children.Add(_selectingRectangle);

            //一直执行刷新界面
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

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


        #region override事件


        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //判断鼠标是否点击在定义的四个矩形上面，如果符合，就把选择框弹出，把画布锁住，并得到初始点，最后设置截图开始
            if (IsMouseOnThis(e))
            {
                if (!this.Children.Contains(_selectingRectangle))
                {
                    _selectingRectangle=new Rectangle();
                    //选择框的Border颜色与厚度
                    _selectingRectangle.Stroke = _selectingRectangleBackground;
                    _selectingRectangle.StrokeThickness = 2.0;
                    Children.Add(_selectingRectangle);
                }
                StartToShowMask(e.GetPosition(this));
                _catchStart = true;

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
                    Console.WriteLine("点击了1次");
                    _startMove = true;
                    _selectedStartPoint = e.GetPosition(this);
                    if (!IsMouseCaptured)
                    {
                        CaptureMouse();
                    }
                }
            }
            base.OnMouseLeftButtonDown(e);
        }



        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Source.Equals(this)&&_catchFinished)
            {
                Cursor =Cursors.Hand;
            }
            else if (IsMouseOnThis(e))
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
            base.OnMouseMove(e);
        }



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
            base.OnMouseLeftButtonUp(e);
        }


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
                _selectedRegion=Rect.Empty;               
                Children.Remove(_selectingRectangle);
                _selectingRectangle = null;
                ClearSelectionData();
            }
            else if (!_selectedRegion.IsEmpty && e.Source.Equals(this))
            {
                //todo:右键菜单弹出
                Console.WriteLine("弹出菜单");
            }
            base.OnMouseRightButtonUp(e);
        }


        //判断鼠标是否点在定义的四个矩形上面
        private bool IsMouseOnThis(RoutedEventArgs e)
        {
            return e.Source.Equals(_maskRectangleLeft) || e.Source.Equals(_maskRectangleRight) ||
                   e.Source.Equals(_maskRectangleTop) || e.Source.Equals(_maskRectangleBottom);
        }



        /// <summary>
        /// 显示选择框
        /// </summary>
        /// <param name="position"></param>
        private void StartToShowMask(Point position)
        {
            _selectingRectangle.Visibility = Visibility.Visible;
            _selectedStartPoint = new Point?(position);
            if (!IsMouseCaptured)
            {
                CaptureMouse();
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
                else
                {
                    //Console.WriteLine("距离太小。。。。。。");
                    //todo:如果拖动的距离不被认定拖拽的话，就什么都不做。
                }
            }
            else
            {
                MessageBox.Show("_selectedStartPoint点为空");
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
                //ClearSelectionData();
                //todo:生成一个Popup窗体，里面是Menu
                MakePopUpElement();
            }
        }

        /// <summary>
        /// 生成一个Popup元素
        /// </summary>
        private void MakePopUpElement()
        {
            PopupControl popupControl=new PopupControl();
            popupControl.Width = 240;
            popupControl.Height = 30;
            SetTop(popupControl, _selectedRegion.Bottom);
            SetLeft(popupControl, _selectedRegion.Right -popupControl.Width);
            Grid grid = MaskingCanvasOwner.Content as Grid;
            ((grid.Children[0]) as MaskingCanvas).Children.Add(popupControl);

        }


        /// <summary>
        /// 设置初始点为null
        /// </summary>
        private void ClearSelectionData()
        {
            _selectedStartPoint = null;
            _selectedEndPoint = null;
        }


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
        }
        #endregion


        #endregion


    }

    #region 无用代码
    //private void UpdateSelectingRectangleLayout()
    //{
    //    if (_selectedRegion.Init)//!_selectedRegion.IsEmpty)
    //    {
    //        SetLeft(_selectingRectangle, _selectedRegion.Left);
    //        SetTop(_selectingRectangle,_selectedRegion.Top);
    //        _selectingRectangle.Width = _selectedRegion.Width;
    //        _selectingRectangle.Height = _selectedRegion.Height;
    //    }
    //}


    //internal class SelectedRegionElement : UIElement
    //{
    //    public double Left { get; set; }
    //    public double Top { get; set; }
    //    public double Width { get; set; }
    //    public double Height { get; set; }

    //    public double Right
    //    {
    //        get { return Left + Width; }
    //    }

    //    public double Bottom
    //    {
    //        get { return Top + Height; }
    //    }

    //    public bool Init { get; set; }

    //    //protected override void OnRender(DrawingContext drawingContext)
    //    //{
    //    //    drawingContext.DrawRectangle(Brushes.Transparent, new System.Windows.Media.Pen(Brushes.Red, 2), new Rect(Left, Top, Width, Height));
    //    //    base.OnRender(drawingContext);
    //    //}

    //}
    #endregion


}