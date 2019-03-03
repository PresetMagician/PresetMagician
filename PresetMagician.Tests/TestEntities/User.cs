using System.Collections.Generic;
using SharedModels;

namespace PresetMagician.Tests.TestEntities
{
    public class User: TrackableModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Name),
            nameof(Company)
        };
        
        public string Name { get; set; }
        public Company Company { get; set; }
    }
}