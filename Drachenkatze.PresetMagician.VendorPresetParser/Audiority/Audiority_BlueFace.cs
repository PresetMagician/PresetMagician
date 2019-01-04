using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_BlueFace: Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1194668409};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Audiority\Presets\Blue Face");

            DoScan(RootBank, directory);
        }
    }
}