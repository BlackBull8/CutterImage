using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CutterLogical
{
    public static class HelperClass
    {
        /// <summary>
        /// 获取屏幕的截图
        /// </summary>
        /// <returns></returns>
        public static Bitmap GetScreenCutter()
        {
            try
            {
                //获取屏幕的大小
                Rectangle rect = SystemInformation.VirtualScreen;
                //创建bitmap对象
                Bitmap bitmap=new Bitmap(rect.Width,rect.Height,PixelFormat.Format32bppArgb);
                //创建画布
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    //在画布上截屏
                    g.CopyFromScreen(rect.X,rect.Y,0,0,rect.Size,CopyPixelOperation.SourceCopy);
                }
                return bitmap;
            }
            catch (Exception)
            {
                //throw;
            }
            return null;
        }


        /// <summary>
        /// 扩展方法，将Bitmap类型转换成BitmapSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource BitmapToBitmapSource(this Bitmap bitmap)
        {
            BitmapSource returnSource;
            try
            {
                //进行转换
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception)
            {
                returnSource = null;
            }
            return returnSource;
        }

        /// <summary>
        /// 扩展方法，将RenderTargetBitmap类型转换成Bitmap
        /// </summary>
        /// <param name="renderTargetBitmap"></param>
        /// <returns></returns>
        public static Bitmap RenderTargetBitmapToBitmap(this RenderTargetBitmap renderTargetBitmap)
        {
            Bitmap returnBitmap;
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    encoder.Save(stream);
                    returnBitmap = new Bitmap(stream);
                }

            }
            catch (Exception)
            {

                returnBitmap = null;
            }
            return returnBitmap;
        }
    }
}