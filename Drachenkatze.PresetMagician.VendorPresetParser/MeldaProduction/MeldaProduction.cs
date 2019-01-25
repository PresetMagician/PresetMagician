using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    public abstract class MeldaProduction : AbstractVendorPresetParser
    {
        protected abstract string PresetFile { get; }
        protected abstract string RootTag { get; }

        protected static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"MeldaProduction\");

        protected static readonly string FallbackParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"MeldaProduction\");

        public override int GetNumPresets()
        {
            return ScanPresetXmlFile(PresetFile, RootTag, false).GetAwaiter().GetResult();
        }

        public override async Task DoScan()
        {
            await ScanPresetXmlFile(PresetFile, RootTag);
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
                Plugin.Error(
                    $"Error: Could not find {filename} in neither {ParseDirectory} nor {FallbackParseDirectory}");
                return 0;
            }

            var rootDocument = XDocument.Parse(File.ReadAllText(fullFilename));

            var rootElement = rootDocument.Element(rootTag);

            return await ScanPresetXml(rootElement, RootBank.CreateRecursive("Factory"), persist);
        }

        public async Task<int> ScanPresetXml(XElement rootElement, PresetBank presetBank, bool persist = true)
        {
            int count = 0;
            var directories = rootElement.Elements("Directory");

            foreach (var directory in directories)
            {
                var bankNameElement = directory.Attribute("Name");

                if (bankNameElement == null)
                {
                    bankNameElement = directory.Attribute("name");

                    if (bankNameElement == null)
                    {
                        Plugin.Error("A bankNameElement has no name attribute.");
                        Plugin.Debug(directory.ToString());
                        continue;
                    }
                }

                var subBank = presetBank.CreateRecursive(bankNameElement.Value);
                count += await ScanPresetXml(directory, subBank, persist);
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
                        Plugin.Error("A presetElement has no name attribute.");
                        Plugin.Debug(presetElement.ToString());
                        continue;
                    }
                }

                count++;

                if (persist)
                {
                    var preset = new Preset
                        {PresetName = nameAttribute.Value, Plugin = Plugin, PresetBank = presetBank};

                    var base64 = presetElement.Value.Trim().Replace("-", "/").Replace("$", "");

                    await PresetDataStorer.PersistPreset(preset, Convert.FromBase64String(base64));
                }
            }

            return count;
        }
    }
}