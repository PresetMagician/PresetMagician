using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_VBCRack : SlateDigitalPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1447183211};

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\Virtual Buss Compressors Rack\Presets");
        }

        protected override string PresetSectionName { get; } = "VBCk";
    }
}