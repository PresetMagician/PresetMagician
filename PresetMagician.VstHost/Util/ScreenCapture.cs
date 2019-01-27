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
using Host = PresetMagician.VstHost.VST.VstHost;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace PresetMagician.VstHost.Util
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class ScreenCapture
    {
        // ReSharper disable once InconsistentNaming
        private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;
        private static readonly Action EmptyDelegate = delegate { };

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);

        public static Bitmap CaptureVstScreenshot(IVstPluginContext pluginContext)
        {
            if (!pluginContext.PluginCommandStub.EditorGetRect(out Rectangle wndRect))
            {
                return null;
            }

            if (wndRect.Width == 0 || wndRect.Height == 0)
            {
                return null;
            }

            var window = new Window //make sure the window is invisible
            {
                Width = wndRect.Width,
                Height = wndRect.Height,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = 0,
                Top = 0,
                ResizeMode = ResizeMode.NoResize,
                Margin = new Thickness(0),
                SnapsToDevicePixels = true,
                UseLayoutRounding = false,
                SizeToContent = SizeToContent.Manual,
                Opacity = 0,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            var helper = new WindowInteropHelper(window);
            helper.EnsureHandle();


            int value = 1; // TRUE to disable
            DwmSetWindowAttribute(helper.Handle,
                DWMWA_TRANSITIONS_FORCEDISABLED,
                ref value,
                Marshal.SizeOf(value));

            window.Show();
            window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, EmptyDelegate);

            var success = pluginContext.PluginCommandStub.EditorOpen(helper.Handle);


            if (!success)
            {
                window.Visibility = Visibility.Hidden;
                window.Hide();
                window.Close();
                return null;
            }


            for (var i = 0; i < 10; i++)
            {
                pluginContext.PluginCommandStub.EditorIdle();
                window.Dispatcher
                    .BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { Thread.Sleep(10); })).Wait();
            }

            if (pluginContext.PluginCommandStub.EditorGetRect(out Rectangle wndRect2))
            {
                window.Width = wndRect2.Width;
                window.Height = wndRect2.Height;
            }

            for (var i = 0; i < 10; i++)
            {
                pluginContext.PluginCommandStub.EditorIdle();
                window.Dispatcher
                    .BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { Thread.Sleep(10); })).Wait();
            }


            var bmp = PrintWindow(helper.Handle);
            pluginContext.PluginCommandStub.EditorClose();
            window.Visibility = Visibility.Hidden;
            window.Hide();

            window.Close();
            Thread.Sleep(500);
            return bmp;
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        /// <summary>
        /// Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }


        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            User32.RECT rc = new User32.RECT();
            User32.GetWindowRect(hwnd, ref rc);

            int width = rc.right - rc.left;
            int height = rc.bottom - rc.top;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            User32.PrintWindow(hwnd, hdcBitmap, 0);

            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();

            return bmp;
        }

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_LAYERED = 0x00080000;
            public const int LWA_ALPHA = 0x00000002;

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            [DllImport("user32.dll")]
            public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

            [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
            public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);


            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
            public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll")]
            public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        }
    }
}