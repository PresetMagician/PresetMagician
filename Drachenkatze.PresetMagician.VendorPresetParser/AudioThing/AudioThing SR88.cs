﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public class AudioThing_SR88 : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1397897272};


        public void ScanBanks()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            RootBank.PresetBanks.Add(factoryBank);

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\SR-88");

            ConfigNode = "SR-88_SETTINGS";

            DoScan(factoryBank, directory);
        }
    }
}