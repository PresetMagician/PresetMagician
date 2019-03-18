using System;
using System.Collections.Generic;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService
        )
            : base(Commands.Plugin.ScanSelectedPlugin, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService.SelectedPluginChanged += VstServiceOnSelectedPluginChanged;
        }

        private void VstServiceOnSelectedPluginChanged(object sender, EventArgs e)
        {
            InvalidateCommand();
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            if (_globalFrontendService.SelectedPlugin == null ||
                _globalFrontendService.SelectedPlugin.IsEnabled == false)
            {
                return new List<Plugin>();
            }

            return new List<Plugin> {_globalFrontendService.SelectedPlugin};
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                   _globalFrontendService.SelectedPlugin != null && _globalFrontendService.SelectedPlugin.IsEnabled;
        }
    }
}