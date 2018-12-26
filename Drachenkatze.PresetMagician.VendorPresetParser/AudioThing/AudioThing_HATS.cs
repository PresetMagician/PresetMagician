using System;
using System.Collections.Generic;
using System.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public class AudioThing_HATS : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1212240979};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\Hats");

            DoScan(factoryBank, directory);
        }
    }
}