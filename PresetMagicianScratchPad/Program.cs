using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Catel.IoC;
using GSF;
using PresetMagician;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Utils.Logger;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
using PresetMagician.VstHost.VST;
using PresetMagicianScratchPad.Stuff;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AirMusicTech.ConvertFiles(AirMusicTech.TESTSETUP_VELVET);
            
            
            //AirMusicTech.TestLoadInPlugin(AirMusicTech.TESTSETUP_HYBRID);

            /*var culture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            TestAirMusicStuff();*/

            //CompareStuff();

            /*var outFile = @"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets\User\foo095.tfx";

            var data = File.ReadAllBytes(@"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets\User\foo.tfx");
            var floatVal = 0.95d;
            
            using (var ms = new MemoryStream(data))
            {
                ms.Seek(0x40, SeekOrigin.Begin);

                for (var i = 0; i < 627; i++)
                {
                    var data2 = BigEndian.GetBytes(floatVal);
                    ms.Write(data2, 0, data2.Length);
                    ms.Seek(4, SeekOrigin.Current);
                }
                
                File.WriteAllBytes(outFile, ms.ToArray());
            }*/

            /*var content = @"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets";
            var file = @"User\foo10-2.tfx";
            var parser = new TfxHybrid3();
            parser.Parse(content, file);
            
            var index = 0;
            var origValue = 1.0d;
            foreach (var parameter in parser.Parameters)
            {
                if (Math.Round(parameter, 4) != Math.Round(origValue, 4))
                {
                    var change = (parameter / origValue) - 1;
                    var absDiff = parameter - origValue;
                    
                    Debug.WriteLine($"{index} Parameter (orig): {Math.Round(origValue, 4)} found: {Math.Round(parameter, 4)} Change%: {Math.Round(change,4)} changeAbs: {absDiff}");
                }


                index++;
            } */

        }
        
        

        public static void CompareStuff()
        {
            var content = @"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets";
            var file = @"05 Leads\Soft Lead 05.tfx";
            var outputFile = @"C:\Users\Drachenkatze\Desktop\output.bin";
            var parser = new TfxHybrid3();
            parser.Parse(content, file);
            
            var parser2 = new TfxHybrid3();
            parser2.Parse(content, @"User\foo234.tfx");

            Debug.WriteLine("ParamCount1:" +parser.Parameters.Count);
            Debug.WriteLine("ParamCount2:" +parser2.Parameters.Count);
            
            //File.WriteAllBytes(outputFile, parser.GetDataToWrite());
            var index = 0;
            foreach (var parameter in parser.Parameters)
            {
                var parameter2 = parser2.Parameters[index];

                var roundedValue1 = parameter.ToString("F2", 
                    CultureInfo.InvariantCulture);
                var roundedValue2 = parameter2.ToString("F2", 
                    CultureInfo.InvariantCulture);
                if (roundedValue1 != roundedValue2)
                {
                    Debug.WriteLine($"{index} Parameter (orig): {parameter} Parameter2 (user): {parameter2}");
                }

                index++;
            }
        }

        

        
    }
}