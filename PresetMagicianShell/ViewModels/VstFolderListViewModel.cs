using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Services.Interfaces;
using ApplicationSettings = PresetMagicianShell.Settings.Application;

namespace PresetMagicianShell.ViewModels
{
    public class VstFolderListViewModel : ViewModelBase
    {
        private readonly IRuntimeConfigurationService _configurationService;
        private readonly ISelectDirectoryService _selectDirectoryService;

        public ListCollectionView ListCollectionView { get; set; }


        public VstFolderListViewModel(IRuntimeConfigurationService configurationService, ISelectDirectoryService selectDirectoryService)
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => selectDirectoryService);
       
            _configurationService = configurationService;
            _selectDirectoryService = selectDirectoryService;
           
            AddDefaultVstFolders = new Command(OnAddDefaultVstFoldersExecute);
            AddFolder = new TaskCommand(OnAddFolderExecute);
            RemoveFolder = new Command<object>(OnRemoveFolderExecute);
        }


        public override string Title { get; protected set; } = "VST Directories";
        
        public ObservableCollection<string> VstDirectories { get; set; }

        #region Commands

        public Command AddDefaultVstFolders { get; }

        private void OnAddDefaultVstFoldersExecute()
        {
            foreach (var i in VstPathScanner.getCommonVSTPluginDirectories())
            {
                if (!(from path in VstDirectories where path == i select path).Any())
                {
                    VstDirectories.Add(i);
                }
            }
            
            _configurationService.SaveConfiguration();
        }
        
        public TaskCommand AddFolder { get; }

        private async Task OnAddFolderExecute ()
        {
            if (await _selectDirectoryService.DetermineDirectoryAsync())
            {
                VstDirectories.Add(_selectDirectoryService.DirectoryName);
            }
        }
        
        public Command<object> RemoveFolder { get; }

        private void OnRemoveFolderExecute (object parameter)
        {

            var folders = (parameter as IList).Cast<string>();

            foreach (var folder in folders.ToList())
            {
                VstDirectories.Remove(folder);
            }
        }
        
        #endregion
    }
}