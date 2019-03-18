using System.Collections.Generic;
using System.Linq;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Plugin.ScanSelectedPlugins, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService.SelectedPlugins.CollectionChanged += OnPluginsListChanged;
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in _globalFrontendService.SelectedPlugins where plugin.IsEnabled select plugin)
                .ToList();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                   _globalFrontendService.SelectedPlugins.Count > 0;
        }
    }
}