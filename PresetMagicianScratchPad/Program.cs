using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ceras;
using PresetMagician.Core.Services;

namespace PresetMagicianScratchPad
{
   
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var dp = new DataPersisterService();
            dp.Load();
            
            Debug.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}