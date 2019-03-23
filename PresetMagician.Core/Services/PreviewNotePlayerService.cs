using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Ceras;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public class PreviewNotePlayerService
    {
       
        private readonly GlobalService _globalService;
        

        public PreviewNotePlayerService(GlobalService globalService)
        {
            _globalService = globalService;
        }

        public bool IsPreviewNotePlayerInUse(PreviewNotePlayer previewNotePlayer)
        {
            foreach (var plugin in _globalService.Plugins)
            {
                var hasPreviewNotePlayer = (from preset in plugin.Presets
                    where ReferenceEquals(preset.PreviewNotePlayer, previewNotePlayer)
                    select preset).Any();

                if (hasPreviewNotePlayer)
                {
                    return true;
                }
            }

            return false;
        }
    }
}