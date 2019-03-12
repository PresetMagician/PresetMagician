using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PresetMagician.Legacy.Models
{
    public class Preset 
    {
        public Preset()
        {
        }


        

        #region Properties

        #region Basic Properties

        /// <summary>
        /// The PresetId. Always a new GUID
        /// </summary>
        [Key]
        public string PresetId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The Vst Plugin Id, which is required in case we lose the association with the plugin and might need to
        /// "repair" the database manually.
        /// </summary>
        public int VstPluginId { get; private set; }

        /// <summary>
        /// If the preset is ignored, it will never be updated by a preset parser and will never be exported. Useful
        /// if a plugin reports empty or nonsense presets.
        /// </summary>
        public bool IsIgnored { get; set; }

        #endregion

        #region Plugin

        /// <summary>
        /// The PluginId as foreign key
        /// </summary>
        [ForeignKey("Plugin")]
        [Index("UniquePreset", IsUnique = true)]
        public int PluginId { get; set; }

        /// <summary>
        /// The plugin this preset belongs to. As soon as the plugin is set, we fill the plugins bank structure
        /// with the string representation of the bank path 
        /// </summary>
        public Plugin Plugin { get; set; }
      

        #endregion

        

        #region Metadata properties

        /// <summary>
        /// The name of the preset
        /// </summary>
        public string PresetName { get; set; }

        /// <summary>
        /// The author of the preset
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The preset comment
        /// </summary>
        public string Comment { get; set; }


        /// <summary>
        /// The bank path. Only set via EntityFramework when loading from the database
        /// </summary>
        public string BankPath { get; set; }
       
        public ICollection<Type> Types { get; set; } = new List<Type>();
        public ICollection<Mode> Modes { get; set; }= new List<Mode>();

        #endregion

        #region Preset Data Properties

        /// <summary>
        /// The SourceFile identifies where the preset came from. Usually a filename, but can be any string, depending
        /// on how the plugin stores it's presets
        /// </summary>
        [Index("UniquePreset", IsUnique = true)]
        public string SourceFile { get; set; }

        /// <summary>
        /// The preset size. Mainly used for statistics
        /// </summary>
        public int PresetSize { get; set; }

        /// <summary>
        /// The compressed preset size. Mainly used for statistics
        /// </summary>
        public int PresetCompressedSize { get; set; }

        /// <summary>
        /// The hash of the preset data. For memory usage reasons, we keep the preset data in a separate table so we
        /// can load all presets for a plugin and have a very low memory footprint. 
        /// </summary>
        public string PresetHash { get; set; }

        /// <summary>
        /// The preset hash we last exported. This is required because the preset data could change, but still be the
        /// same size. 
        /// </summary>
        public string LastExportedPresetHash { get; set; }

        /// <summary>
        /// The date and time when the preset was last exported. Mainly informational.
        /// </summary>
        public DateTime? LastExported { get; set; }

        /// <summary>
        /// If the metadata has been modified. Used in conjunction with the modified preset data to determinate if the
        /// preset should be exported again
        /// </summary>
        public bool IsMetadataModified { get; set; }

        #endregion


        public int PreviewNoteNumber { get; set; }
      

        #endregion

        #region Change Tracking

        public List<string> UserOverwrittenProperties = new List<string>();
        /// <summary>
        /// Stores all properties which the user has manually modified. These properties will never be updated by a preset parser
        /// </summary>
        [Column("UserModifiedMetadata")]
        // ReSharper disable once UnusedMember.Global
        public string SerializedUserModifiedMetadata
        {
            get => JsonConvert.SerializeObject(UserOverwrittenProperties);
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        UserOverwrittenProperties = JsonConvert.DeserializeObject<List<string>>(value);
                    }
                    catch (JsonReaderException e)
                    {
                       
                    }
                }
            }
        }


        #endregion

    }
}