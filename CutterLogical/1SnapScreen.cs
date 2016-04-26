using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CutterLogical
{
    /// <summary>
    /// 截屏
    /// </summary>
    public class SnapScreen
    {
        public int CutterScreen()
        {
            //创建透明窗体
            TransParentCutterUI transParentCutterUi=new TransParentCutterUI();
            //寻找Image控件
            Grid grid = transParentCutterUi.Content as Grid;
            System.Windows.Controls.Image imageScreen =
                LogicalTreeHelper.FindLogicalNode(grid, "ImageScreen") as System.Windows.Controls.Image;

            //创建画的长度与宽度
            var screenBmp = new Bitmap((int)SystemParameters.PrimaryScreenWidth,
               (int)SystemParameters.PrimaryScreenHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //创建画布
            var bmpGraphics = Graphics.FromImage(screenBmp);
            //截图
            bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);

            //把Bitmap转化成ImageSource并赋值给Image控件
            var hBitmap = screenBmp.GetHbitmap();
             imageScreen.Source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);

            //把screenBmp截图赋值给界面的_originBmp变量
            transParentCutterUi._originBmp = screenBmp;

            if (transParentCutterUi.ShowDialog() == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}