using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public class PluginScanPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService)
            : base(Commands.Plugin.ScanPlugins, commandManager, runtimeConfigurationService, vstService, applicationService, dispatcherService)
        {
           
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in _vstService.Plugins where plugin.Configuration.IsEnabled select plugin).ToList();
        }
    }
}