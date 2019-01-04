using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_Pre7X : Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1350064183};


        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"Audiority\Presets\Pre X7");

            DoScan(RootBank, directory);
        }
    }
}