using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel;
using Catel.Data;
using Catel.Fody;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Orc.FileSystem;
using PresetMagician.Models.ControllerAssignments;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstPluginViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IVstService _vstService;
        private readonly IOpenFileService _openFileService;
        private readonly ISelectDirectoryService _selectDirectoryService;
        
        public VstPluginViewModel(IVstService vstService, IOpenFileService openFileService, ISelectDirectoryService selectDirectoryService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => openFileService);
            Argument.IsNotNull(() => selectDirectoryService);
            
            _vstService = vstService;
            _vstService.SelectedPluginChanged += OnSelectedPluginChanged;
            
            Plugin = _vstService.SelectedPlugin;

            _openFileService = openFileService;
            _selectDirectoryService = selectDirectoryService;
            
            OpenNKSFFile = new TaskCommand(OnOpenNKSFFileExecute);
            ClearMappings = new TaskCommand(OnClearMappingsExecute);
            AddAdditionalPresetFiles = new TaskCommand(OnAddAdditionalPresetFilesExecute);
            AddAdditionalPresetFolder = new TaskCommand(OnAddAdditionalPresetFolderExecute);
            RemoveAdditionalBankFiles = new Command<object>(OnRemoveAdditionalBankFilesExecute);
        }
        
        public ObservableCollection<ControllerAssignmentPage> ControllerAssignmentPages { get; set; } = new ObservableCollection<ControllerAssignmentPage>();
        public int CurrentControllerAssignmentPage { get; set; }

        private void OnSelectedPluginChanged(object o, EventArgs e)
        {
            Plugin = _vstService.SelectedPlugin;
            RaisePropertyChanged(nameof(Plugin));
            GenerateControllerMappingModels();
            CurrentControllerAssignmentPage = 0;
        }

        public TaskCommand OpenNKSFFile { get; set; }

        private async Task OnOpenNKSFFileExecute()
        {
            try
            {
                _openFileService.Filter = "NKS Files (*.nksf,*.nksfx)|*.nksf;*.nksfx";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    ApplyControllerMapping(_openFileService.FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }
        
       
        
        
        private List<string> GetFiles (string path, List<string> patterns)
        {
            List<string> files = new List<string>();
            var directory = new DirectoryInfo(_selectDirectoryService.DirectoryName);
            
            foreach (var pattern in patterns)
            {
                var results = directory.EnumerateFiles(pattern, SearchOption.AllDirectories);

                foreach (var result in results)
                {
                    if (!files.Contains(result.FullName))
                    {
                        files.Add(result.FullName);
                    }
                }
            }

            return files;
        }        
        public TaskCommand AddAdditionalPresetFiles { get; set; }

        private async Task OnAddAdditionalPresetFilesExecute()
        {
            try
            {
                _openFileService.Filter = "Bank/Preset Files (*.fxb,*.fxp)|*.fxp;*.fxp";
                _openFileService.IsMultiSelect = true;

                if (await _openFileService.DetermineFileAsync())
                {
                    foreach (var filename in _openFileService.FileNames)
                    {
                        AddBankFile(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }
        
        public TaskCommand AddAdditionalPresetFolder { get; set; }

        private async Task OnAddAdditionalPresetFolderExecute()
        {
            
            try
            {
                if (await _selectDirectoryService.DetermineDirectoryAsync())
                {
                    var files = GetFiles(_selectDirectoryService.DirectoryName, new List<string> {"*.fxp", "*.fxb"});
                    
                    foreach (var filename in files)
                    {
                        AddBankFile(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        private void AddBankFile(string path)
        {
            if (!(from bankFile in Plugin.Configuration.AdditionalBankFiles where bankFile.Path == path select bankFile).Any())
            {
                string bankName;
                
                if (Path.GetExtension(path) == ".fxp")
                {
                    bankName = "User Presets";
                }
                else
                {
                    bankName = Path.GetFileNameWithoutExtension(path);
                }
                
                Plugin.Configuration.AdditionalBankFiles.Add(new BankFile { Path = path, BankName = bankName});
            } 
        }
        
        public Command<object> RemoveAdditionalBankFiles { get; set; }

        private void OnRemoveAdditionalBankFilesExecute(object parameter)
        {
            
            var folders = (parameter as IList).Cast<BankFile>();

            foreach (var folder in folders.ToList())
            {
                Plugin.Configuration.AdditionalBankFiles.Remove(folder);
            }
        }
        
        public TaskCommand ClearMappings { get; set; }

        private async Task OnClearMappingsExecute()
        {
            Plugin.Configuration.DefaultControllerAssignments = null;
            GenerateControllerMappingModels();
        }

        private void ApplyControllerMapping(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                NKSFRiff n = new NKSFRiff();
                n.Read(fileStream);

                Plugin.Configuration.DefaultControllerAssignments =
                    n.kontaktSound.controllerAssignments.controllerAssignments;
            }
            
            GenerateControllerMappingModels();
        }

        private void GenerateControllerMappingModels()
        {
            if (!IsPluginSet)
            {
                return;
            }
            
            var controllerAssignmentPages = new ObservableCollection<ControllerAssignmentPage>();

            if (Plugin.Configuration.DefaultControllerAssignments != null)
            {
                foreach (var page in Plugin.Configuration.DefaultControllerAssignments.controllerAssignments)
                {
                    var controllerAssignmentPage = new ControllerAssignmentPage();
                    List<string> titles = new List<string>();
                    foreach (var control in page)
                    {
                        var newControl = new ControllerAssignmentControl(control);
                        
                        if (control.section != null)
                        {
                            titles.Add(control.section);
                        }

                        if (controllerAssignmentPage.Controls.Count > 0 && controllerAssignmentPage.Controls.Last().section == null && control.section != null)
                        {

                            controllerAssignmentPage.Controls.Last().LastSectionItem = true;
                        }
                        controllerAssignmentPage.Controls.Add(newControl);
                    }

                    if (titles.Count == 0)
                    {
                        controllerAssignmentPage.Title =
                            $"Page {Plugin.Configuration.DefaultControllerAssignments.controllerAssignments.IndexOf(page)}";
                    }
                    else
                    {
                        controllerAssignmentPage.Title = String.Join(" / ", titles);
                    }
                    
                    

                    controllerAssignmentPages.Add(controllerAssignmentPage);
                }
            }

            ControllerAssignmentPages = controllerAssignmentPages;
            RaisePropertyChanged(nameof(ControllerAssignmentPages));
            CurrentControllerAssignmentPage = 0;
        }

        public bool IsPluginSet
        {
            get { return Plugin != null; }
        }

        public Models.Plugin Plugin { get; protected set; }
        
        
      
    }

}