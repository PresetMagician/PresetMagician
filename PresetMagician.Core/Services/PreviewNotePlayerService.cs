using System;
using System.Collections.Generic;
using System.IO;
using Ceras;
using PresetMagician.Core.Models;

namespace SharedModels.Services
{
    public class PreviewNotePlayerService
    {
        public static Dictionary<string, PreviewNotePlayer> PreviewNotePlayers =
            new Dictionary<string, PreviewNotePlayer> {{"default", PreviewNotePlayer.Default}};
        
        public static string DefaultPreviewNotePlayerStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PreviewNotePlayers.presetmagician");
        
        private readonly CerasSerializer _serializer;

        public PreviewNotePlayerService()
        {
            var serializerConfig = new SerializerConfig();
            serializerConfig.DefaultTargets = TargetMember.None;
            serializerConfig.KnownTypes.Add(typeof(PreviewNotePlayer));

            serializerConfig.VersionTolerance.Mode = VersionToleranceMode.Standard;
            _serializer = new CerasSerializer(serializerConfig);
        }

        public void LoadPreviewNotePlayers()
        {
            if (File.Exists(DefaultPreviewNotePlayerStoragePath))
            {
                PreviewNotePlayers = _serializer.Deserialize<Dictionary<string, PreviewNotePlayer>>(File.ReadAllBytes(DefaultPreviewNotePlayerStoragePath));
            }
            
            if (!PreviewNotePlayers.ContainsKey("default"))
            {
                PreviewNotePlayers.Add("default", PreviewNotePlayer.Default);
            }
        }

        public void SavePreviewNotePlayers()
        {
            var data = _serializer.Serialize(PreviewNotePlayers);
            File.WriteAllBytes(DefaultPreviewNotePlayerStoragePath, data);
        }
        
    }
}