using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_M12Filter: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1298220649 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("M12-Filter");
            ScanPresets(instruments);
        }
    }
}