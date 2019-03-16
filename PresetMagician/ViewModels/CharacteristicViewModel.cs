using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Data;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    public class CharacteristicViewModel : ViewModelBase
    {
        private ModelBackup _modelBackup; 
        public CharacteristicViewModel(Characteristic characteristic)
        {
            DeferValidationUntilFirstSaveCall = false;
            _modelBackup = characteristic.CreateBackup();
            Characteristic = characteristic;
            Characteristics = (from t in Characteristic.GlobalCharacteristics where !t.IsRedirect && t != characteristic orderby t.CharacteristicName select t).ToList();
            CharacteristicsRedirectingToThis = (from t in Characteristic.GlobalCharacteristics
                where t.RedirectCharacteristic == characteristic
                orderby t.CharacteristicName
                select t).ToList();
            AllowRedirect = CharacteristicsRedirectingToThis.Count == 0;
        }

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (IsRedirect && RedirectCharacteristic == null)
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(RedirectCharacteristic),
                    "You need to specify a characteristic to redirect to"));
            }
            base.ValidateFields(validationResults);
        }
        
        protected override async Task<bool> CancelAsync()
        {
            Characteristic.RestoreBackup(_modelBackup);
            return await base.CancelAsync();
        }


        [Model] public Characteristic Characteristic { get; set; }

        [ViewModelToModel("Characteristic")]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9/&+\-\s]+$", ErrorMessage =
            "Only alphanumeric characters, numbers, &, +, /, - and spaces are allowed.")]
        public string CharacteristicName { get; set; }

     

        [ViewModelToModel("Characteristic")] public bool IsRedirect { get; set; }

        [ViewModelToModel("Characteristic")] public Characteristic RedirectCharacteristic { get; set; }

        public List<Characteristic> Characteristics { get; set; }
        public List<Characteristic> CharacteristicsRedirectingToThis { get; set; }
        public new string Title { get; set; }
        public bool AllowRedirect { get; set; }
        
        
    }
}