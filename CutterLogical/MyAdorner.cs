using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CutterLogical.EventArgsDefinition;

namespace CutterLogical
{
    class MyCanvasAdorner : Adorner
    {
        const double ThumbSize = 12;
        const double MinSize = 20;
        readonly Thumb _tl, _tr, _bl, _br;       
        public event EventHandler<VerticalAlignmentEventArgs> VerticEventHandler;
        public event EventHandler<HorizontalAlignmentEventArgs> HoriEventHandler;  
        public MyCanvasAdorner(UIElement adorned)
            : base(adorned)
        {
            _tl = GetResizeThumb(Cursors.SizeNWSE, HorizontalAlignment.Left, VerticalAlignment.Top);
            _tr = GetResizeThumb(Cursors.SizeNESW, HorizontalAlignment.Right, VerticalAlignment.Top);
            _bl = GetResizeThumb(Cursors.SizeNESW, HorizontalAlignment.Left, VerticalAlignment.Bottom);
            _br = GetResizeThumb(Cursors.SizeNWSE, HorizontalAlignment.Right, VerticalAlignment.Bottom);
            _visCollec=new VisualCollection(this);
            _visCollec.Add(_tl);
            _visCollec.Add(_tr);
            _visCollec.Add(_bl);
            _visCollec.Add(_br);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double offset = ThumbSize / 2;
            Size sz = new Size(ThumbSize, ThumbSize);
            _tl.Arrange(new Rect(new Point(-offset, -offset), sz));
            _tr.Arrange(new Rect(new Point(AdornedElement.RenderSize.Width - offset, -offset), sz));
            _bl.Arrange(new Rect(new Point(-offset, AdornedElement.RenderSize.Height - offset), sz));
            _br.Arrange(
                new Rect(
                    new Point(AdornedElement.RenderSize.Width - offset, AdornedElement.RenderSize.Height - offset), sz));
            return finalSize;
        }

        private void Resize(FrameworkElement element)
        {
            if (Double.IsNaN(element.Width))
                element.Width = element.RenderSize.Width;
            if (Double.IsNaN(element.Height))
                element.Height = element.RenderSize.Height;
        }

        Thumb GetResizeThumb(Cursor cur, HorizontalAlignment hor, VerticalAlignment ver)
        {
            var thumb = new Thumb()
            {
                Background = Brushes.Red,
                Width = ThumbSize,
                Height = ThumbSize,
                HorizontalAlignment = hor,
                VerticalAlignment = ver,
                Cursor = cur,
                Template = new ControlTemplate(typeof(Thumb))
                {
                    VisualTree = GetFactory(new SolidColorBrush(Colors.Green))
                }
            };
            thumb.DragDelta += (s, e) =>
            {
                var element = AdornedElement as FrameworkElement;
                if (element == null)
                    return;
                Resize(element);

                switch (thumb.VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        if (element.Height + e.VerticalChange > MinSize)
                        {
                            element.Height += e.VerticalChange;
                            VerticalAlignmentEventArgs arg=new VerticalAlignmentEventArgs() {Dist=e.VerticalChange,VerticalType=VerticalAlignment.Bottom};
                            if(VerticEventHandler!=null)
                            VerticEventHandler(this, arg);
                        }
                        break;
                    case VerticalAlignment.Top:
                        if (element.Height - e.VerticalChange > MinSize)
                        {
                            element.Height -= e.VerticalChange;
                            Canvas.SetTop(element, Canvas.GetTop(element) + e.VerticalChange);
                            VerticalAlignmentEventArgs arg = new VerticalAlignmentEventArgs() { Dist = e.VerticalChange, VerticalType = VerticalAlignment.Top };
                            if (VerticEventHandler != null)
                                VerticEventHandler(this, arg);
                        }
                        break;
                }
                switch (thumb.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        if (element.Width - e.HorizontalChange > MinSize)
                        {
                            element.Width -= e.HorizontalChange;
                            Canvas.SetLeft(element, Canvas.GetLeft(element) + e.HorizontalChange);
                            HorizontalAlignmentEventArgs arg=new HorizontalAlignmentEventArgs() {Dist=e.HorizontalChange,HorizontalType=HorizontalAlignment.Left};
                            if (HoriEventHandler != null)
                                HoriEventHandler(this, arg);
                        }
                        break;
                    case HorizontalAlignment.Right:
                        if (element.Width + e.HorizontalChange > MinSize)
                        {
                            element.Width += e.HorizontalChange;
                            HorizontalAlignmentEventArgs arg = new HorizontalAlignmentEventArgs() { Dist = e.HorizontalChange, HorizontalType = HorizontalAlignment.Right };
                            HoriEventHandler?.Invoke(this, arg);
                        }
                        break;
                }

                e.Handled = true;
            };
            return thumb;
        }
        FrameworkElementFactory GetFactory(Brush back)
        {
            back.Opacity = 0.6;
            var fef = new FrameworkElementFactory(typeof(Ellipse));
            fef.SetValue(Ellipse.FillProperty, back);
            fef.SetValue(Ellipse.StrokeProperty, Brushes.White);
            fef.SetValue(Ellipse.StrokeThicknessProperty, (double)1);
            return fef;
        }


        #region visCollec
        private readonly VisualCollection _visCollec;
        protected override Visual GetVisualChild(int index)
        {
            return _visCollec[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _visCollec.Count;
            }
        }
        #endregion

    }
}