using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Collections;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Exceptions;
using PresetMagician.NKS;
using PresetMagician.Utils;

namespace PresetMagician.Core.Models
{
    [DataContract]
    public class PresetExportInfo
    {
        public enum FolderExportMode
        {
            ONE_LEVEL_LAST_BANK,
            ONE_LEVEL_FIRST_BANK,
            SINGLE_FOLDER,
            SUBFOLDERS_TRIMMED,
            SUBFOLDERS
        }

        public enum FileOverwriteMode
        {
            FORCE_OVERWRITE,
            REPORT_ERROR,
            APPEND_GUID
        }

        public PresetExportInfo(Preset preset)
        {
            PluginName = preset.Plugin.PluginName;
            PluginVendor = preset.Plugin.PluginVendor;
            PluginId = preset.Plugin.VstPluginId;
            PluginType = preset.Plugin.PluginType;
            PresetGuid = Guid.Parse(preset.PresetId);
            if (preset.PresetBank == null)
            {
                throw new ArgumentException("PresetExportInfo: PresetBank is null. Please report this as a bug!");
            }

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
                if (!type.IsIgnored)
                {
                    Types.Add(new List<string> {type.EffectiveTypeName, type.EffectiveSubTypeName});
                }
            }

            foreach (var mode in preset.Metadata.Characteristics)
            {
                if (!mode.IsIgnored)
                {
                    Modes.Add(mode.EffectiveCharacteristicName);
                }
            }
        }

        [DataMember] public string PluginName { get; set; }
        [DataMember] public string PluginVendor { get; set; }
        [DataMember] public int PluginId { get; set; }
        [DataMember] public Plugin.PluginTypes PluginType { get; set; }
        [DataMember] public List<string> BankPath { get; set; }
        [DataMember] public string BankName { get; set; }
        [DataMember] public string PresetName { get; set; }
        [DataMember] public Guid PresetGuid { get; set; }
        [DataMember] public PreviewNotePlayer PreviewNotePlayer { get; set; }
        [DataMember] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [DataMember] public string Author { get; set; }
        [DataMember] public string Comment { get; set; }
        [DataMember] public FolderExportMode FolderMode { get; set; } = FolderExportMode.SUBFOLDERS_TRIMMED;
        [DataMember] public FileOverwriteMode OverwriteMode { get; set; } = FileOverwriteMode.REPORT_ERROR;
        [DataMember] public string UserContentDirectory { get; set; }

        public bool CanExport()
        {
            ComputeOutputFilename();
            return _canExport;
        }
        
        public string CannotExportReason { get; private set; }= "";
        private bool _canExport = true;
        private string _outputFilename;

        [DataMember]
        public List<List<string>> Types { get; set; } =
            new List<List<string>>();

        [DataMember] public List<string> Modes { get; set; } = new List<string>();

        private List<string> GetBankPath()
        {
            switch (FolderMode)
            {
                case FolderExportMode.SINGLE_FOLDER:
                    return new List<string>();
                case FolderExportMode.ONE_LEVEL_LAST_BANK:
                    return new List<string> {BankPath.Last()};
                case FolderExportMode.ONE_LEVEL_FIRST_BANK:
                    return new List<string> {BankPath.First()};
                case FolderExportMode.SUBFOLDERS_TRIMMED:
                    var bp = new List<string>(BankPath.ToList());


                    if (bp.Count > 1)
                    {
                        var d = BankPath.GetRange(1, BankPath.Count - 1);
                        var lastBankPath = string.Join(" - ", d);

                        bp.RemoveRange(1, BankPath.Count - 1);
                        bp.Add(lastBankPath);
                    }

                    return bp;
                case FolderExportMode.SUBFOLDERS:
                    return BankPath.ToList();
            }

            return null;
        }

        public string GetFullOutputPath()
        {
            return Path.Combine(GetUserContentDirectory(), GetOutputFilename());
        }

        public void ComputeOutputFilename()
        {
            if (_outputFilename != null)
            {
                return;
            }
            string fileExtension;

            if (PluginType == Plugin.PluginTypes.Instrument)
            {
                fileExtension = ".nksf";
            }
            else if (PluginType == Plugin.PluginTypes.Effect)
            {
                fileExtension = ".nksfx";
            }
            else
            {
                throw new ArgumentException("Unknown device type");
            }

            var fileName = GetNKSFPresetName(PresetName) + fileExtension;
            var filePath = Path.Combine(GetUserContentDirectory(), fileName);

            if (File.Exists(filePath))
            {
                Guid existingGuid;
                switch (OverwriteMode)
                {
                    case FileOverwriteMode.FORCE_OVERWRITE:
                        break;
                    case FileOverwriteMode.APPEND_GUID:
                        existingGuid = NKSFRiff.GetPresetGuid(filePath);


                        if (existingGuid != PresetGuid)
                        {
                            fileName = GetNKSFPresetName(PresetName, true) + fileExtension;
                        }

                        break;
                    case FileOverwriteMode.REPORT_ERROR:
                        existingGuid = NKSFRiff.GetPresetGuid(filePath);


                        if (existingGuid != PresetGuid)
                        {
                            _canExport = false;
                            CannotExportReason = $"File {filePath} already exists and has a different preset ID";
                        }


                        break;
                }
            }

            _outputFilename = fileName;
        }
        public string GetOutputFilename()
        {
            ComputeOutputFilename();
            return _outputFilename;
        }

        private string GetUserContentDirectory()
        {
            string userContentDirectory;
            if (!Directory.Exists(UserContentDirectory))
            {
                userContentDirectory = VstUtils.GetDefaultNativeInstrumentsUserContentDirectory();
            }
            else
            {
                userContentDirectory = UserContentDirectory;
            }

            var bankDirectory = Path.Combine(userContentDirectory, GetNKSFPluginName(PluginName),
                GetNKSFBankName());

            return bankDirectory;
        }


        private string GetNKSFBankName()
        {
            var bp = GetBankPath();
            var sanitizedBankPath = new List<string>();
            foreach (var bankDir in bp)
            {
                var sanitizedBankDir = PathUtils.SanitizeDirectory(bankDir);
                sanitizedBankDir = PathUtils.SanitizeFilename(sanitizedBankDir);

                sanitizedBankDir = sanitizedBankDir.Replace("/", "-");
                sanitizedBankPath.Add(sanitizedBankDir);
            }
            
            return string.Join(@"\", sanitizedBankPath);
           
        }

        private string GetNKSFPluginName(string pluginName)
        {
            pluginName = PathUtils.SanitizeDirectory(pluginName);
            pluginName = PathUtils.SanitizeFilename(pluginName);

            return pluginName;
        }

        private string GetNKSFPresetName(string presetName, bool withGuid = false)
        {
            // Returns the sanitized preset name

            presetName = PathUtils.SanitizeDirectory(presetName);
            presetName = PathUtils.SanitizeFilename(presetName);

            if (withGuid)
            {
                presetName = presetName + "." + PresetGuid;
            }

            return presetName;
        }

        public string GetPreviewFilename(bool wav = false)
        {
            var bankDirectory = GetUserContentDirectory();

            var previewDirectory = Path.Combine(bankDirectory, ".previews");


            var extension = ".ogg";

            if (wav)
            {
                extension = ".wav";
            }

            return Path.Combine(previewDirectory,
                GetOutputFilename() + extension);
        }
    }
}