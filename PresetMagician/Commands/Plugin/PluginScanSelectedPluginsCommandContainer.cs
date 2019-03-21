using System.Collections.Generic;
using System.Linq;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.Plugin.ScanSelectedPlugins, commandManager, serviceLocator)
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