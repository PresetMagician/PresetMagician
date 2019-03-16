using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ceras;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagicianScratchPad
{
   
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            
            
            var dp = new DataPersisterService();
            sw.Start();
            dp.Load();
            Console.WriteLine("Load: "+sw.ElapsedMilliseconds+"ms");
            sw.Restart();
            
            Type.GlobalTypes.BeginEdit();
            Characteristic.GlobalCharacteristics.BeginEdit();
            
            foreach (var plugin in dp.Plugins)
            {
                plugin.BeginEdit();
            }
            Console.WriteLine("BeginEdit: "+sw.ElapsedMilliseconds+"ms");
            
            sw.Restart();
            
            Type.GlobalTypes.EndEdit();
            Characteristic.GlobalCharacteristics.EndEdit();
            
            foreach (var plugin in dp.Plugins)
            {
                plugin.EndEdit();
            }
            Console.WriteLine("CancelEdit: "+sw.ElapsedMilliseconds+"ms");
        }
    }
}