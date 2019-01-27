using System.Collections.Generic;
using System.IO;
using Catel.Data;
using Newtonsoft.Json;

namespace PresetMagician.Models.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VstDirectory : ValidatableModelBase
    {
        [JsonProperty] public string Path { get; set; }
        [JsonProperty] public bool Active { get; set; } = true;

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (Active)
            {
                if (!Directory.Exists(Path))
                {
                    validationResults.Add(
                        FieldValidationResult.CreateError(nameof(Path), $"Directory {Path} does not exist"));
                }
            }
        }
    }
}