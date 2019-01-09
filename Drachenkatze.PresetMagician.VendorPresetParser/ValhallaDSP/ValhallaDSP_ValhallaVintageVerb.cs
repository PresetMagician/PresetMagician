using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ValhallaDSP_ValhallaVintageVerb : ValhallaDSP, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1986356531};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Valhalla DSP, LLC\ValhallaVintageVerb\Presets");
        
        public override string BankLoadingNotes { get; set; } = $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public ValhallaDSP_ValhallaVintageVerb()
        {
            DefaultModes.Add("Reverb");
        }
        
        public void ScanBanks()
        {
            DoScan(RootBank, ParseDirectory);
        }
    }
  
}