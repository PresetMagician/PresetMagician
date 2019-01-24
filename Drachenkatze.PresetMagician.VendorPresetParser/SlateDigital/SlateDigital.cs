using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.SlateDigital
{
    public abstract class SlateDigital : AbstractVendorPresetParser
    {
        protected abstract string GetDataDirectory();

        protected string GetExtension()
        {
            return "epf";
        }
        protected abstract string GetPresetSectionName();

        public override void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetDataDirectory()}";
        }

        public override async Task DoScan()
        {
            var parser = new SlateDigitalPresetParser(RemoteVstService, Plugin, GetExtension(), PresetDataStorer, GetPresetSectionName());
            await parser.DoScan(RootBank, GetDataDirectory());
        }
    }
}