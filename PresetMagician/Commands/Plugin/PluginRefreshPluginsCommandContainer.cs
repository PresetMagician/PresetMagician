using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Extensions;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : CommandContainerBase
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();

        private readonly IApplicationService _applicationService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IMessageService _messageService;
        private readonly IDatabaseService _databaseService;

        public PluginRefreshPluginsCommandContainer(ICommandManager commandManager,
            IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService,
            IApplicationService applicationService, IDispatcherService dispatcherService,
            IMessageService messageService, IDatabaseService databaseService)
            : base(Commands.Plugin.RefreshPlugins, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => messageService);
            Argument.IsNotNull(() => databaseService);
            
            _runtimeConfigurationService = runtimeConfigurationService;
            _dispatcherService = dispatcherService;
            _vstService = vstService;
            _applicationService = applicationService;
            _messageService = messageService;
            _databaseService = databaseService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var vstPluginDLLFiles = new ObservableCollection<string>();
            var vstDirectories = (from vstDirectory in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories
                where vstDirectory.Active
                select vstDirectory).ToList();

            _applicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                vstDirectories.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(() =>
            {
                var dllFiles = new List<string>();

                foreach (var i in vstDirectories)
                {
                    _applicationService.UpdateApplicationOperationStatus(
                        vstDirectories.IndexOf(i),
                        $"Scanning {i.Path}");

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        dllFiles = VstUtils.EnumeratePlugins(i.Path);
                    }
                    catch (Exception e)
                    {
                        _applicationService.AddApplicationOperationError(
                            $"Unable to scan {i.Path} because of {e.Message}");
                    }

                    if (dllFiles.Count > 0)
                    {
                        vstPluginDLLFiles.AddRange(dllFiles);
                    }
                }
            }, true, cancellationToken);

            var plugins = _vstService.Plugins;
            _applicationService.StopApplicationOperation("VST directory scan completed.");
            _applicationService.StartApplicationOperation(this, "Verifying plugins",
                plugins.Count);

            await TaskHelper.Run(() =>
            {
                foreach (var plugin in plugins)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var isInDirectory = false;

                    foreach (var vstDirectory in vstDirectories)
                    {
                        if (plugin.DllPath.StartsWith(vstDirectory.Path))
                        {
                            isInDirectory = true;
                        }
                    }

                    if (File.Exists(plugin.DllPath) && isInDirectory)
                    {
                        plugin.IsPresent = true;

                        _applicationService.UpdateApplicationOperationStatus(
                            plugins.IndexOf(plugin),
                            $"Verifying {plugin.DllPath}");

                        var remotePluginInstance = _vstService.GetRemotePluginInstance(plugin).Result;
                        if (plugin.DllHash != remotePluginInstance.GetPluginHash())
                        {
                            plugin.Invalidate();
                        }

                        _dispatcherService.Invoke(() => { plugin.NativeInstrumentsResource.Load(plugin); });
                    }
                    else
                    {
                        plugin.IsPresent = false;
                    }
                }
            }, true, cancellationToken);

            await _dispatcherService.InvokeAsync(() =>
            {
                foreach (var dllPath in vstPluginDLLFiles)
                {
                    var foundPlugin = (from plugin in plugins where plugin.DllPath == dllPath select plugin)
                        .FirstOrDefault();

                    if (foundPlugin == null)
                    {
                        var plugin = new Plugin
                        {
                            DllPath = dllPath
                        };
                        
                        //Debug.WriteLine(_databaseService.Context.Entry(plugin).State);

                        plugins.Add(plugin);

                        //_databaseService.Context.Entry(plugin).State = EntityState.Detached;
                        Debug.WriteLine(_databaseService.Context.Entry(plugin).State);

                    }
                }
            });


            var crashedPlugin = (from plugin in plugins where plugin.IsScanning select plugin).FirstOrDefault();

            if (crashedPlugin != null)
            {
                var result = await _messageService.ShowAsync(
                    $"It seems the plugin {crashedPlugin.DllFilename} caused PresetMagician to crash during the last scan." +
                    Environment.NewLine +
                    "Would you like to disable the plugin?",
                    "Plugin crash detected", MessageButton.YesNo);

                if (result == MessageResult.Yes)
                {
                    crashedPlugin.IsEnabled = false;
                }

                crashedPlugin.IsScanning = false;
            }

            await _vstService.SavePlugins();

            if (cancellationToken.IsCancellationRequested)
            {
                _applicationService.StopApplicationOperation("VST directory scan aborted.");
            }
            else
            {
                _applicationService.StopApplicationOperation("VST directory scan completed.");
            }
        }
    }
}