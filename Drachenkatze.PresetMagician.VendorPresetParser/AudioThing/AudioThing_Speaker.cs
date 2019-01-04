using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_Speaker : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1934650226};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"AudioThing\Presets\Speaker");

            DoScan(RootBank, directory);
        }
    }
}