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
            dp.Load();
            
            
            sw.Start();
            Type.GlobalTypes.BeginEdit();
            Characteristic.GlobalCharacteristics.BeginEdit();
            
            foreach (var plugin in dp.Plugins)
            {
                plugin.BeginEdit();
            }
            Console.WriteLine("BeginEdit: "+sw.ElapsedMilliseconds+"ms");
            
            sw.Restart();
            
            Type.GlobalTypes.CancelEdit();
            Characteristic.GlobalCharacteristics.CancelEdit();
            
            foreach (var plugin in dp.Plugins)
            {
                plugin.CancelEdit();
            }
            Console.WriteLine("CancelEdit: "+sw.ElapsedMilliseconds+"ms");
        }
    }
}