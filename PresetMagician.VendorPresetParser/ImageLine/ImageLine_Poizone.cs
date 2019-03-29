using System;
using System.Collections.Generic;
using System.IO;
using PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.ImageLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ImageLine_Poizone : RecursiveFXPDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1398896471};

        protected override string Extension { get; } = "fxp";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Image-Line\PoiZone\");
        }
    }
}