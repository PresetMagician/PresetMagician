using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_VerbSuiteClassics : SlateDigital, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1450402640};

       protected override string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\VerbSuite Classics\Presets");
        }

       protected override string GetPresetSectionName()
       {
           return "VscP";
       }
    }
}