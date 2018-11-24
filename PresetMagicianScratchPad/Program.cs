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
            var vstPlugin = new VSTPlugin(@"C:\Users\Felicia Hummel\Documents\TestVSTs\Repro-5(x64).dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);

            var handler = VendorPresetParser.GetPresetHandler(vstPlugin);

            handler.ScanBanks();

            foreach (var bank in handler.Banks)
            {
                foreach (var preset in bank.VSTPresets)
                {
                    vstHost.pluginExporter.ExportPresetAudioPreviewRealtime(vstPlugin, preset);
                    vstHost.pluginExporter.ExportPresetNKSF(vstPlugin, preset);
                }
            }
            //

            vstHost.UnloadVST(plugin);
        }
    }
}