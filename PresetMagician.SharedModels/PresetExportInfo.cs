using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
using Drachenkatze.PresetMagician.NKSF.NKSF;

namespace SharedModels
{
    [DataContract]
    public class PresetExportInfo
    {
        public PresetExportInfo(Preset preset)
        {
            PluginName = preset.Plugin.PluginName;
            PluginVendor = preset.Plugin.PluginVendor;
            PluginId = preset.Plugin.PluginId;
            PluginType = preset.Plugin.PluginType;
            BankPath = preset.PresetBank.GetBankPath().ToList();
            BankPath.RemoveFirst();
            BankName = preset.PresetBank.BankName;
            PresetName = preset.PresetName;
            PreviewNoteNumber = preset.PreviewNoteNumber;
            DefaultControllerAssignments = preset.Plugin.DefaultControllerAssignments;
            Author = preset.Author;
            Comment = preset.Comment;
            Types = preset.Types;
            Modes = preset.Modes;
        }

        [DataMember] public string PluginName { get; set; }
        [DataMember] public string PluginVendor { get; set; }
        [DataMember] public int PluginId { get; set; }
        [DataMember] public Plugin.PluginTypes PluginType { get; set; }
        [DataMember] public List<string> BankPath { get; set; }
        [DataMember] public string BankName { get; set; }
        [DataMember] public string PresetName { get; set; }
        [DataMember] public int PreviewNoteNumber { get; set; }
        [DataMember] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [DataMember] public string Author { get; set; }
        [DataMember] public string Comment { get; set; }

        [DataMember]
        public ObservableCollection<ObservableCollection<string>> Types { get; set; } =
            new ObservableCollection<ObservableCollection<string>>();

        [DataMember] public ObservableCollection<string> Modes { get; set; } = new ObservableCollection<string>();
    }
}