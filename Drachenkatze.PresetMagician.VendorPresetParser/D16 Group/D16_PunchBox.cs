using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class D16_PunchBox : D16Group, IVendorPresetParser
    {
        protected override string XmlPluginName { get; } = "PunchBox";
        protected override string Extension { get; } = ".pbprs";

        private const string FactoryBankPath = @"D16 Group\PunchBox\Presets\Master.d16pkg";
        private const string UserBankPath = @"D16 Group\PunchBox\UserStore\Presets\Master";

        private int _presetExportCount;

        public override List<int> SupportedPlugins => new List<int> {1347306072};

        public override void Init()
        {
            DefaultModes.Add("Drums");
            DefaultModes.Add("Kick Drum");
            base.Init();
        }

        public override string Remarks { get; set; } =
            "Due to a bug in PunchBox, the plugin needs to be reloaded after 60 exported presets. Export might pause for a few seconds.";

        public override async Task DoScan()
        {
            await ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank.CreateRecursive(BankNameUser));
            await ProcessD16PkgArchive(GetFactoryBankPath(FactoryBankPath), RootBank.CreateRecursive(BankNameFactory));
        }

        public override int GetNumPresets()
        {
            var count = 0;
            count += ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank.CreateRecursive(BankNameUser),
                false).GetAwaiter().GetResult();
            count += ProcessD16PkgArchive(GetFactoryBankPath(FactoryBankPath),
                RootBank.CreateRecursive(BankNameFactory), false).GetAwaiter().GetResult();

            return count;
        }

        public override void OnAfterPresetExport()
        {
            _presetExportCount++;
            if (_presetExportCount <= 60)
            {
                return;
            }

            _presetExportCount = 0;
            RemoteVstService.ReloadPlugin(Plugin.Guid);
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