using System.Collections.ObjectModel;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class RecursiveFXPDirectoryParser: RecursiveBankDirectoryParser
    {
        public RecursiveFXPDirectoryParser(Plugin plugin, string extension, ObservableCollection<Preset> presets) : base(plugin, extension, presets)
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