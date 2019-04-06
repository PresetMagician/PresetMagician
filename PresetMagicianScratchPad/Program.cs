using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Catel.IoC;
using GSF;
using PresetMagician;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Utils;
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

            AirMusicTech.ConvertFiles(AirMusicTech.TESTSETUP_VACUUMPRO);

           

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