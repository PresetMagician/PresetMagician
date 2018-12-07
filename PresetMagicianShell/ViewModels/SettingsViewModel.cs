using Catel;
using Catel.MVVM;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.ViewModels
{
    public class SettingsViewModel: ViewModelBase
    {
        public RuntimeConfiguration RuntimeConfiguration { get; private set; }

        public SettingsViewModel(
            IRuntimeConfigurationService configurationService
        )
        {
            Argument.IsNotNull(() => configurationService);

            RuntimeConfiguration = configurationService.RuntimeConfiguration;
        }

    }
}