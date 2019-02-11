// NOTE: THIS PLUGIN IS HUGELY COMPLEX DUE TO NESTED XML WITHIN ATTRIBUTES -> NOT GOING TO BE SUPPORTED
/*
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Anotar.Catel;
using PresetMagician.VendorPresetParser.Common;
using PresetMagician.VstHost.VST;
using JetBrains.Annotations;

namespace PresetMagician.VendorPresetParser.SlateDigital
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class SlateDigital_VirtualMixRack : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1447909458};

       protected override string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Slate Digital\Virtual Mix Rack\Presets\Virtual Mix Rack");

            var parser = new SlateDigitalVMXPresetParser(VstPlugin, "epf", Presets, "VMXR");
            parser.DoScan(RootBank, directory);
        }
    }

    public class SlateDigitalVMXPresetParser : SlateDigitalPresetParser
    {
        public SlateDigitalVMXPresetParser(IVstPlugin vstPlugin, string extension, ObservableCollection<Preset> presets, string presetSectionName) : base(vstPlugin, extension, presets, presetSectionName)
        {
        }
        
        protected override void MigrateData(XNode source, XNode dest, Preset preset)
        {
            base.MigrateData(source, dest, preset);
            
            var dataNode =
                $"/package/archives/archive[@client_id='{_presetSectionName}-modules-preset']/section[@id='SlotPresets']/entry";
            
            Plugin.Debug(dataNode);
            var elements = source.XPathSelectElements(dataNode);
            Plugin.Debug(elements.Count().ToString);
            
            foreach (var element in elements)
            {
                Plugin.Debug(element.Attribute("value").Value);
            }
        }
    }
}*/

