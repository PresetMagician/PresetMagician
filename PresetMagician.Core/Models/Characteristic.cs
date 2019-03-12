using System.Collections.Generic;
using Ceras;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public class Characteristic:ModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(CharacteristicName)
        };
        
        public static GlobalCharacteristicCollection GlobalCharacteristics = new GlobalCharacteristicCollection();

        [Include] public string CharacteristicName { get; set; } = "";
    }
}