using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ImageLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ImageLine_Poizone: AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1398896471};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"Image-Line\PoiZone\");
        
        public override string BankLoadingNotes { get; set; } = $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public void ScanBanks()
        {
            DoScan(RootBank, ParseDirectory);
        }
        
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var parser = new RecursiveFXPDirectoryParser(VstPlugin, "fxp",Presets);
            parser.DoScan(rootBank, directory);
        }
    }
}