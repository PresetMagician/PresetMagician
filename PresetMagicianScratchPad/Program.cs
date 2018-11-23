using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Diagnostics;
using System.Text;

namespace PresetMagicianScratchPad
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var vstPlugin = new VSTPlugin(@"C:\Users\Drachenkatze\Documents\SafeVSTPlugins\Kairatune.dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);

            var handler = VendorPresetParser.GetPresetHandler(vstPlugin);

            Debug.WriteLine(handler.PresetParserType);

            /* foreach (var parser in types)
             {
                 IVendorPresetParser instance = (IVendorPresetParser)Activator.CreateInstance(parser);

                 instance.VstPlugin = vstPlugin;
                 if (instance.CanHandle())
                 {
                     foreach (var bank in instance.Banks)
                     {
                         Debug.WriteLine("Found Bank " + bank.BankName);

                         foreach (var preset in bank.VSTPresets)
                         {
                             if (bank.VSTPresets.IndexOf(preset) % 60 == 0)
                             {
                                 vstHost.UnloadVST(vstPlugin);
                                 vstHost.LoadVST(vstPlugin);
                             }
                             Debug.WriteLine("Exporting Preset " + preset.PresetName);
                             vstHost.pluginExporter.ExportPresetNKSF(vstPlugin, preset);
                             vstHost.pluginExporter.ExportPresetAudioPreviewRealtime(vstPlugin, preset);
                         }
                     }
                 }
                 else
                 {
                     Debug.WriteLine("No");
                 }
             }*/

            vstHost.UnloadVST(plugin);
        }
    }
}