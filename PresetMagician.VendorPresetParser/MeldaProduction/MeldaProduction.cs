using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using PresetMagician.Core.Models;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    public abstract class MeldaProduction : AbstractVendorPresetParser
    {
        protected static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"MeldaProduction\");

        protected static readonly string FallbackParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"MeldaProduction\");

        protected abstract string PresetFile { get; }
        protected abstract string RootTag { get; }
        private Dictionary<string, int> SourceFileDuplicates = new Dictionary<string, int>();

        public override int GetNumPresets()
        {
            return base.GetNumPresets() + ScanPresetXmlFile(PresetFile, RootTag, false).GetAwaiter().GetResult();
        }

        public override async Task DoScan()
        {
            SourceFileDuplicates.Clear();
            await ScanPresetXmlFile(PresetFile, RootTag);
            await base.DoScan();
        }

        public async Task<int> ScanPresetXmlFile(string filename, string rootTag, bool persist = true)
        {
            var fullFilename = Path.Combine(ParseDirectory, filename);

            if (!File.Exists(fullFilename))
            {
                fullFilename = Path.Combine(FallbackParseDirectory, filename);
            }

            if (!File.Exists(fullFilename))
            {
                Logger.Error(
                    $"Error: Could not find {filename} in neither {ParseDirectory} nor {FallbackParseDirectory}");
                return 0;
            }

            var rootDocument = XDocument.Parse(File.ReadAllText(fullFilename));

            var rootElement = rootDocument.Element(rootTag);

            return await ScanPresetXml(rootElement, fullFilename, RootBank.CreateRecursive("Factory"), persist);
        }

        public async Task<int> ScanPresetXml(XElement rootElement, string fileName, PresetBank presetBank,
            bool persist = true)
        {
            var count = 0;
            var directories = rootElement.Elements("Directory");

            foreach (var directory in directories)
            {
                var bankNameElement = directory.Attribute("Name");

                if (bankNameElement == null)
                {
                    bankNameElement = directory.Attribute("name");

                    if (bankNameElement == null)
                    {
                        Logger.Warning($"A directory within {fileName} contains no name attribute. Maybe the file is corrupt?");
                        continue;
                    }
                }

                var subBank = presetBank.CreateRecursive(bankNameElement.Value);
                count += await ScanPresetXml(directory, fileName, subBank, persist);
            }

            var presets = rootElement.Elements("preset");

            foreach (var presetElement in presets)
            {
                var nameAttribute = presetElement.Attribute("name");

                if (nameAttribute == null)
                {
                    nameAttribute = presetElement.Attribute("Name");

                    if (nameAttribute == null)
                    {
                        Logger.Error($"A preset in the file {fileName} has no name attribute. Ignoring the preset, "+
                            "as PresetMagician does not know how to identify the name of the preset.");
                        continue;
                    }
                }

                count++;

                if (persist)
                {
                    var sourceFile = fileName + ":" + presetBank.BankPath + "/" + nameAttribute.Value;
                    var presetName = nameAttribute.Value;
                    if (SourceFileDuplicates.ContainsKey(sourceFile))
                    {
                        SourceFileDuplicates[sourceFile]++;
                        sourceFile += ":" + (SourceFileDuplicates[sourceFile] + 1);
                        presetName += "-1";
                        Logger.Warning(
                            $"The preset file {fileName} contains a duplicate preset name in {presetBank.BankPath}/"+
                        $"{nameAttribute.Value}. "+Environment.NewLine+Environment.NewLine +
                            "PresetMagician has no reliable way to check which preset is which "+
                            "(think of it as two files with exactly the same name in the same directory). "+
                            Environment.NewLine+Environment.NewLine +
                            $"PresetMagician will use {presetName} for it; however, it could happen that the preset " +
                            "data gets mixed up between these duplicate presets."+
                            Environment.NewLine+Environment.NewLine + 
                            "To avoid this, please rename the duplicates to an unique name.");
                    }
                    else
                    {
                        SourceFileDuplicates.Add(sourceFile, 0);
                    }
                    
                    var preset = new PresetParserMetadata
                    {
                        PresetName = presetName, Plugin = PluginInstance.Plugin,
                        BankPath = presetBank.BankPath,
                        SourceFile = sourceFile
                    };

                    var base64 = presetElement.Value.Trim().Replace("-", "/").Replace("$", "");

                    await DataPersistence.PersistPreset(preset, Convert.FromBase64String(base64));
                }
            }

            return count;
        }
    }
}