using System.ComponentModel.DataAnnotations;

namespace PresetMagician.Legacy.Models
{
    public class BankFile
    {
        [Key] public int BankId { get; set; }
        
        public Plugin Plugin { get; set; }

        public string Path { get; set; }
        
        public string BankName { get; set; }
        
        public string ProgramRange { get; set; }

    }
}