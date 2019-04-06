using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_FogConvolver : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1716479811};

        protected override string Extension { get; } = "atp";

        public override void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetSettingsFile()}";
            base.Init();
        }

        private string GetSettingsFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\FogConvolver\settings.ats");
        }

        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            var dirs = new List<(string directory, PresetBank presetBank)>();

            var settingsFile = GetSettingsFile();

            if (!File.Exists(settingsFile))
            {
                return new List<(string directory, PresetBank presetBank)>();
            }

            var settingsXml = XDocument.Load(settingsFile);
            var banksElement = settingsXml.Element("FogConvolver_GENERAL_SETTINGS").Element("BANKS");

            var pathsAttribute = banksElement.Attribute("Paths");

            if (pathsAttribute == null)
            {
                Logger.Error($"The settings file {settingsFile} does not contain bank paths.");
                return dirs;
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

                dirs.Add((path, factoryBank));
            }

            return dirs;
        }

        protected override string GetParseDirectory()
        {
            // Should not be called
            throw new NotImplementedException();
        }
    }
}