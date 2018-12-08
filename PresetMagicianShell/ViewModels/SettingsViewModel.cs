using Catel;
using Catel.MVVM;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using MahApps.Metro.IconPacks;


namespace PresetMagicianShell.ViewModels
{
    public class SettingsViewModel: PaneViewModel
    {
        public RuntimeConfiguration RuntimeConfiguration { get; private set; }

        public SettingsViewModel(
            IRuntimeConfigurationService configurationService
        )
        {
            Argument.IsNotNull(() => configurationService);

            RuntimeConfiguration = configurationService.RuntimeConfiguration;
            Title = "Settings";
        }

    }
}