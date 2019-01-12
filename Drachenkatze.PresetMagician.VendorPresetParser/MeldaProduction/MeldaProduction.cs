using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Anotar.Catel;
using GSF;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    public class MeldaProduction: AbstractVendorPresetParser
    {
        protected static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"MeldaProduction\");
        
        protected static readonly string FallbackParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"MeldaProduction\");
        
        public void ScanPresetXMLFile(string filename, string rootTag)
        {
            var fullFilename = Path.Combine(ParseDirectory, filename);

            if (!File.Exists(fullFilename))
            {
                fullFilename = Path.Combine(FallbackParseDirectory, filename);
            }

            if (!File.Exists(fullFilename))
            {
                LogTo.Error(
                    $"Error: Could not find {filename} in neither {ParseDirectory} nor {FallbackParseDirectory}");
                return;
            }

            var rootDocument = XDocument.Parse(File.ReadAllText(fullFilename));

            var rootElement = rootDocument.Element(rootTag);

            var factoryBank = new PresetBank();
            factoryBank.BankName = BankNameFactory;
            ScanPresetXML(rootElement, factoryBank);
            
            RootBank.PresetBanks.Add(factoryBank);
        }

        public void ScanPresetXML(XElement rootElement, PresetBank presetBank)
        {
            var directories = rootElement.Elements("Directory");

            foreach (var directory in directories)
            {
                var bankNameElement = directory.Attribute("Name");

                if (bankNameElement == null)
                {
                    bankNameElement = directory.Attribute("Name");

                    if (bankNameElement == null)
                    {
                        LogTo.Error("A bankNameElement has no name attribute.");
                        LogTo.Debug(directory.ToString);
                        continue;
                    }
                }
                var subBank = new PresetBank();
                subBank.BankName = bankNameElement.Value;
                ScanPresetXML(directory, subBank);
                
                presetBank.PresetBanks.Add(subBank);
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
                        LogTo.Error("A presetElement has no name attribute.");
                        LogTo.Debug(presetElement.ToString);
                        continue;
                    }
                }
                
                var preset = new Preset();
                preset.PresetName = nameAttribute.Value;
                preset.SetPlugin(VstPlugin);
                preset.PresetBank = presetBank;

                var base64 = presetElement.Value.Trim().Replace("-", "/").Replace("$", "");
                
                preset.PresetData = Convert.FromBase64String(base64);
                
                Presets.Add(preset);
            }
        }
    }
}