using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService)
            : base(Commands.Plugin.ScanSelectedPlugins, commandManager, runtimeConfigurationService, vstService, applicationService, dispatcherService)
        {
            vstService.SelectedPlugins.CollectionChanged += OnPluginsListChanged;
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in _vstService.SelectedPlugins where plugin.Configuration.IsEnabled select plugin).ToList();
        }
        
     

        protected override bool CanExecute(object parameter)
        {
            Debug.WriteLine(_vstService.SelectedPlugins.Count);
            return base.CanExecute(parameter) &&
                   _vstService.SelectedPlugins.Count > 0;
        }
    }
}