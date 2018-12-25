using System.Collections.Generic;
using Catel.Collections;
using Catel.Data;
using Newtonsoft.Json;
using PresetMagicianShell.Models.Settings;

namespace PresetMagicianShell.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RuntimeConfiguration : ValidatableModelBase
    {
        [JsonProperty]
        [ExcludeFromValidation]
        public FastObservableCollection<VstDirectory> VstDirectories { get; private set; } =
            new FastObservableCollection<VstDirectory>();

        [JsonProperty] public string NativeInstrumentsUserContentDirectory { get; set; }

        [JsonProperty] public bool ExportWithAudioPreviews { get; set; } = true;

        [JsonProperty]
        public FastObservableCollection<Plugin> CachedPlugins { get; set; } = new FastObservableCollection<Plugin>();

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (VstDirectories != null)
            {
                foreach (var directory in VstDirectories)
                {
                    if (directory.HasErrors)
                    {
                        validationResults.Add(FieldValidationResult.CreateError(nameof(VstDirectories),
                            directory.GetErrorMessage()));
                    }
                }
            }
        }
    }
}