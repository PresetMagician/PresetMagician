using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_FGX : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1179069784};

        protected override string Extension { get; } = "";

        public override void Init()
        {
            SetPreProcessXmlFunction(PreProcessXML);
            base.Init();
        }

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\FG-X Virtual Mastering Console\Presets");
        }

        private string PreProcessXML(string data)
        {
            var xmlPreset = XDocument.Parse(data);

            var rootElement = xmlPreset.Element("FGXSETTINGS");

            foreach (var attr in rootElement.Attributes().ToList())
            {
                if (attr.Name.LocalName.StartsWith("FG", false, CultureInfo.InvariantCulture))
                {
                    var newAtt = new XAttribute(attr.Name.LocalName + "0", attr.Value);
                    attr.Remove();
                    rootElement.Add(newAtt);
                }
            }

            return xmlPreset.ToString();
        }
    }
}