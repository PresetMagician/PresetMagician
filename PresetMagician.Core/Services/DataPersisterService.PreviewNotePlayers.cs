using System.Collections.Generic;
using System.IO;
using System.Linq;
using Catel.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        private static string GetPreviewNotePlayersFile()
        {
            Directory.CreateDirectory(DefaultDataStoragePath);
            return Path.Combine(DefaultDataStoragePath, PreviewNotePlayersStorageFile);
        }


        public void LoadPreviewNotePlayers()
        {
            if (File.Exists(GetPreviewNotePlayersFile()))
            {
                var previewNotePlayers =
                    GetLoadSerializer().Deserialize<List<PreviewNotePlayer>>(
                        File.ReadAllBytes(GetPreviewNotePlayersFile()));

                _globalService.PreviewNotePlayers.Clear();
                _globalService.PreviewNotePlayers.AddRange(previewNotePlayers);
            }

            var defaultPlayerFound = false;
            foreach (var previewNotePlayer in _globalService.PreviewNotePlayers)
            {
                if (previewNotePlayer.PreviewNotePlayerId == "default")
                {
                    PreviewNotePlayer.Default = previewNotePlayer;
                    defaultPlayerFound = true;
                }
            }

            if (!defaultPlayerFound)
            {
                _globalService.PreviewNotePlayers.Add(PreviewNotePlayer.Default);
            }
        }

        public void SavePreviewNotePlayers()
        {
            var data = GetSaveSerializer().Serialize(_globalService.PreviewNotePlayers.ToList());
            File.WriteAllBytes(GetPreviewNotePlayersFile(), data);
        }
    }
}