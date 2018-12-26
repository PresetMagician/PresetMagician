using System;
using System.Collections.Generic;
using System.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public class AudioThing_LatinPercussion : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1819293752};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\LatinPercussion");

            DoScan(factoryBank, directory);
        }
    }
}