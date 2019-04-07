using PresetMagician.Core.Enums;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser
{
    public class NullPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override bool IsNullParser => true;
        public override PresetParserPriorityEnum Priority { get; } = PresetParserPriorityEnum.NULL_PRIORITY;

        public override bool CanHandle()
        {
            return true;
        }
    }
}