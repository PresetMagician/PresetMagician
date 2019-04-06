using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public RuntimeConfiguration EditableConfiguration { get; private set; }

        private readonly IRuntimeConfigurationService _configurationService;
        private readonly ICommandManager _commandManager;
        private readonly GlobalService _globalService;
        private readonly ILicenseService _licenseService;
        private readonly IAdvancedMessageService _advancedMessageService;
        public ApplicationState ApplicationState { get; private set; }


        public string SelectedTabTitle { get; set; }

        public SettingsViewModel(
            IRuntimeConfigurationService configurationService,
            ILicenseService licenseService,
            ICommandManager commandManager, DataPersisterService dataPersisterService,
            GlobalFrontendService globalFrontendService, GlobalService globalService,
            IAdvancedMessageService advancedMessageService
        )
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => licenseService);

            _configurationService = configurationService;
            _commandManager = commandManager;
            _globalService = globalService;
            _advancedMessageService = advancedMessageService;

            ApplicationState = globalFrontendService.ApplicationState;

            OpenVstWorkerLogDirectory = new Command(OnOpenVstWorkerLogDirectoryExecute);
            OpenDataDirectory = new Command(OnOpenDataDirectoryExecute);
            Title = "Settings";

            PresetDatabaseStatistics = dataPersisterService.GetStorageStatistics();
            
            TotalPresets = (from p in PresetDatabaseStatistics select p.PresetCount).Sum();
            TotalPresetsUncompressedSize = (from p in PresetDatabaseStatistics select p.PresetUncompressedSize).Sum();
            TotalPresetsCompressedSize = (from p in PresetDatabaseStatistics select p.PresetCompressedSize).Sum();
            SavedSpace = (from p in PresetDatabaseStatistics select p.SavedSpace).Sum();
            SavedSpacePercent = (double) TotalPresetsCompressedSize / TotalPresetsUncompressedSize;
        }


        public Command OpenVstWorkerLogDirectory { get;  }

        private void OnOpenVstWorkerLogDirectoryExecute()
        {
            Process.Start(Path.GetDirectoryName(VstUtils.GetVstWorkerLogDirectory()));
        }
        
        public Command OpenDataDirectory { get;  }

        private void OnOpenDataDirectoryExecute()
        {
            Process.Start(DataPersisterService.DefaultDataStoragePath);
        }


        protected override async Task InitializeAsync()
        {
            EditableConfiguration = _configurationService.EditableConfiguration;
        }

        protected override async Task<bool> SaveAsync()
        {
            if (EditableConfiguration.FolderExportMode != _globalService.RuntimeConfiguration.FolderExportMode ||
                EditableConfiguration.FileOverwriteMode != _globalService.RuntimeConfiguration.FileOverwriteMode)
            {
                await _advancedMessageService.ShowAsyncWithDontShowAgain(
                    "You have changed the folder export mode and/or the file overwrite mode. Note that PresetMagician " +
                    "does not move old files - please backup/clean up old bank folders to ensure no NKS files are " +
                    "accidentally deleted or duplicated.", "ChangedFolderExportFileOverwriteMode",
                    "Changed Folder Export / File Overwrite Mode", null, MessageImage.Information);
            }
            _commandManager.ExecuteCommand("Application.ApplyConfiguration");

            return await base.SaveAsync();
        }

        public int TotalPresets { get; set; }
        public long TotalPresetsUncompressedSize { get; set; }
        public long TotalPresetsCompressedSize { get; set; }
        public double SavedSpace { get; set; }
        public double SavedSpacePercent { get; set; }
        public List<PresetDatabaseStatistic> PresetDatabaseStatistics { get; }
        

        public string DefaultNativeInstrumentsUserContentDirectory
        {
            get { return VstUtils.GetDefaultNativeInstrumentsUserContentDirectory(); }
        }
    }
}