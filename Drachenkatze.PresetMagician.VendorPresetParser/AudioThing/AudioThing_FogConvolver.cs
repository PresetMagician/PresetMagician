using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_FogConvolver : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1716479811};

        public override void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetSettingsFile()}";
        }

        public string GetSettingsFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\FogConvolver\settings.ats");
        }

        public override async Task DoScan()
        {
            var vc2parser = new VC2Parser(Plugin, "atp", PresetDataStorer);

            var settingsFile = GetSettingsFile();

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

                var factoryBank = RootBank.CreateRecursive(bankName);

                await vc2parser.DoScan(factoryBank, path);
            }
        }
    }
}