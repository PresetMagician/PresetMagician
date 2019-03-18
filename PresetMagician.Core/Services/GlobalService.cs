using Catel.Collections;
using Orchestra;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public class GlobalService
    {
        public GlobalService()
        {
            GlobalCharacteristics = Characteristic.GlobalCharacteristics;
            GlobalTypes = Type.GlobalTypes;
        }

        public FastObservableCollection<Plugin> Plugins { get; } = new FastObservableCollection<Plugin>();
        public GlobalCharacteristicCollection GlobalCharacteristics { get; }
        public GlobalTypeCollection GlobalTypes { get; }
        
        public IRemoteVstHostProcessPool RemoteVstHostProcessPool { get; set; }
        
        public string PresetMagicianVersion { get; } = VersionHelper.GetCurrentVersion();
    }
}