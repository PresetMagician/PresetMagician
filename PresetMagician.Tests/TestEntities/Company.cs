using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Data;
using PresetMagician.Core.Interfaces;
using SharedModels.Collections;
using SharedModels.Data;
using TrackableEntities.Client;

namespace PresetMagician.Tests.TestEntities
{
    public class Company: ModelBase
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
        public virtual EditableCollection<User> Users { get; set; }= new EditableCollection<User>();

     
    }
}