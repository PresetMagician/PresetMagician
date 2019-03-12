using System;
using System.Collections.Generic;
using CannedBytes.Midi.Message;
using Catel.Collections;
using Catel.Data;
using Newtonsoft.Json;
using PresetMagician.Core.Models;

namespace PresetMagician.Models
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
        public int DefaultPreviewMidiNoteNumber
        {
            get { return DefaultPreviewMidiNote.NoteNumber; }
            set { DefaultPreviewMidiNote.NoteNumber = value; }
        }

        public MidiNoteName DefaultPreviewMidiNote { get; set; } = new MidiNoteName("C3");

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