using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Catel.IoC;
using Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
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

        }

        public static void CompareStuff()
        {
            var content = @"C:\Program Files (x86)\AIR Music Technology\Xpand!2\Presets";
            var file = @"01 Soft Pads\01 Sweepscape.tfx";
            var parser = new TfxXpand2();
            parser.Parse(content, file);
            
            var parser2 = new TfxXpand2();
            parser2.Parse(content, @"User\01 Sweepscape.tfx");

            Debug.WriteLine("ParamCount1:" +parser.Parameters.Count);
            Debug.WriteLine("ParamCount2:" +parser2.Parameters.Count);
            var index = 0;
            foreach (var parameter in parser.Parameters)
            {
                var parameter2 = parser2.Parameters[index];

                if (Math.Round(parameter,4) != Math.Round(parameter2,4))
                {
                    Debug.WriteLine($"{index} Parameter: {parameter} Parameter2: {parameter2}");
                }

                index++;
            }
        }

        public static void TestAirMusicStuff()
        {
            var _serviceLocator = ServiceLocator.Default;

            FrontendInitializer.RegisterTypes(_serviceLocator);
            FrontendInitializer.Initialize(_serviceLocator);

            var pluginDll = @"C:\Program Files\Steinberg\VstPlugins\Xpand!2_x64.dll";
            var content = @"C:\Program Files (x86)\AIR Music Technology\Xpand!2\Presets";
            var outputFile = @"C:\Users\Drachenkatze\Desktop\output.bin";
            var hexEditor = @"C:\Users\Drachenkatze\AppData\Local\HHD Software\Hex Editor Neo\HexFrame.exe";
            var parser = new TfxXpand2();

            var remoteVstService = _serviceLocator.ResolveType<RemoteVstService>();

            var plugin = new Plugin();
            plugin.PluginLocation = new PluginLocation();
            plugin.PluginLocation.DllPath = pluginDll;

            var pluginInstance = remoteVstService.GetInteractivePluginInstance(plugin, false);

            pluginInstance.LoadPlugin().Wait();

            foreach (var file in Directory.EnumerateFiles(content, "*.tfx", SearchOption.AllDirectories))
            {
                Debug.WriteLine(file);
                var fileName = file.Replace(content + "\\", "");
                parser.Parse(content, fileName);

                Debug.WriteLine("num parameters: " + parser.Parameters.Count);

                var data = parser.GetDataToWrite();
                File.WriteAllBytes(outputFile, data);
                try
                {
                    pluginInstance.SetChunk(data, false);
                }
                catch (FaultException e)
                {
                    var processStartInfo = new ProcessStartInfo(hexEditor)
                    {
                        Arguments = outputFile + " " + "\"" + Regex.Replace(file, @"(\\+)$", @"$1$1") + "\"",
                        UseShellExecute = true
                    };


                    var proc = new Process();

                    proc.StartInfo = processStartInfo;
                    proc.Start();
                    proc.WaitForExit();
                    return;
                }
            }
        }
    }
}