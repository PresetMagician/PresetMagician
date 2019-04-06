using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_VBCFGRed : SlateDigitalPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1447183218};

        protected override string PresetSectionName { get; } = "VBCr";

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\Virtual Buss Compressors FG-Red\Presets");
        }
    }
}