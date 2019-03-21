using System.Collections.Generic;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public class TypeUsage : WrappedModel<Type>
    {
        public TypeUsage()
        {
        }

        public TypeUsage(Type item) : base(item)
        {
        }

        public Type Type
        {
            get { return OriginalItem; }
        }

        public int UsageCount { get; set; }
        public HashSet<Plugin> Plugins { get; } = new HashSet<Plugin>();
    }
}