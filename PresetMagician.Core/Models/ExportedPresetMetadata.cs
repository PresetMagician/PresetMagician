using Ceras;

namespace PresetMagician.Core.Models
{
    public class ExportedPresetMetadata : PresetMetadata
    {
        public PreviewNotePlayer PreviewNotePlayer { get; set; } = PreviewNotePlayer.Default;
        [Include] public string PresetHash { get; set; }
        
        [Include]
        public string PreviewNotePlayerId {
            get { return PreviewNotePlayer.PreviewNotePlayerId; }
            set { PreviewNotePlayer = PreviewNotePlayer.GetPreviewNotePlayer(value); }
        }
    }
}