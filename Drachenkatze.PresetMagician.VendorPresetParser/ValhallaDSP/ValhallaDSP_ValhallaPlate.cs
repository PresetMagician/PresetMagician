using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ValhallaDSP_ValhallaPlate : ValhallaDSP, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1886151028};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Valhalla DSP, LLC\ValhallaPlate\Presets");
        
        public override string BankLoadingNotes { get; set; } = $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public void ScanBanks()
        {
            DoScan(RootBank, ParseDirectory);
        }
    }
  
}