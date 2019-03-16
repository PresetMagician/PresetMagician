using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
using Drachenkatze.PresetMagician.NKSF.NKSF;

namespace PresetMagician.Core.Models
{
    [DataContract]
    public class PresetExportInfo
    {
        public PresetExportInfo(Preset preset)
        {
            PluginName = preset.Plugin.PluginName;
            PluginVendor = preset.Plugin.PluginVendor;
            PluginId = preset.Plugin.VstPluginId;
            PluginType = preset.Plugin.PluginType;
            BankPath = preset.PresetBank.GetBankPath().ToList();
            BankPath.RemoveFirst();
            BankName = preset.PresetBank.BankName;
            PresetName = preset.Metadata.PresetName;
            PreviewNotePlayer = preset.PreviewNotePlayer;
            DefaultControllerAssignments = preset.Plugin.DefaultControllerAssignments;
            Author = preset.Metadata.Author;
            Comment = preset.Metadata.Comment;


            foreach (var type in preset.Metadata.Types)
            {
                Types.Add(new List<string> {type.EffectiveTypeName, type.EffectiveSubTypeName});
            }

            foreach (var mode in preset.Metadata.Characteristics)
            {
                Modes.Add(mode.EffectiveCharacteristicName);
            }
        }

        [DataMember] public string PluginName { get; set; }
        [DataMember] public string PluginVendor { get; set; }
        [DataMember] public int PluginId { get; set; }
        [DataMember] public Plugin.PluginTypes PluginType { get; set; }
        [DataMember] public List<string> BankPath { get; set; }
        [DataMember] public string BankName { get; set; }
        [DataMember] public string PresetName { get; set; }
        [DataMember] public PreviewNotePlayer PreviewNotePlayer { get; set; }
        [DataMember] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [DataMember] public string Author { get; set; }
        [DataMember] public string Comment { get; set; }

        [DataMember]
        public List<List<string>> Types { get; set; } =
            new List<List<string>>();

        [DataMember] public List<string> Modes { get; set; } = new List<string>();
    }
}