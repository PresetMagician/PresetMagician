using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.ValhallaDSP
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ValhallaDSP_ValhallaPlate : ValhallaDSP, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1886151028};

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Valhalla DSP, LLC\ValhallaPlate\Presets");
        }

        public override void Init()
        {
            DefaultModes.Add("Reverb");
            DefaultModes.Add("Plate");

            base.Init();
        }
    }
}