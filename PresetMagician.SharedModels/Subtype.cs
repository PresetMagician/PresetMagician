using System.ComponentModel.DataAnnotations;

namespace SharedModels
{
    public class Subtype
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
    }
}