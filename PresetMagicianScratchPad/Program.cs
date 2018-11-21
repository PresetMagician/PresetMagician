using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PresetMagicianScratchPad
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var vstPlugin = new VSTPlugin(@"C:\Program Files\VSTPlugins\D16 Group\PunchBox.dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);

            var preset2 = plugin.getPreset(0, true, true);
            
            Debug.WriteLine(Encoding.UTF8.GetString(preset2.PresetData.Skip(8).ToArray()));
            Debug.WriteLine(plugin.PresetSaveModeDescription);

            var type = typeof(IVendorPresetParser);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterfaces().Contains(type));


            foreach (var parser in types) { 
            IVendorPresetParser instance = (IVendorPresetParser)Activator.CreateInstance(parser);

                if (instance.SupportedPlugins.Contains(plugin.PluginID))
                {
                    instance.VstPlugin = vstPlugin;
                    foreach (var bank in instance.Banks)
                    {
                        Debug.WriteLine("Found Bank "+bank.BankName);

                        
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
                } else
                {
                    Debug.WriteLine("No");
                }
            }

            

            vstHost.UnloadVST(plugin);
        }
    }
}