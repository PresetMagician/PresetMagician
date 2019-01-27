using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Jacobi.Vst.Core.Host;
using Microsoft.DwayneNeed.Win32;
using Microsoft.DwayneNeed.Win32.User32;
using Host = PresetMagician.VstHost.VST.VstHost;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace PresetMagician.VstHost.Util
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class ScreenCapture
    {
        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            var handle = new HWND(hwnd);
            RECT rc = new RECT();
            NativeMethods.GetWindowRect(handle, ref rc);
            
            int width = rc.right - rc.left;
            int height = rc.bottom - rc.top;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            NativeMethods.PrintWindow(handle, hdcBitmap, 0);

            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();

            return bmp;
        }
    }
}