using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.D16_Group
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class D16_Frontier : D16Group, IVendorPresetParser
    {
        protected override string XmlPluginName { get; } = "Frontier";
        protected override string Extension { get; } = ".frprs";

        private const string UserBankPath = @"D16 Group\Frontier\UserStore\Presets\";

        public override List<int> SupportedPlugins => new List<int>
        {
            1179807287
        };

        public override int GetNumPresets()
        {
           return ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank.CreateRecursive(BankNameUser),
                false).GetAwaiter().GetResult();
        }
        
        public override async Task DoScan()
        {
            await ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank.CreateRecursive(BankNameUser));
        }
    }
}