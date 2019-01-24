using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_VirtualTapeMachines : SlateDigital, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1448365427};

       protected override string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\Virtual Tape Machines\Presets");
        }

       protected override string GetPresetSectionName()
       {
           return "VTMs";
       }
    }
}