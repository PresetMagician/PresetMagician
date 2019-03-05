using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedModels;
using SharedModels.Collections;

namespace PresetMagician.Tests.TestEntities
{
    public class Company: TrackableModelBaseFoo
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Users),
            nameof(Name),
            nameof(AdminUser)
        };

        [Key]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; } = "";
        
        public virtual User AdminUser { get; set; }
        public virtual IList<User> Users { get; set; }= new TrackableCollection<User>();

     
    }
}