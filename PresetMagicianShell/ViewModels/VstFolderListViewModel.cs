using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Configuration;
using Catel.MVVM;
using Drachenkatze.PresetMagician.VSTHost.VST;
using ApplicationSettings = PresetMagicianShell.Settings.Application;

namespace PresetMagicianShell.ViewModels
{
    public class VstFolderListViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configurationService;
        
        public VstFolderListViewModel(IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => configurationService);
       
            _configurationService = configurationService;
           
            Title = "VST Directories";
            
            AddDefaultVstFolders = new TaskCommand(OnAddDefaultVstFoldersExecute);
            AddFolder = new TaskCommand(OnAddFolderExecute);
            RemoveFolder = new TaskCommand(OnRemoveFolderExecute);
        }

        public ObservableCollection<DirectoryInfo> VstDirectories { get; set; } = new ObservableCollection<DirectoryInfo>();

        #region Commands

        public TaskCommand AddDefaultVstFolders { get; private set; }

        private async Task OnAddDefaultVstFoldersExecute()
        {
            foreach (DirectoryInfo i in VstPathScanner.getCommonVSTPluginDirectories())
            {
                if (!(from path in VstDirectories where path.FullName == i.FullName select path.FullName).Any())
                {
                    VstDirectories.Add(i);
                }
            }
        }
        
        public TaskCommand AddFolder { get; private set; }

        private async Task OnAddFolderExecute ()
        {
           
        }
        
        public TaskCommand RemoveFolder { get; private set; }

        private async Task OnRemoveFolderExecute ()
        {
           
        }
        
        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            VstDirectories = _configurationService.GetLocalValue(ApplicationSettings.Directories.VstDirectories, new ObservableCollection<DirectoryInfo>());
        }
        
        protected override async Task<bool> SaveAsync()
        {
            _configurationService.SetRoamingValue(ApplicationSettings.Directories.VstDirectories, VstDirectories);

            return await base.SaveAsync();
        }
        
        #endregion
    }
}