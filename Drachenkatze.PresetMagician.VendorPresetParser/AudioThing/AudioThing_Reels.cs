using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_Reels : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1884582467};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"AudioThing\Presets\Reels");

            DoScan(RootBank, directory);
        }
    }
}