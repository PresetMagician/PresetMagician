using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catel.Collections;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;
using PresetMagicianShell.Models.Settings;

namespace PresetMagicianShell.Models
{
    [JsonObjectAttribute(MemberSerialization.OptIn)]
    public class RuntimeConfiguration: ValidatableModelBase
    {
        [JsonProperty]
        [ExcludeFromValidationAttribute]
        public FastObservableCollection<VstDirectory> VstDirectories { get; private set; } = new FastObservableCollection<VstDirectory>();

        [JsonProperty]
        public string NativeInstrumentsUserContentDirectory { get; set; } = null;

        [JsonProperty]
        public FastObservableCollection<Plugin> CachedPlugins { get; set; } = new FastObservableCollection<Plugin>();

        protected override void ValidateFields(List<IFieldValidationResult> validationResults)
        {
            if (VstDirectories != null) {
            foreach (var directory in VstDirectories)
            {
                if (directory.HasErrors)
                {
                    validationResults.Add(FieldValidationResult.CreateError(nameof(VstDirectories), directory.GetErrorMessage()));
                }
            }
            }
        }
    }
}