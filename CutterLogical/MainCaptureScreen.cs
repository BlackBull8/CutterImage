using System;

namespace CutterLogical
{
    public class MainCaptureScreen
    {
        public event EventHandler NotifyEventHanlder;
        public void StartToCapture()
        {
            ScreenImageUI mainCutterUI =new ScreenImageUI(this);
            mainCutterUI.Topmost = true;
            if (mainCutterUI.ShowDialog()==true)
            {
                mainCutterUI = null;
                NotifyEventHanlder?.Invoke(this, new EventArgs());
            }
        }
    }
}