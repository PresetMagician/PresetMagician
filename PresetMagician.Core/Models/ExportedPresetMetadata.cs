using Ceras;

namespace PresetMagician.Core.Models
{
    public class ExportedPresetMetadata : PresetMetadata
    {
        [Include] public PreviewNotePlayer PreviewNotePlayer { get; set; } = PreviewNotePlayer.Default;
        [Include] public string PresetHash { get; set; }
    }
}