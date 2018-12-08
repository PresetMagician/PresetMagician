using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Helpers;
using PresetMagicianShell.Models.Settings;
using PresetMagicianShell.Services.Interfaces;
using ApplicationSettings = PresetMagicianShell.Settings.Application;

namespace PresetMagicianShell.ViewModels
{
    public class VstFolderListViewModel : ViewModelBase
    {
        private readonly IRuntimeConfigurationService _configurationService;
        private readonly ISelectDirectoryService _selectDirectoryService;
        private readonly IVstService _vstService;

        public VstFolderListViewModel(IRuntimeConfigurationService configurationService,
            ISelectDirectoryService selectDirectoryService, IVstService vstService)
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => selectDirectoryService);
            Argument.IsNotNull(() => vstService);

            _configurationService = configurationService;
            _selectDirectoryService = selectDirectoryService;
            _vstService = vstService;
            

            AddDefaultVstFolders = new Command(OnAddDefaultVstFoldersExecute);
            AddFolder = new TaskCommand(OnAddFolderExecute);
            RemoveFolder = new Command<object>(OnRemoveFolderExecute);

            VstDirectories = configurationService.RuntimeConfiguration.VstDirectories;
        }


        public override string Title { get; protected set; } = "VST Directories";

        public ObservableCollection<VstDirectory> VstDirectories { get; set; }

        #region Commands

        public Command AddDefaultVstFolders { get; }

        private void OnAddDefaultVstFoldersExecute()
        {
            foreach (var i in VstPathScanner.getCommonVSTPluginDirectories())
            {
                if (!(from path in VstDirectories where path.Path == i select path).Any())
                {
                    VstDirectories.Add(new VstDirectory() { Path = i });
                }
            }
            _vstService.RefreshPluginList();
        }

        public TaskCommand AddFolder { get; }

        private async Task OnAddFolderExecute()
        {
            if (await _selectDirectoryService.DetermineDirectoryAsync())
            {
                VstDirectories.Add(new VstDirectory() { Path = _selectDirectoryService.DirectoryName });
            }

            _vstService.RefreshPluginList();
        }

        public Command<object> RemoveFolder { get; }

        private void OnRemoveFolderExecute(object parameter)
        {
            var folders = (parameter as IList).Cast<VstDirectory>();

            foreach (var folder in folders.ToList())
            {
                VstDirectories.Remove(folder);
            }
            _vstService.RefreshPluginList();
        }

        #endregion
    }
}