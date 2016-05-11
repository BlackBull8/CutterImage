using System.Windows;
using System.Windows.Input;

namespace CutterLogical.UserControls
{
    /// <summary>
    ///     MessageBoxDiy.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxDiy : Window
    {
        private MessageBoxDiy()
        {
            InitializeComponent();
        }

        public new string Title
        {
            get { return TbTitle.Text; }
            set { TbTitle.Text = value; }
        }

        public string Message
        {
            get { return TbMsg.Text; }
            set { TbMsg.Text = value; }
        }

        public static bool? Show(string title, string msg)
        {
            var msgBox = new MessageBoxDiy();
            msgBox.Title = title;
            msgBox.Message = msg;
            return msgBox.ShowDialog();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}