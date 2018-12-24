using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Windows;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Models;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            /*var vstHost = new VstHost();
            var plugin = new Plugin();
            plugin.DllPath = @"C:\Program Files\VSTPlugins\Zebralette(x64).dll";
            
            vstHost.LoadVST(plugin);
            plugin.DeterminatePresetParser();
            plugin.PresetParser.ScanBanks();

            var exporter = new VstPluginExport(vstHost);

            foreach (var i in plugin.PresetParser.Presets)
            {
                exporter.ExportPresetAudioPreviewRealtime(plugin, i);
            }

            vstHost.UnloadVST(plugin);*/

        }
    }
}