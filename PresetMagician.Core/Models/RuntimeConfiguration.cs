using System;
using System.Collections.Generic;
using Catel.Collections;
using Catel.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PresetMagician.Core.Models.Audio;
using PresetMagician.Core.Models.MIDI;

namespace PresetMagician.Core.Models
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

        [JsonProperty] public bool CompressPresetData { get; set; } = true;

        [JsonProperty] public bool AutoCreateResources { get; set; }

        [JsonProperty] public int NumPoolWorkers { get; set; } = 4;

        [JsonProperty] public int MaxPoolWorkerStartupTime { get; set; } = 20;

        [JsonProperty] public DateTime LastBackupNotificationDateTime { get; set; } = DateTime.Now;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public PresetExportInfo.FolderExportMode FolderExportMode { get; set; } =
            PresetExportInfo.FolderExportMode.SUBFOLDERS_TRIMMED;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public PresetExportInfo.FileOverwriteMode FileOverwriteMode { get; set; } =
            PresetExportInfo.FileOverwriteMode.REPORT_ERROR;

        [JsonProperty] public bool ShowDeveloperTools { get; set; }
        [JsonProperty] public string HexEditorExecutable { get; set; }
        [JsonProperty] public string HexEditorArguments { get; set; }
        [JsonProperty] public AudioOutputDevice AudioOutputDevice { get; set; }
        [JsonProperty] public int AudioLatency { get; set; } = 60;

        [JsonProperty]
        public List<MidiInputDevice> MidiInputDevices { get; private set; } = new List<MidiInputDevice>();


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