using System;
using System.Collections.Generic;
using System.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    public class Audiority_XenoVerb : Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1199076962};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"Audiority\Presets\XenoVerb");

            DoScan(factoryBank, directory);
        }
    }
}