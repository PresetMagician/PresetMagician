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
using PresetMagician.Models;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group.PunchBox
{
    public class D16_PunchBox : D16Group, IVendorPresetParser
    {
        protected override string XmlPluginName { get; } = "PunchBox";
        protected override string Extension { get; } = ".pbprs";

        private const string FactoryBankPath = @"D16 Group\PunchBox\Presets\Master.d16pkg";
        private const string UserBankPath = @"D16 Group\PunchBox\UserStore\Presets\Master";

        

        private int PresetExportCount;

        public override List<int> SupportedPlugins => new List<int> {1347306072};

        public D16_PunchBox()
        {
            DefaultModes.Add("Drums");
            DefaultModes.Add("Kick Drum");
        }

        public override string Remarks { get; set; } =
            "Due to a bug in PunchBox, the plugin needs to be reloaded after 60 exported presets. Export might pause for a few seconds.";

        public void ScanBanks()
        {
            RootBank.PresetBanks.Add(GetFactoryPresets());
            RootBank.PresetBanks.Add(GetUserPresets());
        }

        public override void OnAfterPresetExport()
        {
            PresetExportCount++;
            if (PresetExportCount <= 60)
            {
                return;
            }

            PresetExportCount = 0;
            RemoteVstService.ReloadPlugin(Plugin.Guid);
        }

        private PresetBank GetUserPresets()
        {
            var userBank = new PresetBank {BankName = BankNameUser};

            ProcessPresetDirectory(GetUserBankPath(UserBankPath), userBank);
            return userBank;
        }

        private PresetBank GetFactoryPresets()
        {
            var factoryBank = new PresetBank {BankName = BankNameFactory};


            ProcessD16PKGArchive(GetFactoryBankPath(FactoryBankPath), factoryBank);
            return factoryBank;
        }
        
        protected override void PostProcessXML(XElement presetElement)
        {
           foreach (var element in presetElement.Element("ExtraData").Element("Samples").Elements())
           {
               element.SetAttributeValue("origin", "Factory");
           }
        }
    }
}