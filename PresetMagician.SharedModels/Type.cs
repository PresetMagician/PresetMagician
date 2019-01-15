using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedModels
{
    public class Type
    {
        [Key]
        public int Id { get; set; }
        
        public ICollection<Plugin> Plugins { get; set; }
                
        public string Name { get; set; }
        public string SubTypeName { get; set; }
        
    }
}