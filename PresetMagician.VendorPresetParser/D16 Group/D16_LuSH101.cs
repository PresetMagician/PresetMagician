using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.D16_Group
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class D16_LuSH101 : D16Group, IVendorPresetParser
    {
        private const string UserBankPath = @"D16 Group\LuSH-101\Presets\";
        protected override string XmlPluginName { get; } = "LuSH-101";
        protected override string Extension { get; } = ".shprst";

        public override List<int> SupportedPlugins => new List<int>
        {
            1397240113
        };

        public override int GetNumPresets()
        {
            return base.GetNumPresets() + ProcessPresetDirectory(GetUserBankPath(UserBankPath),
                       RootBank.CreateRecursive(BankNameUser),
                       false).GetAwaiter().GetResult();
        }

        public override async Task DoScan()
        {
            await ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank);
            await base.DoScan();
        }

        protected override (PresetParserMetadata preset, byte[] presetData) GetPreset(string name, string presetData,
            PresetBank presetBank)
        {
            var pluginState = new XElement("PluginState");
            pluginState.SetAttributeValue("application", XmlPluginName);
            var midiControlMap = new XElement("MidiControlMap");
            midiControlMap.SetAttributeValue("name", XmlPluginName);


            var xmlPreset = XDocument.Parse(presetData);
            var presetElement = xmlPreset.Element("Preset");
            var presetName = name.Replace(Extension, "");
            presetElement.SetAttributeValue("name", "ParametersState");

            var preset = new PresetParserMetadata
            {
                PresetName = name.Replace(Extension, ""), Plugin = PluginInstance.Plugin, BankPath = presetBank.BankPath
            };

            var tagsAttribute = presetElement.Attribute("tags");

            if (!(tagsAttribute is null))
            {
                var modes = GetModes(tagsAttribute.Value);

                foreach (var modeName in modes)
                {
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = modeName});
                }
            }

            presetElement.SetAttributeValue("tags", null);
            presetElement.SetAttributeValue("version", null);


            pluginState.Add(presetElement);

            var presetElement2 = new XElement("Preset");
            presetElement2.SetAttributeValue("name", "OtherParameters");

            var param = new XElement("param");
            param.SetAttributeValue("name", "Preset Name");
            param.SetAttributeValue("value", presetName);

            presetElement2.Add(param);
            pluginState.Add(presetElement2);


            var ms = RecursiveVC2Parser.WriteVC2(pluginState.ToString());

            return (preset, ms.ToArray());
        }
    }
}