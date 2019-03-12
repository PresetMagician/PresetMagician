using System.Runtime.Serialization;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Plugin;
using Newtonsoft.Json;

namespace PresetMagician.Legacy.Models
{
    [DataContract]
    public class VstPluginInfoSurrogate
    {
        public VstPluginInfoSurrogate()
        {
            
        }

        public VstPluginInfoSurrogate(VstPluginInfo vstPluginInfo)
        {
            FromNonSurrogate(vstPluginInfo);
        }
        /// <summary>
        /// Plugin flags.
        /// </summary>
        [DataMember]
        public string StringFlags {
            get { return JsonConvert.SerializeObject(Flags); }
            set
            {
                Flags = JsonConvert.DeserializeObject<VstPluginFlags>(value);
            }
            
        }

        public VstPluginFlags Flags { get; set; }

        /// <summary>
        /// The number of programs the plugin supports.
        /// </summary>
        [DataMember]
        public int ProgramCount { get; set; }

        /// <summary>
        /// The number of parameters the plugin supports.
        /// </summary>
        [DataMember]
        public int ParameterCount { get; set; }

        /// <summary>
        /// The number of audio inputs the plugin supports.
        /// </summary>
        [DataMember]
        public int AudioInputCount { get; set; }

        /// <summary>
        /// The number of audio outputs the plugin supports.
        /// </summary>
        [DataMember]
        public int AudioOutputCount { get; set; }

        /// <summary>
        /// The latency of the plugin audio processing.
        /// </summary>
        [DataMember]
        public int InitialDelay { get; set; }

        /// <summary>
        /// The unique ID of the plugin.
        /// </summary>
        /// <remarks>Must be a four character code.</remarks>
        [DataMember]
        public int PluginID { get; set; }

        /// <summary>
        /// The version of the plugin.
        /// </summary>
        [DataMember]
        public int PluginVersion { get; set; }

        public VstPluginInfo ToNonSurrogate()
        {
            var vstPluginInfo = new VstPluginInfo
            {
                Flags = Flags,
                ProgramCount = ProgramCount,
                ParameterCount = ParameterCount,
                AudioInputCount = AudioInputCount,
                AudioOutputCount = AudioOutputCount,
                InitialDelay = InitialDelay,
                PluginID = PluginID,
                PluginVersion = PluginVersion
            };

            return vstPluginInfo;
        }
        
        public void FromNonSurrogate(VstPluginInfo vstPluginInfo)
        {
            Flags = vstPluginInfo.Flags;
            PluginID = vstPluginInfo.PluginID;
            InitialDelay = vstPluginInfo.InitialDelay;
            ProgramCount = vstPluginInfo.ProgramCount;
            ParameterCount = vstPluginInfo.ParameterCount;
            PluginVersion = vstPluginInfo.PluginVersion;
            AudioInputCount = vstPluginInfo.AudioInputCount;
            AudioOutputCount = vstPluginInfo.AudioOutputCount;
        }
    }
}