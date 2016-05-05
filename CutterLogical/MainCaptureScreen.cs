using System;

namespace CutterLogical
{
    public class MainCaptureScreen
    {
        public event EventHandler NotifyEventHanlder;
        public void StartToCapture()
        {
            ScreenImageUI mainCutterUI =new ScreenImageUI(this);
            if (mainCutterUI.ShowDialog()==true)
            {
                NotifyEventHanlder?.Invoke(this, new EventArgs());
            }
        }
    }
}