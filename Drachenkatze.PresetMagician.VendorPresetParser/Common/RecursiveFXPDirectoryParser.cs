using System.Collections.ObjectModel;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class RecursiveFXPDirectoryParser: RecursiveBankDirectoryParser
    {
        public RecursiveFXPDirectoryParser(IVstPlugin vstPlugin, string extension, ObservableCollection<Preset> presets) : base(vstPlugin, extension, presets)
        {
        }

        protected override void ProcessFile(string fileName, Preset preset)
        {
            var fxp = new FXP();
            fxp.ReadFile(fileName);

            preset.PresetData = fxp.ChunkDataByteArray;
        }
    }
}