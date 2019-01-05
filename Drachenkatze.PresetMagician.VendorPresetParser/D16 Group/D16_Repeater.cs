using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class D16_Repeater: D16Group, IVendorPresetParser
    {
        protected override string XmlPluginName { get; } = "Repeater";
        protected override string Extension { get; } = ".rpprs";
        
        private const string FactoryBankPath = @"D16 Group\Repeater\Presets\FactoryPresets.d16pkg";
        private const string UserBankPath = @"D16 Group\Repeater\UserStore\Presets\";
        
        public override List<int> SupportedPlugins => new List<int> {1380988728};
        
        public void ScanBanks()
        {
            RootBank.PresetBanks.Add(GetFactoryPresets());
            RootBank.PresetBanks.Add(GetUserPresets());
        }
        
        private PresetBank GetFactoryPresets()
        {
            var factoryBank = new PresetBank
            {
                BankName = BankNameFactory
            };

           
            ProcessD16PKGArchive(GetFactoryBankPath(FactoryBankPath), factoryBank);
            return factoryBank;
        }
        
        private PresetBank GetUserPresets()
        {
            var userBank = new PresetBank {BankName = BankNameUser};

            ProcessPresetDirectory(GetUserBankPath(UserBankPath), userBank);
            return userBank;
        }
    }
}