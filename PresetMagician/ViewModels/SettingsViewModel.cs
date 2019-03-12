using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public RuntimeConfiguration EditableConfiguration { get; private set; }

        private readonly IRuntimeConfigurationService _configurationService;
        private readonly ICommandManager _commandManager;
        private readonly ILicenseService _licenseService;
        private readonly IUIVisualizerService _uiVisualizerService;
        public ApplicationState ApplicationState{ get; private set; }

        
        public string SelectedTabTitle { get; set; }
        public SettingsViewModel(
            IRuntimeConfigurationService configurationService,
            ILicenseService licenseService,
            IUIVisualizerService uiVisualizerService,
            ICommandManager commandManager
        )
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => uiVisualizerService);

            _configurationService = configurationService;
            _commandManager = commandManager;

            _uiVisualizerService = uiVisualizerService;
            ApplicationState = configurationService.ApplicationState;
            
            OpenVstWorkerLogDirectory = new TaskCommand(OnOpenVstWorkerLogDirectoryExecute);
            Title = "Settings";
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
        public long SavedSpace { get; set; }
        public double SavedSpacePercent { get; set; }

        public string DefaultNativeInstrumentsUserContentDirectory
        {
            get { return VstUtils.GetDefaultNativeInstrumentsUserContentDirectory(); }
        }
    }
}