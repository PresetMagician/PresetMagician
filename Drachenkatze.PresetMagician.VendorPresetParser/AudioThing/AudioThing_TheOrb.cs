using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_TheOrb: AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1951355458};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"AudioThing\Presets\TheOrb");

            DoScan(RootBank, directory);
        }
    }
}