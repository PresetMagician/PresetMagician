using System;
using System.Collections.Generic;

namespace SharedModels.NewModels
{
    public class PreviewNotePlayer
    {
        public static PreviewNotePlayer Default
        {
            get {
                if (_defaultPreviewNotePlayer == null)
                {
                    _defaultPreviewNotePlayer = new PreviewNotePlayer();
                    _defaultPreviewNotePlayer.PreviewNotePlayerId = "default";
                    _defaultPreviewNotePlayer.Name = "Default C3";
                    _defaultPreviewNotePlayer.PreviewNotes.Add(new PreviewNote {NoteNumber = 48, Start=0, Duration = 1});
                }
                
                return _defaultPreviewNotePlayer;
                
            }

            set => _defaultPreviewNotePlayer = value;
        }

        private static PreviewNotePlayer _defaultPreviewNotePlayer;
        
        

        public string PreviewNotePlayerId { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public List<PreviewNote> PreviewNotes { get; } = new List<PreviewNote>();

        public bool IsEqualTo(PreviewNotePlayer target)
        {
            if (target == null)
            {
                return false;
            }
            if (target.Name != Name)
            {
                return false;
            }

            return true;
        }
    }

    public class PreviewNote
    {
        public int NoteNumber { get; set; }
        public double Start { get; set; }
        public double Duration { get; set; }
    }
}