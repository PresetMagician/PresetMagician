using System.Xml.Linq;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandStructArray: RolandStruct
    {
        public RolandStructArray(RolandMemorySection parent, XElement node) : base(parent, node)
        {
        }
    }
}