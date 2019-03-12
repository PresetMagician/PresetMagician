using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    public class RenamePresetBankViewModel: ViewModelBase
    {
        public Plugin Plugin { get; set; }
        public PresetBank PresetBank { get; set; }
        
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage="Only alphanumeric characters, numbers and spaces are allowed.")]
        public string NewBankName { get; set; }
        
        public RenamePresetBankViewModel((Plugin plugin, PresetBank presetBank) data)
        {
            DeferValidationUntilFirstSaveCall = false;
            Plugin = data.plugin;
            PresetBank = data.presetBank;
            NewBankName = data.presetBank.BankName;
            Title = "Rename Preset Bank";

        }

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (PresetBank == null)
            {
                return;
                
            }
            if (NewBankName != PresetBank.BankName)
            {
                if (PresetBank.ParentBank.ContainsBankName(NewBankName))
                {
                    validationResults.Add(FieldValidationResult.CreateError(nameof(NewBankName), "The bank name already exists on the same level!"));
                }
            }
            
        }

        protected override Task<bool> SaveAsync()
        {
            PresetBank.BankName = NewBankName;
            return base.SaveAsync();
        }

       
    }
}