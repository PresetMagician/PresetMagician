using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Threading;
using PresetMagicianShell.Extensions;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : CommandContainerBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;

        public PluginRefreshPluginsCommandContainer(ICommandManager commandManager,
            IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Plugin.RefreshPlugins, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);

            _runtimeConfigurationService = runtimeConfigurationService;
            _vstService = vstService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var vstPluginDLLFiles = new ObservableCollection<string>();

            _log.Debug("Scanning VST directories for changed plugins");
            _runtimeConfigurationService.ApplicationState.IsPluginsRefreshing = true;

            await TaskHelper.Run(() =>
            {
                ObservableCollection<string> dllFiles = new ObservableCollection<string>();

                foreach (var i in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories)
                {
                    try
                    {
                        dllFiles = _vstService.VstHost.EnumeratePlugins(i.Path);
                    }
                    catch (Exception e)
                    {
                        _log.Error($"Unable to scan {i.Path} because of {e.Message}");
                        _log.Debug(e);
                    }

                    if (dllFiles.Count > 0)
                    {
                        vstPluginDLLFiles.AddRange(dllFiles);
                    }
                }
            }, true);

            var plugins = _runtimeConfigurationService.RuntimeConfiguration.Plugins;

            using (plugins.SuspendChangeNotifications())
            {
                plugins.RemoveAll(item => !vstPluginDLLFiles.Contains(item.DllPath));

                foreach (var dllPath in vstPluginDLLFiles)
                {
                    var foundPlugin = (from plugin in plugins where plugin.DllPath == dllPath select plugin)
                        .FirstOrDefault();

                    if (foundPlugin == null)
                    {
                        plugins.Add(new Plugin
                        {
                            DllPath = dllPath
                        });
                    }
                }
            }

            _log.Debug("Scanning VST directories for changed plugins completed.");
            _runtimeConfigurationService.ApplicationState.IsPluginsRefreshing = false;
        }
    }
}