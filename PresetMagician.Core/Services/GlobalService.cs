using System;
using System.Collections.Generic;
using Catel.Collections;
using Orchestra;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.Core.Services
{
    public class GlobalService
    {
        public GlobalService()
        {
            GlobalCharacteristics = Characteristic.GlobalCharacteristics;
            GlobalTypes = Type.GlobalTypes;
            PreviewNotePlayers = PreviewNotePlayer.PreviewNotePlayers;
            RuntimeConfiguration = new RuntimeConfiguration();
        }

        public FastObservableCollection<Plugin> Plugins { get; } = new FastObservableCollection<Plugin>();
        public GlobalCharacteristicCollection GlobalCharacteristics { get; }
        public GlobalTypeCollection GlobalTypes { get; }
        public List<PreviewNotePlayer> PreviewNotePlayers { get; }

        public IRemoteVstHostProcessPool RemoteVstHostProcessPool { get; private set; }

        public void SetRemoteVstHostProcessPool(IRemoteVstHostProcessPool remoteVstHostProcessPool)
        {
            if (RemoteVstHostProcessPool != null)
            {
                throw new ArgumentException("Process pool already set, cannot set again");
            }

            RemoteVstHostProcessPool = remoteVstHostProcessPool;
        }

        public RuntimeConfiguration RuntimeConfiguration { get; set; }


        public string PresetMagicianVersion { get; } = VersionHelper.GetCurrentVersion();
    }
}