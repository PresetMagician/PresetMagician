using System;
using System.Diagnostics;
using Catel.Collections;
using Catel.IoC;
using PresetMagician;
using PresetMagician.Core.Services;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            FrontendInitializer.RegisterTypes(ServiceLocator.Default);
            var sw = new Stopwatch();


            var dp = ServiceLocator.Default.ResolveType<DataPersisterService>();
            sw.Start();
            dp.Load();
            Console.WriteLine("Load: " + sw.ElapsedMilliseconds + "ms");

            var gs = ServiceLocator.Default.ResolveType<GlobalService>();
            var cnt = gs.GlobalCharacteristics.Count;
            var rnd = new Random();
            var totalAdded = 0;


            sw.Restart();
            var cs = ServiceLocator.Default.ResolveType<CharacteristicsService>();

            cs.UpdateCharacteristicsUsages();
            Console.WriteLine("Get usages: " + sw.ElapsedMilliseconds + "ms");
            Console.WriteLine("characteristics usage count: " + cs.CharacteristicUsages.Count);
            Console.WriteLine("characteristics count: " + gs.GlobalCharacteristics.Count);

            cs.CharacteristicUsages.RemoveFirst();
            Console.WriteLine("characteristics usage count: " + cs.CharacteristicUsages.Count);
            Console.WriteLine("characteristics count: " + gs.GlobalCharacteristics.Count);
        }
    }
}