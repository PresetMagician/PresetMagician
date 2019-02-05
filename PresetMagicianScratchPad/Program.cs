using System;
using System.Diagnostics;
using System.Threading;
using Drachenkatze.PresetMagician.Utils;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DoStuff();
            Process proc = Process.GetCurrentProcess();
            Debug.WriteLine($"Memory usage before GC: {proc.PrivateMemorySize64}");
            Thread.Sleep(500);
            GC.Collect();
            Debug.WriteLine($"Memory usage after GC: {proc.PrivateMemorySize64}");
        }

        public static void DoStuff()
        {
            var foo = new byte[512000000];
            var y = HashUtils.getIxxHash(foo);
        }
    }
}