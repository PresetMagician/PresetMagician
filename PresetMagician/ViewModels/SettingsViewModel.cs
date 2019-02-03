using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.Utils;
using Portable.Licensing;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public RuntimeConfiguration EditableConfiguration { get; private set; }

        private readonly IRuntimeConfigurationService _configurationService;
        private readonly ICommandManager _commandManager;
        private readonly ILicenseService _licenseService;
        private readonly IDatabaseService _databaseService;
        private readonly IUIVisualizerService _uiVisualizerService;
        public ApplicationState ApplicationState{ get; private set; }

        
        public string SelectedTabTitle { get; set; }
        public SettingsViewModel(
            IRuntimeConfigurationService configurationService,
            ILicenseService licenseService, IDatabaseService databaseService,
            IUIVisualizerService uiVisualizerService,
            ICommandManager commandManager
        )
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => databaseService);
            Argument.IsNotNull(() => uiVisualizerService);

            _configurationService = configurationService;
            _commandManager = commandManager;

            _databaseService = databaseService;
            _uiVisualizerService = uiVisualizerService;
            ApplicationState = configurationService.ApplicationState;
            
            CollectDatabaseStatistics = new TaskCommand(OnCollectDatabaseStatisticsExecute);
            OpenDatabaseLocation= new TaskCommand(OnOpenDatabaseLocationExecute);
            OpenVstWorkerLogDirectory = new TaskCommand(OnOpenVstWorkerLogDirectoryExecute);
            Title = "Settings";
        }

        public TaskCommand CollectDatabaseStatistics { get; set; }

        private async Task OnCollectDatabaseStatisticsExecute()
        {
            PresetDatabaseStatistics = await _databaseService.Context.GetPresetStatistics();
            TotalPresetsCompressedSize = (from p in PresetDatabaseStatistics select p.PresetCompressedSize).Sum();
            TotalPresetsUncompressedSize = (from p in PresetDatabaseStatistics select p.PresetUncompressedSize).Sum();
            TotalPresets = (from p in PresetDatabaseStatistics select p.PresetCount).Sum();
            SavedSpace = TotalPresetsUncompressedSize - TotalPresetsCompressedSize;
            SavedSpacePercent = 1-(double)TotalPresetsCompressedSize / TotalPresetsUncompressedSize;
        }
        
        public TaskCommand OpenVstWorkerLogDirectory { get; set; }

        private async Task OnOpenVstWorkerLogDirectoryExecute()
        {
            Process.Start(Path.GetDirectoryName(VstUtils.GetVstWorkerLogDirectory()));
        }
        
            
        public TaskCommand OpenDatabaseLocation { get; set; }

        private async Task OnOpenDatabaseLocationExecute()
        {

            Process.Start(Path.GetDirectoryName(ApplicationDatabaseContext.GetDatabasePath()));
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

        public IList<PresetDatabaseStatistics> PresetDatabaseStatistics { get; set; }
        public int TotalPresets { get; set; }
        public long TotalPresetsUncompressedSize { get; set; }
        public long TotalPresetsCompressedSize { get; set; }
        public long SavedSpace { get; set; }
        public double SavedSpacePercent { get; set; }

        public string DefaultNativeInstrumentsUserContentDirectory
        {
            get { return VstUtils.GetDefaultNativeInstrumentsUserContentDirectory(); }
        }
    }
}