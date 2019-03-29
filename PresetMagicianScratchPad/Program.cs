using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Catel.IoC;
using Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
using GSF;
using PresetMagician;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Utils.Logger;
using PresetMagician.VstHost.VST;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            TestAirMusicStuff();

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

        public static void TestAirMusicStuff()
        {
                var _serviceLocator = ServiceLocator.Default;

            FrontendInitializer.RegisterTypes(_serviceLocator);
            FrontendInitializer.Initialize(_serviceLocator);

            var pluginDll = @"C:\Program Files\Steinberg\VstPlugins\MiniGrand.dll";
            var content = @"C:\ProgramData\AIR Music Technology\Vacuum\Presets";
            var outputFile = @"C:\Users\Drachenkatze\Desktop\output.bin";
            var hexEditor = @"C:\Users\Drachenkatze\AppData\Local\HHD Software\Hex Editor Neo\HexFrame.exe";
            

            var remoteVstService = _serviceLocator.ResolveType<RemoteVstService>();

            var plugin = new Plugin();
            plugin.PluginLocation = new PluginLocation();
            plugin.PluginLocation.DllPath = pluginDll;

            var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

            pluginInstance.LoadPlugin().Wait();
            var x = new List<double>();
            foreach (var file in Directory.EnumerateFiles(content, "*.tfx", SearchOption.AllDirectories))
            {
                Debug.WriteLine(file);
                var parser = new TfxVacuum();
                var fileName = file.Replace(content + "\\", "");
               
                    parser.Parse(content, fileName);

                    //Debug.WriteLine("num parameters: " + parser.Parameters.Count);

                    if (parser.Parameters.Count == 627)
                    {
                        var p = Math.Round(parser.Parameters[15], 6);
                        if (!x.Contains(p))
                        {
                            Debug.WriteLine(file + " " + p);
                            x.Add(p);
                        }
                    }
                    var data = parser.GetDataToWrite();
                    File.WriteAllBytes(outputFile, data);
                    break;
                if (file == @"C:\Program Files (x86)\AIR Music Technology\Hybrid\Presets\06 Bright Pads\25 Open Up.tfx")
                {
                    return;
                }
            }

            x.Sort();
            foreach (var i in x)
            {
                Debug.WriteLine(i);
            }
        }
    }
}