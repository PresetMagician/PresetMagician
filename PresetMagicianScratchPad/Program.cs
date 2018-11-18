using Drachenkatze.PresetMagician.VSTHost.VST;
using System.Diagnostics;

namespace PresetMagicianScratchPad
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var vstPlugin = new VSTPlugin(@"C:\Users\Felicia Hummel\Documents\Vst64\V-Station x64.dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);
            Debug.WriteLine(plugin.PresetSaveModeDescription);
        }
    }
}