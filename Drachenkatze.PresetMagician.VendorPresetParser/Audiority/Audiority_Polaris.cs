using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_Polaris : Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1466513515};

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"Audiority\Presets\Polaris");
        }
    }
}