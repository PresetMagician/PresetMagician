using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
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
        private readonly ILicenseService _licenseService;
        public ApplicationState ApplicationState { get; private set; }


        public string SelectedTabTitle { get; set; }

        public SettingsViewModel(
            IRuntimeConfigurationService configurationService,
            ILicenseService licenseService,
            ICommandManager commandManager, DataPersisterService dataPersisterService,
            GlobalFrontendService globalFrontendService
        )
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => licenseService);

            _configurationService = configurationService;
            _commandManager = commandManager;

            ApplicationState = globalFrontendService.ApplicationState;

            OpenVstWorkerLogDirectory = new TaskCommand(OnOpenVstWorkerLogDirectoryExecute);
            Title = "Settings";

            PresetDatabaseStatistics = dataPersisterService.GetStorageStatistics();
            TotalPresets = (from p in PresetDatabaseStatistics select p.PresetCount).Sum();
            TotalPresetsUncompressedSize = (from p in PresetDatabaseStatistics select p.PresetUncompressedSize).Sum();
            TotalPresetsCompressedSize = (from p in PresetDatabaseStatistics select p.PresetCompressedSize).Sum();
            SavedSpace = (from p in PresetDatabaseStatistics select p.SavedSpace).Sum();
            SavedSpacePercent = (double) TotalPresetsCompressedSize / TotalPresetsUncompressedSize;
        }


        public TaskCommand OpenVstWorkerLogDirectory { get; set; }

        private async Task OnOpenVstWorkerLogDirectoryExecute()
        {
            Process.Start(Path.GetDirectoryName(VstUtils.GetVstWorkerLogDirectory()));
        }


        protected override async Task InitializeAsync()
        {
            EditableConfiguration = _configurationService.EditableConfiguration;
        }

        protected override async Task<bool> SaveAsync()
        {
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