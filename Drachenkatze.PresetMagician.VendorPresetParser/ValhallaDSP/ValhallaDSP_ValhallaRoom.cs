using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ValhallaDSP_ValhallaRoom : ValhallaDSP, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1383429485};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Valhalla DSP, LLC\ValhallaRoom\Factory Presets");
        
        public override string BankLoadingNotes { get; set; } =
            $"Factory Presets are loaded from {ParseDirectory}. Sub-folders define the bank. User presets are "+
            "currently not supported, if you know where user presets are saved please let me know";

        public void ScanBanks()
        {
            DoScan(RootBank, ParseDirectory);
        }
    }
  
}