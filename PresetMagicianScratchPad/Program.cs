using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Catel.Collections;
using Catel.IoC;
using Ceras;
using PresetMagician.Core;
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
            CoreInitializer.RegisterServices();
            var sw = new Stopwatch();
            
            
            var dp = ServiceLocator.Default.ResolveType<DataPersisterService>();
            sw.Start();
            dp.Load();
            Console.WriteLine("Load: "+sw.ElapsedMilliseconds+"ms");
            
            var gs = ServiceLocator.Default.ResolveType<GlobalService>();
            var cnt = gs.GlobalCharacteristics.Count;
            var rnd = new Random();
            var totalAdded = 0;
            
           
            sw.Restart();
           var cs = ServiceLocator.Default.ResolveType<CharacteristicsService>();
           
           cs.UpdateCharacteristicsUsages();
           Console.WriteLine("Get usages: "+sw.ElapsedMilliseconds+"ms");
           Console.WriteLine("characteristics usage count: "+cs.CharacteristicUsages.Count);
           Console.WriteLine("characteristics count: "+gs.GlobalCharacteristics.Count);
           
           cs.CharacteristicUsages.RemoveFirst();
           Console.WriteLine("characteristics usage count: "+cs.CharacteristicUsages.Count);
           Console.WriteLine("characteristics count: "+gs.GlobalCharacteristics.Count);
        }
    }
}