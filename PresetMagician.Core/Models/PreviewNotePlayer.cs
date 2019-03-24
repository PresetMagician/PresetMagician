using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Collections;
using Ceras;

namespace PresetMagician.Core.Models
{
    public class PreviewNotePlayer
    {
        public static PreviewNotePlayer Default
        {
            get
            {
                if (_defaultPreviewNotePlayer == null)
                {
                    _defaultPreviewNotePlayer = new PreviewNotePlayer();
                    _defaultPreviewNotePlayer.PreviewNotePlayerId = "default";
                    _defaultPreviewNotePlayer.Name = "Default C3";
                    _defaultPreviewNotePlayer.PreviewNotes.Add(new PreviewNote
                        {NoteNumber = 48, Start = 0, Duration = 1});
                }

                return _defaultPreviewNotePlayer;
            }

            set => _defaultPreviewNotePlayer = value;
        }

        private static PreviewNotePlayer _defaultPreviewNotePlayer;

        public static FastObservableCollection<PreviewNotePlayer> PreviewNotePlayers =
            new FastObservableCollection<PreviewNotePlayer>();

        public static PreviewNotePlayer GetPreviewNotePlayer(string id)
        {
            var previewNotePlayer =
                (from p in PreviewNotePlayers where p.PreviewNotePlayerId == id select p)
                .FirstOrDefault();

            if (previewNotePlayer != null)
            {
                return previewNotePlayer;
            }

            return Default;
        }

        public PreviewNotePlayer()
        {
        }

        [Include] public string PreviewNotePlayerId { get; set; } = Guid.NewGuid().ToString();
        [Include] public string Name { get; set; }
        [Include] public FastObservableCollection<PreviewNote> PreviewNotes { get; set; } = new FastObservableCollection<PreviewNote>();

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
        [Include] public int NoteNumber { get; set; }
        [Include] public double Start { get; set; }
        [Include] public double Duration { get; set; }
    }
}