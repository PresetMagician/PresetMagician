using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Anotar.Catel;
using Catel.Collections;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group
{
    public abstract class D16Group : AbstractVendorPresetParser
    {
        protected abstract string XmlPluginName { get; }
        protected abstract string Extension { get; }
        
        protected void ProcessD16PKGArchive(string archiveName, PresetBank bank)
        {
            Plugin.Debug($"ProcessD16PKGArchive {archiveName}");
            using (var archive = ZipFile.OpenRead(archiveName))
            {
                var entry = archive.GetEntry("content");

                var contentStream = entry.Open();

                var contentArchive = new ZipArchive(contentStream);

                foreach (var presetEntry in contentArchive.Entries)
                {
                    if (presetEntry.Name == "__desc__")
                    {
                        continue;
                    }

                    var ms = new MemoryStream();
                    presetEntry.Open().CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var presetData = Encoding.UTF8.GetString(ms.ToArray()); 

                    Presets.Add(GetPreset(presetEntry.Name, presetData, bank));
                }
            }
        }

        protected void ProcessPresetDirectory(string presetDirectory, PresetBank bank)
        {
            Plugin.Debug($"ProcessPresetDirectory {presetDirectory}");
            var dirInfo = new DirectoryInfo(presetDirectory);

            
            foreach (var file in dirInfo.EnumerateFiles("*" + Extension))
            {
                var presetData = File.ReadAllText(file.FullName);
                Presets.Add(GetPreset(file.Name, presetData, bank));
            }
        }

        protected virtual Preset GetPreset(string name, string presetData, PresetBank presetBank)
        {
            var pluginState = new XElement("PluginState");
            pluginState.SetAttributeValue("application", XmlPluginName);
            var parametersState = new XElement("ParametersState");
            var midiControlMap = new XElement("MidiControlMap");
            midiControlMap.SetAttributeValue("name", XmlPluginName);

            pluginState.Add(parametersState);

            var xmlPreset = XDocument.Parse(presetData);
            var presetElement = xmlPreset.Element("Preset");
            presetElement.SetAttributeValue("name", name.Replace(Extension, ""));

            var preset = new Preset();

            var tagsAttribute = presetElement.Attribute("tags");

            if (!(tagsAttribute is null))
            {
                preset.Modes.AddRange(GetModes(tagsAttribute.Value));
            }

            presetElement.SetAttributeValue("tags", null);
            presetElement.SetAttributeValue("version", null);

         

            parametersState.Add(presetElement);

            
            preset.PresetName = name.Replace(Extension, "");
            preset.SetPlugin(Plugin);
            preset.PresetBank = presetBank;

            var ms = VC2Parser.WriteVC2(pluginState.ToString());

            preset.PresetData = ms.ToArray();

            return preset;
        }

        protected virtual void PostProcessXML(XElement presetElement)
        {
            
        }
        
        protected List<string> GetModes(string tags)
        {
            List<string> modes = new List<string>();

            var dict = ExtractTags(tags);
            if (dict.ContainsKey("Type"))
            {
                modes.AddRange(dict["Type"]);
            }

            if (dict.ContainsKey("Mode"))
            {
                modes.AddRange(dict["Mode"]);
            }
            return modes;
        }
        
        protected Dictionary<string, string[]> ExtractTags(string tags)
        {
            return tags.Split(';')
                .Select(x => x.Split(':'))
                .ToDictionary(x => x[0], x => x[1].Split('|'));
        }
        
        protected string GetFactoryBankPath(string factoryBankPath)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                factoryBankPath);
        }

        protected string GetUserBankPath(string userBankPath)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), userBankPath);
        }
       
    }
}