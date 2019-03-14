using System.Collections.Generic;
using Ceras;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public interface ICharacteristic
    {
        string CharacteristicName { get; set; }
    }

    public class Characteristic:ModelBase, ICharacteristic
    {
        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }

        private static HashSet<string> _editableProperties { get; } = new HashSet<string>
        {
            nameof(CharacteristicName)
        };
        
        public static GlobalCharacteristicCollection GlobalCharacteristics = new GlobalCharacteristicCollection();

        [Include] public string CharacteristicName { get; set; } = "";
    }
}