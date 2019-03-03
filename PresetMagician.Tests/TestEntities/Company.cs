using System.Collections.Generic;
using System.ComponentModel;
using SharedModels;
using SharedModels.Collections;

namespace PresetMagician.Tests.TestEntities
{
    public class Company: TrackableModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(Users),
            nameof(Name),
            nameof(AdminUser)
        };

        public string Name { get; set; } = "";
        
        public User AdminUser { get; set; }
        public TrackableCollection<User> Users { get; set; }= new TrackableCollection<User>();

     
    }
}