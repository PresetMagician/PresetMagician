using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Xfer_Records
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Xfer_Records_Serum : RecursiveFXPDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1483109208};

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Xfer\Serum Presets\Presets");
        }

        protected override string Extension { get; } = "fxp";
    }
}