using System.Collections.Generic;
using SharedModels.Data;

namespace SharedModels.NewModels
{
    public class Characteristic:ModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(CharacteristicName)
        };
        
        public static GlobalCharacteristicCollection GlobalCharacteristics = new GlobalCharacteristicCollection();

        public string CharacteristicName { get; set; } = "";
    }
}