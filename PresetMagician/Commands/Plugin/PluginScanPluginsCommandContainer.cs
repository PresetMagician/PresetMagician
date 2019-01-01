using System;
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
    public class PluginScanPluginsCommandContainer : CommandContainerBase
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();

        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;
        private readonly IApplicationService _applicationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly ICommandManager _commandManager;

        public PluginScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService)
            : base(Commands.Plugin.ScanPlugins, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);

            _runtimeConfigurationService = runtimeConfigurationService;
            _vstService = vstService;
            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _commandManager = commandManager;

            _vstService.Plugins.CollectionChanged += OnPluginsListChanged;
            _runtimeConfigurationService.ApplicationState.PropertyChanged += OnAllowPluginScanChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return _vstService.Plugins.Count > 0 &&
                   _runtimeConfigurationService.ApplicationState.AllowPluginScan;
        }

        private void OnAllowPluginScanChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.AllowPluginScan)) InvalidateCommand();
        }

        private void OnPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToScan = (from plugin in _vstService.Plugins where plugin.Enabled select plugin).ToList();

            _applicationService.StartApplicationOperation(this, "Analyzing VST plugins",
                pluginsToScan.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(() =>
            {
                foreach (var vst in pluginsToScan)
                    try
                    {
                        _applicationService.UpdateApplicationOperationStatus(
                            pluginsToScan.IndexOf(vst),
                            $"Scanning {vst.DllFilename}");

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        _logger.Debug($"Attempting to load {vst.DllFilename}");


                        _vstService.VstHost.LoadVST(vst);

                        if (vst.IsLoaded)
                        {
                            _logger.Debug($"Loaded {vst.DllFilename}, attempting to find presetParser");
                            vst.DeterminatePresetParser();

                            _applicationService.UpdateApplicationOperationStatus(
                                pluginsToScan.IndexOf(vst),
                                $"Retrieving presets for {vst.DllFilename}");

                            vst.PresetParser.ScanBanks();

                            _dispatcherService.BeginInvoke(() =>
                            {
                                vst.RootBank.PresetBanks.Clear();
                                vst.RootBank.PresetBanks.Add(vst.PresetParser.RootBank);
                                vst.NumPresets = vst.PresetParser.Presets.Count;
                                vst.Presets = vst.PresetParser.Presets;
                                vst.IsScanned = true;
                            });

                            _logger.Debug($"Unloading {vst.DllFilename}");
                            _vstService.VstHost.UnloadVST(vst);
                            _logger.Debug($"Unloaded {vst.DllFilename}");
                        }
                        else
                        {
                            _applicationService.AddApplicationOperationError(
                                $"Unable to analyze {vst.DllFilename} because of {vst.LoadException}");
                            _logger.Debug(vst.LoadException.StackTrace);
                        }
                    }
                    catch (Exception e)
                    {
                        _applicationService.AddApplicationOperationError(
                            $"Unable to analyze {vst.DllFilename} because of {e.Message}");
                        _logger.Debug(e.StackTrace);
                    }
            }, true);

            if (cancellationToken.IsCancellationRequested)
            {
                _applicationService.StopApplicationOperation("Plugin analysis cancelled.");
            }
            else
            {
                _applicationService.StopApplicationOperation("Plugin analysis completed.");
            }

            var unreportedPlugins =
                (from plugin in _vstService.Plugins where !plugin.Reported && plugin.IsScanned select plugin).Any();

            if (unreportedPlugins)
            {
                var result =
                    MessageBox.Show(
                        "There are unsupported plugins which are not reported." + Environment.NewLine +
                        "Would you like to report them, so we can implement support for them?",
                        "Report Unsupported Plugins", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    _commandManager.ExecuteCommand(Commands.Plugin.ReportUnsupportedPlugins);
                }
            }
        }
    }
}