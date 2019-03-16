using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Catel.Data;
using Catel.Linq;
using Catel.MVVM;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Data;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    public class TypeViewModel : ViewModelBase
    {
        private ModelBackup _modelBackup; 
        public TypeViewModel(Type type)
        {
            DeferValidationUntilFirstSaveCall = false;
            _modelBackup = type.CreateBackup();
            Type = type;
            Types = (from t in Type.GlobalTypes where !t.IsRedirect && t != type orderby t.EffectiveFullTypeName select t).ToList();
            TypesRedirectingToThis = (from t in Type.GlobalTypes
                where t.RedirectType == type
                orderby t.FullTypeName
                select t).ToList();
            AllowRedirect = TypesRedirectingToThis.Count == 0;
        }

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (IsRedirect && RedirectType == null)
            {
                validationResults.Add(FieldValidationResult.CreateError(nameof(RedirectType),
                    "You need to specify a type to redirect to"));
            }
            base.ValidateFields(validationResults);
        }

        protected override async Task<bool> CancelAsync()
        {
            Type.RestoreBackup(_modelBackup);
            return await base.CancelAsync();
        }


        [Model] public Type Type { get; set; }

        [ViewModelToModel("Type")]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9/&+\-\s]+$", ErrorMessage =
            "Only alphanumeric characters, numbers, &, +, /, - and spaces are allowed.")]
        public string TypeName { get; set; }

        [ViewModelToModel("Type")]
        [Required(AllowEmptyStrings = true)]
        [RegularExpression(@"^[a-zA-Z0-9/&+\-\s]+$", ErrorMessage =
            "Only alphanumeric characters, numbers, &, +, /, - and spaces are allowed.")]
        public string SubTypeName { get; set; }

        [ViewModelToModel("Type")] public bool IsRedirect { get; set; }

        [ViewModelToModel("Type")] public Type RedirectType { get; set; }

        public List<Type> Types { get; set; }
        public List<Type> TypesRedirectingToThis { get; set; }
        public new string Title { get; set; }
        public bool AllowRedirect { get; set; }
        
        
    }
}