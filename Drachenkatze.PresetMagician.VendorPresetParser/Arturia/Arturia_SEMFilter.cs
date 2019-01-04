using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_SEMFilter: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1330857577 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("SEM-Filter");
            ScanPresets(instruments);
        }
    }
}