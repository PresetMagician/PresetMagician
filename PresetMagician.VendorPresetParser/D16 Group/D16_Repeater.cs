using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.D16_Group
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class D16_Repeater : D16Group, IVendorPresetParser
    {
        protected override string XmlPluginName { get; } = "Repeater";
        protected override string Extension { get; } = ".rpprs";

        private const string FactoryBankPath = @"D16 Group\Repeater\Presets\FactoryPresets.d16pkg";
        private const string UserBankPath = @"D16 Group\Repeater\UserStore\Presets\";

        public override List<int> SupportedPlugins => new List<int> {1380988728};

        public override int GetNumPresets()
        {
            var count = 0;
            count += ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank.CreateRecursive(BankNameUser),
                false).GetAwaiter().GetResult();
            count += ProcessD16PkgArchive(GetFactoryBankPath(FactoryBankPath),
                RootBank.CreateRecursive(BankNameFactory), false).GetAwaiter().GetResult();

            return base.GetNumPresets() + count;
        }

        public override async Task DoScan()
        {
            await ProcessPresetDirectory(GetUserBankPath(UserBankPath), RootBank.CreateRecursive(BankNameUser));
            await ProcessD16PkgArchive(GetFactoryBankPath(FactoryBankPath), RootBank.CreateRecursive(BankNameFactory));
            await base.DoScan();
        }
    }
}