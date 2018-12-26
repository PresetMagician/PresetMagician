using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using MahApps.Metro.IconPacks;


namespace PresetMagician.ViewModels
{
    public class SettingsViewModel: ViewModelBase
    {
        public RuntimeConfiguration EditableConfiguration { get; private set; }

        private readonly IRuntimeConfigurationService _configurationService;
        private readonly ICommandManager _commandManager;

        public SettingsViewModel(
            IRuntimeConfigurationService configurationService,
            ICommandManager commandManager
        )
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => commandManager);

            _configurationService = configurationService;
            _commandManager = commandManager;
            Title = "Settings";
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
      
    }
}