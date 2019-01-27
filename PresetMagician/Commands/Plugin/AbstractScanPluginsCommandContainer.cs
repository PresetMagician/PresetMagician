using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Catel.Windows;
using Drachenkatze.PresetMagician.VendorPresetParser;
using PresetMagician.Models;
using PresetMagician.Models.EventArgs;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractScanPluginsCommandContainer : CommandContainerBase
    {
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

            _databaseService.Context.PresetUpdated += ContextOnPresetUpdated;
            await TaskHelper.Run(async () =>
            {
                foreach (var plugin in pluginsToScan)
                {
                    try
                    {
                        _vstService.SelectedPlugin = plugin;
                        _applicationService.UpdateApplicationOperationStatus(
                            pluginsToScan.IndexOf(plugin),
                            $"Scanning {plugin.DllFilename}");

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        plugin.Debug($"Attempting to load {plugin.DllFilename}");
                        plugin.IsScanning = true;
                        _databaseService.Context.SaveChanges();
                        _databaseService.Context.CompressPresetData =
                            _runtimeConfigurationService.RuntimeConfiguration.CompressPresetData;
                        _databaseService.Context.PreviewNote =
                            _runtimeConfigurationService.RuntimeConfiguration.DefaultPreviewMidiNote;

                        var remotePluginInstance = await _vstService.GetRemotePluginInstance(plugin);

                        await remotePluginInstance.LoadPlugin();

                        if (remotePluginInstance.IsLoaded)
                        {
                            await _dispatcherService.InvokeAsync(() =>
                            {
                                plugin.NativeInstrumentsResource.Load(plugin);
                            });

                            plugin.Debug($"Loaded {plugin.DllFilename}, attempting to find presetParser");

                            VendorPresetParser.DeterminatePresetParser(remotePluginInstance);
                            plugin.PresetParser.PresetDataStorer = _databaseService.GetPresetDataStorer();

                            if (plugin.PresetParser.SupportsAdditionalBankFiles)
                            {
                                plugin.PresetParser.AdditionalBankFiles.Clear();
                                plugin.PresetParser.AdditionalBankFiles.AddRange(plugin.AdditionalBankFiles);
                            }

                            _currentPluginIndex = pluginsToScan.IndexOf(plugin);
                            _currentPlugin = plugin;

                            plugin.Presets.CollectionCountChanged += PresetsOnCollectionChanged;
                            _databaseService.Context.LoadPresetsForPlugin(plugin);
                            plugin.Presets.CollectionCountChanged -= PresetsOnCollectionChanged;


                            using (plugin.Presets.SuspendChangeNotifications())
                            {
                                using (plugin.Presets.SuspendChangeNotifications())
                                {
                                    plugin.PresetParser.Presets = plugin.Presets;
                                    plugin.PresetParser.RootBank = plugin.RootBank.First();

                                    _totalPresets = plugin.PresetParser.GetNumPresets();
                                    _currentPresetIndex = 0;

                                    await plugin.PresetParser.DoScan();
                                }
                            }

                            plugin.IsScanned = true;

                            if (_runtimeConfigurationService.RuntimeConfiguration.AutoCreateResources)
                            {
                                plugin.Debug(
                                    $"Auto-generating resources for {plugin.DllFilename} - Opening Editor",
                                    plugin.DllFilename);
                                _applicationService.UpdateApplicationOperationStatus(
                                    pluginsToScan.IndexOf(plugin),
                                    $"Auto-generating resources for {plugin.DllFilename} - Opening Editor");
                                if (_resourceGeneratorService.ShouldCreateScreenshot(remotePluginInstance))
                                {
                                    remotePluginInstance.OpenEditorHidden();
                                    _dispatcherService.Invoke(() => Application.Current.MainWindow.BringWindowToTop());
                                    await Task.Delay(1000);
                                }
                            }

                            await _dispatcherService.InvokeAsync(() =>
                            {
                                if (_runtimeConfigurationService.RuntimeConfiguration.AutoCreateResources)
                                {
                                    plugin.Debug(
                                        $"Auto-generating resources for {plugin.DllFilename} - Creating screenshot and applying magic");
                                    _applicationService.UpdateApplicationOperationStatus(
                                        pluginsToScan.IndexOf(plugin),
                                        $"Auto-generating resources for {plugin.DllFilename} - Creating screenshot and applying magic");

                                    _resourceGeneratorService.AutoGenerateResources(remotePluginInstance);
                                }
                            });


                            await _databaseService.Context.Flush();

                            plugin.Debug($"Unloading {plugin.DllFilename}");
                            remotePluginInstance.UnloadPlugin();
                            remotePluginInstance.KillHost();
                        }
                        else
                        {
                            _applicationService.AddApplicationOperationError(
                                $"Unable to analyze {plugin.DllFilename} because of {plugin.LoadErrorMessage}");
                        }
                    }
                    catch (Exception e)
                    {
                        plugin.OnLoadError(e);
                        _applicationService.AddApplicationOperationError(
                            $"Unable to analyze {plugin.DllFilename} because of {e.Message}");
                        plugin.Debug(e.StackTrace);
                    }

                    plugin.IsScanning = false;
                    _databaseService.Context.SaveChanges();
                }
            }, true);

            await _databaseService.Context.SaveChangesAsync();
            _databaseService.Context.PresetUpdated -= ContextOnPresetUpdated;

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

            if (_currentPresetIndex > _totalPresets)
            {
                Debug.WriteLine(
                    $"{e.NewValue.Plugin.PluginName}: Got called with {e.NewValue.PresetName} index {_currentPresetIndex} of {_totalPresets}");
            }

            if (updateThrottle > 10)
            {
                _applicationService.UpdateApplicationOperationStatus(
                    _currentPluginIndex,
                    $"Adding/Updating presets for {_currentPlugin.PluginName} ({_currentPresetIndex} / {_totalPresets}): Preset {e.NewValue.PresetName}");
                updateThrottle = 0;
            }
        }

        private void PresetsOnCollectionChanged(object sender, EventArgs e)
        {
            updateThrottle++;

            if (updateThrottle <= 10)
            {
                return;
            }

            var collection = sender as ObservableCollection<Preset>;
            _applicationService.UpdateApplicationOperationStatus(
                _currentPluginIndex,
                $"Pre-loading presets from database for {_currentPlugin.PluginName}: Preset #{collection.Count}");
            updateThrottle = 0;
        }
    }
}