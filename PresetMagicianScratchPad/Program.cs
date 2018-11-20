using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Diagnostics;

namespace PresetMagicianScratchPad
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var vstPlugin = new VSTPlugin(@"C:\Users\Drachenkatze\Documents\deleteme\Plex64.dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);
            Debug.WriteLine(plugin.PresetSaveModeDescription);
            vstHost.UnloadVST(plugin);
        }
    }
}