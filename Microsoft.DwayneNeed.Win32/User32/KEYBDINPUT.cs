using System;
using System.Runtime.InteropServices;

namespace Microsoft.DwayneNeed.Win32.User32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public short wVk;
        public short wScan;
        public KEYEVENTF dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    };
}