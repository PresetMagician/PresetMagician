using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ImageLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ImageLine_Poizone : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1398896471};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"Image-Line\PoiZone\");

        public override string BankLoadingNotes { get; set; } =
            $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public override async Task DoScan()
        {
            var parser = new RecursiveFXPDirectoryParser(Plugin, "fxp", PresetDataStorer);
            await parser.DoScan(RootBank, ParseDirectory);
        }
    }
}