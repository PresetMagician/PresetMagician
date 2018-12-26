using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Catel.Collections;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group.PunchBox
{
    public class D16PunchBox : D16Group, IVendorPresetParser
    {
        private const string FactoryBankPath = @"D16 Group\PunchBox\Presets\Master.d16pkg";
        private const string UserBankPath = @"D16 Group\PunchBox\UserStore\Presets\Master";

        private const string BankNameUser = "User";

        private int PresetExportCount;

        public override List<int> SupportedPlugins => new List<int> {1347306072};

        public override string Remarks { get; set; } =
            "Due to a bug in PunchBox, the plugin needs to be reloaded after 60 exported presets. Export might pause for a few seconds.";

        public void ScanBanks()
        {
            RootBank.PresetBanks.Add(GetFactoryPresets());
            RootBank.PresetBanks.Add(GetUserPresets());
        }

        public override void OnAfterPresetExport(VstHost host, IVstPlugin plugin)
        {
            PresetExportCount++;
            Debug.WriteLine(PresetExportCount);
            if (PresetExportCount > 60)
            {
                PresetExportCount = 0;
                host.UnloadVST(plugin);
                host.LoadVST(plugin);
            }
        }

        private PresetBank GetUserPresets()
        {
            var userBank = new PresetBank();
            userBank.BankName = BankNameUser;

            return userBank;
        }

        private PresetBank GetFactoryPresets()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

            using (var archive = ZipFile.OpenRead(GetFactoryBankPath()))
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

                    Presets.Add(GetPreset(presetEntry.Name, ms, factoryBank));
                }
            }

            return factoryBank;
        }

        private Preset GetPreset(string name, MemoryStream stream, PresetBank presetBank)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var pluginState = new XElement("PluginState");
            pluginState.SetAttributeValue("application", "PunchBox");
            var parametersState = new XElement("ParametersState");
            var midiControlMap = new XElement("MidiControlMap");
            midiControlMap.SetAttributeValue("name", "PunchBox");

            pluginState.Add(parametersState);

            var xmlPreset = XDocument.Load(stream);
            var presetElement = xmlPreset.Element("Preset");
            presetElement.SetAttributeValue("name", name.Replace(".pbprs", ""));

            var preset = new Preset();

            var tagsAttribute = presetElement.Attribute("tags");

            if (!(tagsAttribute is null))
            {
                preset.Modes.AddRange(GetModes(tagsAttribute.Value));
            }

            var types = new ObservableCollection<string>();
            types.Add("Drums");
            types.Add("Kick Drum");
            preset.Types.Add(types);
            presetElement.SetAttributeValue("tags", null);
            presetElement.SetAttributeValue("version", null);

            foreach (var element in presetElement.Element("ExtraData").Element("Samples").Elements())
            {
                element.SetAttributeValue("origin", "Factory");
            }

            parametersState.Add(presetElement);

            
            preset.PresetName = name.Replace(".pbprs", "");
            preset.SetPlugin(VstPlugin);
            preset.PresetBank = presetBank;

            var ms = VC2Writer.WriteVC2(pluginState.ToString());

            preset.PresetData = ms.ToArray();

            return preset;
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
            var tagsAttributeValue = tags;
            var tagAttributes = tagsAttributeValue.Split(';');

            return tags.Split(';')
                .Select(x => x.Split(':'))
                .ToDictionary(x => x[0], x => x[1].Split('|'));
        }

        private string GetFactoryBankPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                FactoryBankPath);
        }

        private string GetUserBankPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), UserBankPath);
        }
    }
}