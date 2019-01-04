using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_TS1TransientShaper: Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1332836457};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Audiority\Presets\TS-1 Transient Shaper");

            DoScan(RootBank, directory);
        }
    }
}