using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_FGX: AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1179069784};

        public void ScanBanks()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\FG-X Virtual Mastering Console\Presets");

            var vc2parser = new VC2Parser(Plugin, "",Presets);
            vc2parser.SetPreProcessXmlFunction(PreProcessXML);
            vc2parser.DoScan(RootBank, directory);
        }

        private string PreProcessXML(string data)
        {
            var xmlPreset = XDocument.Parse(data);

            var rootElement = xmlPreset.Element("FGXSETTINGS");

            foreach (var attr in rootElement.Attributes().ToList())
            {
                if (attr.Name.LocalName.StartsWith("FG", false, CultureInfo.InvariantCulture))
                {
                    XAttribute newAtt = new XAttribute(attr.Name.LocalName + "0", attr.Value);
                    attr.Remove();
                    rootElement.Add(newAtt);
                }
            }
            Plugin.Debug(xmlPreset.ToString());
            return xmlPreset.ToString();
        }
        
    }
}