using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace CutterLogical.UserControls
{
    /// <summary>
    /// PopupControl.xaml 的交互逻辑
    /// </summary>
    public partial class PopupControl : UserControl
    {
        public PopupControl()
        {
            InitializeComponent();
        }

        public event EventHandler<string> StartOperationEvent;
        public event EventHandler<string> CancelOperationEvent;


        private void DrawRectangleTb_Ckecked(object sender, RoutedEventArgs e)
        {
            DrawEllipseTbn.IsChecked = false;
            DrawTextTbn.IsChecked = false;
            StartOperation("Rectangle");
        }

        private void DrawRectangleTb_UnChecked(object sender, RoutedEventArgs e)
        {
            CancelOperation("Rectangle");
        }

        
        private void DrawEllipseTb_Checked(object sender, RoutedEventArgs e)
        {
            DrawRectangleTbn.IsChecked = false;
            DrawTextTbn.IsChecked = false;
            StartOperation("Ellipse");
        }

        private void DrawEllipseTb_UnChecked(object sender, RoutedEventArgs e)
        {
           CancelOperation("Ellipse");
        }

        private void DrawText_Checked(object sender, RoutedEventArgs e)
        {
            DrawRectangleTbn.IsChecked = false;
            DrawEllipseTbn.IsChecked = false;
            StartOperation("Text");
        }

        private void DrawText_UnChecked(object sender, RoutedEventArgs e)
        {
            CancelOperation("Text");
        }

        private void StartOperation(string operation)
        {
            StartOperationEvent?.Invoke(this, operation);
        }

        private void CancelOperation(string operation)
        {
            CancelOperationEvent?.Invoke(this, operation);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
