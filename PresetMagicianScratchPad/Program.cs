using Drachenkatze.PresetMagician.VSTHost.VST;

namespace PresetMagicianScratchPad
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var vstPlugin = new VSTPlugin(@"C:\Users\Drachenkatze\Documents\SafeVSTPlugins\TAL-Elek7ro-II.dll");
            var vstHost = new VstHost();

            vstHost.LoadVST(vstPlugin);
            int numPresets = vstPlugin.NumPresets;

            for (int i = 0; i < vstPlugin.NumPresets; i++)
            {
                var preset = vstPlugin.getPreset(i);
                vstHost.pluginExporter.ExportPresetAudioPreviewRealtime(vstPlugin, preset);
            }

            vstHost.UnloadVST(vstPlugin);
        }
    }
}