using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.ImageLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ImageLine_Sawer : RecursiveBankDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1398888274};

        protected override string Extension { get; } = "sawer";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Image-Line\Sawer\");
        }
    }
}