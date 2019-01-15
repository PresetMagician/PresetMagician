using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class D16_Frontier: D16Group, IVendorPresetParser
    {
        protected override string XmlPluginName { get; } = "Frontier";
        protected override string Extension { get; } = ".frprs";
        
        private const string UserBankPath = @"D16 Group\Frontier\UserStore\Presets\";
        
        public override List<int> SupportedPlugins => new List<int>
        {
            1179807287
        };
        
        public void ScanBanks()
        {
            RootBank.PresetBanks.Add(GetUserPresets());
        }

        private PresetBank GetUserPresets()
        {
            var userBank = new PresetBank {BankName = BankNameUser};

            ProcessPresetDirectory(GetUserBankPath(UserBankPath), userBank);
            return userBank;
        }
    }
}