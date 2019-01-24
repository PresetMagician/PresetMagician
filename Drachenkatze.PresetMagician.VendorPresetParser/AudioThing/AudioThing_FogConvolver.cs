using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Anotar.Catel;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_FogConvolver : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1716479811};


        public void ScanBanks()
        {
            var settingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\FogConvolver\settings.ats");

            var settingsXml = XDocument.Load(settingsFile);
            var banksElement = settingsXml.Element("FogConvolver_GENERAL_SETTINGS").Element("BANKS");

            var pathsAttribute = banksElement.Attribute("Paths");

            if (pathsAttribute == null)
            {
                Plugin.Error("The settings file does not contain bank paths.");
                return;
            }

            var paths = pathsAttribute.Value.Split('|');

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var bankName = path.Split('\\').Last();
                var factoryBank = new PresetBank
                {
                    BankName = bankName
                };

                RootBank.PresetBanks.Add(factoryBank);
                DoScan(factoryBank, path);

            }
            
            
        }
    }
}