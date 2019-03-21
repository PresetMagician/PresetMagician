using System.Collections.Generic;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public class CharacteristicUsage : WrappedModel<Characteristic>
    {
        public CharacteristicUsage()
        {
        }

        public CharacteristicUsage(Characteristic item) : base(item)
        {
        }

        public Characteristic Characteristic
        {
            get { return OriginalItem; }
        }

        public int UsageCount { get; set; }
        public HashSet<Plugin> Plugins { get; } = new HashSet<Plugin>();
    }
}