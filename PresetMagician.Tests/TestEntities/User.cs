using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PresetMagician.Core.Data;

namespace PresetMagician.Tests.TestEntities
{
    public class User : ModelBase
    {
        public override HashSet<string> GetEditableProperties()
        {
            return new HashSet<string>
            {
                nameof(Name),
                nameof(Company)
            };
        }


        [Key] public virtual int Id { get; set; }

        public virtual string Name { get; set; }
        public virtual Company Company { get; set; }
    }
}