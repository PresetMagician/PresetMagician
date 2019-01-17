using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VendorPresetParser;
using NuGet;
using PresetMagician.Models;
using PresetMagician.Models.EventArgs;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractScanPluginsCommandContainer : CommandContainerBase
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();

        protected readonly IRuntimeConfigurationService _runtimeConfigurationService;
        protected readonly IVstService _vstService;
        protected readonly IApplicationService _applicationService;
        protected readonly IDispatcherService _dispatcherService;
        protected readonly ICommandManager _commandManager;
        protected readonly IDatabaseService _databaseService;
        protected readonly INativeInstrumentsResourceGeneratorService _resourceGeneratorService;
        private int _totalPresets;
        private int _currentPresetIndex;
        private int updateThrottle;
        private int _currentPluginIndex;
        private Plugin _currentPlugin;

        protected AbstractScanPluginsCommandContainer(string command, ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService,
            IDatabaseService databaseService, INativeInstrumentsResourceGeneratorService resourceGeneratorService)
            : base(command, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => databaseService);
            Argument.IsNotNull(() => resourceGeneratorService);

            _runtimeConfigurationService = runtimeConfigurationService;
            _vstService = vstService;
            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _databaseService = databaseService;
            _commandManager = commandManager;
            _resourceGeneratorService = resourceGeneratorService;

            _vstService.Plugins.CollectionChanged += OnPluginsListChanged;
            _runtimeConfigurationService.ApplicationState.PropertyChanged += OnAllowPluginScanChanged;
        }

        protected abstract List<Plugin> GetPluginsToScan();


        protected override bool CanExecute(object parameter)
        {
            return _vstService.Plugins.Count > 0 &&
                   _runtimeConfigurationService.ApplicationState.AllowPluginScan;
        }

        protected void OnAllowPluginScanChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.AllowPluginScan)) InvalidateCommand();
        }

        protected void OnPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToScan = GetPluginsToScan();

            _applicationService.StartApplicationOperation(this, "Analyzing VST plugins",
                pluginsToScan.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(async () =>
            {
                foreach (var vst in pluginsToScan)
                {
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
                        vst.IsScanning = true;
                        _databaseService.Context.SaveChanges();
                        _databaseService.Context.CompressPresetData =
                            _runtimeConfigurationService.RuntimeConfiguration.CompressPresetData;

                        _vstService.VstHost.LoadVST(vst);

                        if (vst.IsLoaded)
                        {
                            _logger.Debug($"Loaded {vst.DllFilename}, attempting to find presetParser");

                            VendorPresetParser.DeterminatePresetParser(vst);
                            vst.PresetParser.PresetDataStorer = _databaseService.GetPresetDataStorer();

                            if (vst.PresetParser.SupportsAdditionalBankFiles)
                            {
                                vst.PresetParser.AdditionalBankFiles.Clear();
                                vst.PresetParser.AdditionalBankFiles.AddRange(vst.AdditionalBankFiles);
                            }

                            _currentPluginIndex = pluginsToScan.IndexOf(vst);
                            _currentPlugin = vst;

                            vst.Presets.CollectionChanged += PresetsOnCollectionChanged;
                            _databaseService.Context.LoadPresetsForPlugin(vst);
                            vst.Presets.CollectionChanged -= PresetsOnCollectionChanged;

                            
                            _totalPresets = vst.PresetParser.GetNumPresets();
                            _currentPresetIndex = 0;

                            _databaseService.Context.PresetUpdated += ContextOnPresetUpdated;
                            
                            var _itemsLock = new object();
                            var _itemsLock2 = new object();
                            
                            _dispatcherService.Invoke(() =>
                            {
                                BindingOperations.EnableCollectionSynchronization(vst.Presets, _itemsLock);
                                BindingOperations.EnableCollectionSynchronization(vst.RootBank.PresetBanks, _itemsLock2);
                            });
                            

                            vst.PresetParser.Presets = vst.Presets;
                            vst.PresetParser.RootBank = vst.RootBank.First();

                            lock (_itemsLock)
                            lock (_itemsLock2)
                            {
                                vst.PresetParser.DoScan();
                            }
                            

                            _databaseService.Context.PresetUpdated -= ContextOnPresetUpdated;

                            vst.IsScanned = true;
                            

                            await _dispatcherService.InvokeAsync(async () =>
                            {
                            vst.NativeInstrumentsResource.Load(vst);
                                if (_runtimeConfigurationService.RuntimeConfiguration.AutoCreateResources)
                                {
                                    _logger.Debug($"Auto-generating resources for {vst.DllFilename}");
                                    _applicationService.UpdateApplicationOperationStatus(
                                        pluginsToScan.IndexOf(vst),
                                        $"Auto-generating resources for {vst.DllFilename}");


                                    await _resourceGeneratorService.AutoGenerateResources(vst);
                                }
                                    });
                                

                           
                            

                            await _databaseService.Context.Flush();

                            

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

#if DEBUG
                        throw;
#endif
                    }

                    vst.IsScanning = false;
                    _databaseService.Context.SaveChanges();
                }
            }, true);

            await _databaseService.Context.SaveChangesAsync();

            if (cancellationToken.IsCancellationRequested)
            {
                _applicationService.StopApplicationOperation("Plugin analysis cancelled.");
            }
            else
            {
                _applicationService.StopApplicationOperation("Plugin analysis completed.");
            }

            var unreportedPlugins =
                (from plugin in _vstService.Plugins
                    where !plugin.IsReported && !plugin.IsSupported && plugin.IsScanned && plugin.IsEnabled
                    select plugin).Any();

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

        private void ContextOnPresetUpdated(object sender, PresetUpdatedEventArgs e)
        {
            updateThrottle++;
            _currentPresetIndex++;

            if (updateThrottle > 10)
            {
                _applicationService.UpdateApplicationOperationStatus(
                    _currentPluginIndex,
                    $"Adding/Updating presets for {_currentPlugin.PluginName} ({_currentPresetIndex} / {_totalPresets}): Preset {e.NewValue.PresetName}");
                updateThrottle = 0;
            }
        }

        private void PresetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            updateThrottle++;

            if (updateThrottle > 10)
            {
                var collection = sender as ObservableCollection<Preset>;
                _applicationService.UpdateApplicationOperationStatus(
                    _currentPluginIndex,
                    $"Pre-loading presets from database for {_currentPlugin.PluginName}: Preset #{collection.Count}");
                updateThrottle = 0;
            }
        }
    }
}