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
using System.Windows.Shapes;

namespace Cutter_UI.UserControls
{
    /// <summary>
    /// MessageBoxDiy.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxDiy : Window
    {
        private MessageBoxDiy()
        {
            InitializeComponent();
        }

        public new string Title
        {
            get { return this.TbTitle.Text; }
            set { this.TbTitle.Text = value; }
        }

        public string Message
        {
            get { return this.TbMsg.Text; }
            set { this.TbMsg.Text = value; }
        }

        public static bool? Show(string title, string msg)
        {
            var msgBox=new MessageBoxDiy();
            msgBox.Title = title;
            msgBox.Message = msg;
            return msgBox.ShowDialog();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
