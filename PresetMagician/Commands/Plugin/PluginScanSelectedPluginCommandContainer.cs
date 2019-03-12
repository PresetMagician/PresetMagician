using System;
using System.Collections.Generic;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService,
            IAdvancedMessageService messageService,
            INativeInstrumentsResourceGeneratorService resourceGeneratorService, PresetDataPersisterService presetDataPersisterService)
            : base(Commands.Plugin.ScanSelectedPlugin, commandManager, runtimeConfigurationService, vstService,
                applicationService, dispatcherService, messageService, resourceGeneratorService, presetDataPersisterService)
        {
            vstService.SelectedPluginChanged += VstServiceOnSelectedPluginChanged;
        }

        private void VstServiceOnSelectedPluginChanged(object sender, EventArgs e)
        {
            InvalidateCommand();
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            if (_vstService.SelectedPlugin == null || _vstService.SelectedPlugin.IsEnabled == false)
            {
                return new List<Plugin>();
            }

            return new List<Plugin> {_vstService.SelectedPlugin};
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                   _vstService.SelectedPlugin != null && _vstService.SelectedPlugin.IsEnabled;
        }
    }
}