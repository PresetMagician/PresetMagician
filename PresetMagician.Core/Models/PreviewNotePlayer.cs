using System;
using System.Collections.Generic;
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

        public static Dictionary<string, PreviewNotePlayer> PreviewNotePlayers =
            new Dictionary<string, PreviewNotePlayer>();

        public static PreviewNotePlayer GetPreviewNotePlayer(PreviewNotePlayer player)
        {
            if (PreviewNotePlayers.ContainsKey(player.PreviewNotePlayerId))
            {
                return PreviewNotePlayers[player.PreviewNotePlayerId];
            }
            else
            {
                PreviewNotePlayers.Add(player.PreviewNotePlayerId, player);
                return player;
            }
        }

        public PreviewNotePlayer()
        {
            PreviewNotePlayers.Add(PreviewNotePlayerId, this);
        }

        [Include] public string PreviewNotePlayerId { get; private set; } = Guid.NewGuid().ToString();
        [Include] public string Name { get; set; }
        [Include] public List<PreviewNote> PreviewNotes { get; } = new List<PreviewNote>();

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