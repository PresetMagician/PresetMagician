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
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService, IDatabaseService databaseService)
            : base(Commands.Plugin.ScanSelectedPlugin, commandManager, runtimeConfigurationService, vstService, applicationService, dispatcherService, databaseService)
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
            
            return new List<Plugin> { _vstService.SelectedPlugin};
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                   _vstService.SelectedPlugin != null && _vstService.SelectedPlugin.IsEnabled;
        }
    }
}