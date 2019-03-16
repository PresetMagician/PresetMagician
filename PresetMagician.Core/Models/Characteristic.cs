using System;
using System.Collections.Generic;
using Ceras;
using Ceras.Resolvers;
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
            nameof(CharacteristicName),
            nameof(IsRedirect),
            nameof(RedirectCharacteristic),
            nameof(IsIgnored)
        };
        
        public static GlobalCharacteristicCollection GlobalCharacteristics = new GlobalCharacteristicCollection();

        [Include] public string CharacteristicName { get; set; } = "";
        
        private bool _isRedirect;

        [Include]
        public bool IsRedirect
        {
            get => _isRedirect;
            set { _isRedirect = value;
                if (!_isRedirect)
                {
                    RedirectCharacteristic = null;
                }
            }
        }
        
        [Include] public bool IsIgnored { get; set; }

        [Include] public Characteristic RedirectCharacteristic { get; set; }

        public string EffectiveCharacteristicName
        {
            get
            {
                if (IsRedirect && RedirectCharacteristic != null)
                {
                    return RedirectCharacteristic.CharacteristicName;
                }

                return CharacteristicName;
            }
        }
        
        public string StatusDescription
        {
            get
            {
                if (IsRedirect)
                {
                    return $"Redirected from {CharacteristicName}";
                }

                if (IsIgnored)
                {
                    return "This characteristic is ignored";
                }

                return null;
            }
        }
    }
}