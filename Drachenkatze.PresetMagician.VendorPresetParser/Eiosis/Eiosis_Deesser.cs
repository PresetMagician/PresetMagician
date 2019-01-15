using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.SlateDigital;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Eiosis
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Eiosis_Deesser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1160922195};

        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Eiosis\E2Deesser\Presets");

            var parser = new SlateDigitalPresetParser(Plugin, "epf", Presets, "E2DS");
            parser.DoScan(RootBank, directory);
        }
    }
}