// Hard to implement, in-memory has additional defaults not present in the preset, preventing setting
// Namely 5.1 channel information 

/*using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using PresetMagician.VstHost.VST;
using JetBrains.Annotations;

namespace PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Eiosis_AirEQ51 : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1095070005};

       protected override string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Eiosis\AirEQ 5.1\Presets");

            var parser = new AirEQ51PresetParser(VstPlugin, "epf", Presets, "AEqP");
            parser.DoScan(RootBank, directory);
        }
    }

    class AirEQ51PresetParser : SlateDigitalPresetParser
    {
        public AirEQ51PresetParser(IVstPlugin vstPlugin, string extension, ObservableCollection<Preset> presets, string presetSectionName) : base(vstPlugin, extension, presets, presetSectionName)
        {
        }
        
        protected override void MigrateData(XNode source, XNode dest, Preset preset)
        {
            var dataNode =
                $"/package/archives/archive[@client_id='{_presetSectionName}-preset']/section[@id='ParameterValues']/entry";

            var nodes = source.XPathSelectElements(dataNode);

            var dataNode2 =
                $"/package/archives/archive[@client_id='AEq5-state']/section[@id='Slot0']";
            var insertNode = dest.XPathSelectElement(dataNode2);
            insertNode.Elements().Remove();
            insertNode.Add(nodes);
            
            var presetNameNode = new XElement("entry");
            presetNameNode.SetAttributeValue("id", "Preset name");
            presetNameNode.SetAttributeValue("type", "string");
            
            
            presetNameNode.SetAttributeValue("value",preset.PresetBank.BankName + "/"+preset.PresetName);
            
            insertNode.Add(presetNameNode);
        }
    }
}*/

