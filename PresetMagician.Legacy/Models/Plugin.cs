using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Catel.Collections;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Newtonsoft.Json;

namespace PresetMagician.Legacy.Models
{
    public class Plugin
    {
        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

        #region Methods

        public Plugin()
        {
        }

        #endregion


        #region Properties

        #region Basic Properties

        /// <summary>
        /// The plugin ID for storage in the database
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Defines if the current plugin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// The Vst Plugin Id, which is used when matching against PluginLocations
        /// </summary>
        public int VstPluginId { get; set; }

        /// <summary>
        /// The type of the plugin
        /// </summary>
        public PluginTypes PluginType { get; set; } = PluginTypes.Unknown;

        #endregion

        #region Plugin Errors

        #endregion

        #region Property PluginCapabilities

        /// <summary>
        /// The plugin capabilities
        /// </summary>
        [NotMapped]
        public virtual IList<PluginInfoItem> PluginCapabilities { get; } = new List<PluginInfoItem>();

        /// <summary>
        /// These are the plugin capabilities which will get serialized into the database
        /// todo check if this can be refactored to use EF6 builtin serializer only and get rid of that serializer stuff
        /// </summary>
        [Column("PluginCapabilities")]
        public string SerializedPluginCapabilities
        {
            get => JsonConvert.SerializeObject(PluginCapabilities);
            set
            {
                try
                {
                    var capabilities = JsonConvert.DeserializeObject<List<PluginInfoItem>>(value);
                    PluginCapabilities.Clear();
                    PluginCapabilities.AddRange(capabilities);
                }
                catch (JsonReaderException e)
                {
                }
            }
        }

        #endregion

        #region Property Plugin Location

        private PluginLocation _pluginLocation;

        /// <summary>
        /// The plugin location to use. Attach event listeners if being set
        /// </summary>
        public PluginLocation PluginLocation { get; set; }

        #endregion

        #region Plugin Info

        [NotMapped] public VstPluginInfoSurrogate PluginInfo { get; set; }

        /// <summary>
        /// These are the plugin infos which will get serialized into the database
        /// todo check if this can be refactored to use EF6 builtin serializer only and get rid of that serializer stuff
        /// </summary>
        [Column("PluginInfo")]
        public string SerializedPluginInfo
        {
            get => JsonConvert.SerializeObject(PluginInfo);
            set
            {
                try
                {
                    PluginInfo = JsonConvert.DeserializeObject<VstPluginInfoSurrogate>(value);
                }
                catch (JsonReaderException e)
                {
                }
            }
        }

        #endregion


        public List<Preset> Presets { get; set; } =
            new List<Preset>();


        public string LastKnownGoodDllPath { get; set; }


        public int AudioPreviewPreDelay { get; set; }

        [NotMapped] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [Column("DefaultControllerAssignments")]
        // ReSharper disable once UnusedMember.Global
        public string SerializedDefaultControllerAssignments
        {
            get => JsonConvert.SerializeObject(DefaultControllerAssignments);
            set => DefaultControllerAssignments = JsonConvert.DeserializeObject<ControllerAssignments>(value);
        }

        public bool IsReported { get; set; }
        public bool DontReport { get; set; }

        public ICollection<BankFile> AdditionalBankFiles { get; set; } = new List<BankFile>();

        public ICollection<Type> DefaultTypes { get; set; } = new HashSet<Type>();

        public ICollection<Mode> DefaultModes { get; set; } = new HashSet<Mode>();


        public string PluginName { get; set; } = "<unknown>";

        public string PluginVendor { get; set; }


        /// <summary>
        /// Defines if the plugin is or was scanned
        /// </summary>
        ///
        public bool IsAnalyzed { get; set; }

        public bool HasMetadata { get; set; }

        /// <summary>
        /// Defines if the plugin is supported
        /// </summary>
        public bool IsSupported { get; set; }

        #endregion
    }
}