using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Orchestra.Services;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.Workers;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Catel.Reflection;
using NuGet;

namespace PresetMagicianShell.ViewModels
{
    public class VstPluginsViewModel : ViewModelBase
    {
        private IStatusService _statusService;
        private IPleaseWaitService _pleaseWaitService;
        private VstHost _vstHost;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

       
        #region VstPlugins property

        /// <summary>
        /// Gets or sets the VstPlugins value.
        /// </summary>
        public ObservableCollection<Plugin> VstPlugins { get; private set; }

        #endregion


        public override string Title { get; protected set; } = "VST Plugins";

        private readonly IViewModelFactory _viewModelFactory;
        private readonly IUIVisualizerService _uiVisualizerService;

        public VstPluginsViewModel(IStatusService statusService, IPleaseWaitService pleaseWaitService,
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator, IViewModelFactory viewModelFactory, IUIVisualizerService uiVisualizerService)
        {
            Argument.IsNotNull(() => statusService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => viewModelFactory);
            Argument.IsNotNull(() => uiVisualizerService);

            _pleaseWaitService = pleaseWaitService;
            _vstHost = new VstHost();
            _statusService = statusService;
            _runtimeConfigurationService = runtimeConfigurationService;
            _viewModelFactory = viewModelFactory;
            _uiVisualizerService = uiVisualizerService;

            serviceLocator.RegisterInstance(this);

            ScanPlugins = new TaskCommand(OnScanPluginsExecute);
            RefreshPluginList = new TaskCommand(OnRefreshPluginListExecute);
            EnablePlugin = new Command<object>(OnEnablePluginExecute);
            DisablePlugin = new Command<object>(OnDisablePluginExecute);
            ShowPluginInfo = new Command<object>(OnShowPluginInfoExecute);

            VstPlugins = runtimeConfigurationService.RuntimeConfiguration.Plugins;
            RefreshPluginList.Execute();
        }

        public Command<object> EnablePlugin { get; set; }

        private void OnEnablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();
            
            foreach (var plugin in plugins)
            {
                plugin.Enabled = true;
            }
        }

        public Command<object> DisablePlugin { get; set; }

        private void OnDisablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();

            foreach (var plugin in plugins)
            {
                plugin.Enabled = false;
            }
        }

        public Command<object> ShowPluginInfo { get; set; }

        private void OnShowPluginInfoExecute(object parameter)
        {
            var plugin = (parameter as Plugin);

            /*var j = TypeCache.GetTypes();

            var settingsViewModelType = TypeCache.GetTypes(x => string.Equals(x.Name, "VstPluginInfoViewModel")).FirstOrDefault();
            if (settingsViewModelType == null)
            {
                //throw Log.ErrorAndCreateException<InvalidOperationException>("Cannot find type '{0}'", "VstPluginInfoViewModel");
            }

            var viewModel = _viewModelFactory.CreateViewModel(settingsViewModelType, plugin.PluginInfoItems, null);*/

            _uiVisualizerService.ShowDialogAsync<VstPluginInfoViewModel>(plugin);

        }

        public TaskCommand RefreshPluginList { get; set; }

        private async Task OnRefreshPluginListExecute()
        {
            ObservableCollection<String> vstPluginDLLs = new ObservableCollection<String>();
            ObservableCollection<VSTPlugin> vstPlugins = new ObservableCollection<VSTPlugin>();

            await TaskHelper.Run(() =>
            {
                foreach (var i in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories)
                {
                    foreach (string path in _vstHost.EnumeratePlugins(i.Path))
                    {
                        vstPluginDLLs.Add(path);
                    }
                }
            }, true);

            VstPlugins.RemoveAll(item => !vstPluginDLLs.Contains(item.DllPath));

            foreach (String dllPath in vstPluginDLLs)
            {
                var foundPlugin = (from plugin in VstPlugins where plugin.DllPath == dllPath select plugin).FirstOrDefault();

                if (foundPlugin == null)
                {
                    VstPlugins.Add(new Plugin()
                    {
                        DllPath = dllPath
                    });
                }
            }
        }

        public TaskCommand ScanPlugins { get; set; }

        private async Task OnScanPluginsExecute()
        {
            var newList = (from plugin in VstPlugins where plugin.Enabled == true select plugin).ToList();

            await TaskHelper.Run(() =>
            {
                foreach (Plugin vst in newList)
                {
                    try
                    {
                        UpdateStatus(newList.IndexOf(vst)+1, newList.Count, $"Loading {vst.DllPath}");
                        _vstHost.LoadVST(vst);
                        vst.DeterminatePresetParser();

                        UpdateStatus(newList.IndexOf(vst)+1, newList.Count, $"Scanning banks for {vst.DllPath}");
                        vst.PresetParser.ScanBanks();
                        vst.PresetBanks = new ObservableCollection<PresetBank>(vst.PresetParser.Banks);
                        vst.NumPresets = vst.PresetParser.NumPresets;
                        vst.IsScanned = true;

                        UpdateStatus(newList.IndexOf(vst)+1, newList.Count, $"Unloading {vst.DllPath}");
                        _vstHost.UnloadVST(vst);

                        UpdateStatus(newList.IndexOf(vst)+1, newList.Count, $"Done scanning {vst.DllPath}");
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        foreach (var i in e.LoaderExceptions)
                        {
                            _logger.Info(i.Message);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Info("Unable to load {0}, exception occurred: {1}", vst.DllPath, e.ToString());
                    }



                }
            }, true);
        }

        private void UpdateStatus (int currentItem, int totalItems, string statusText)
        {
            _logger.Info(statusText);
            _pleaseWaitService.UpdateStatus(currentItem, totalItems, "{0} {1} " + statusText);
            _statusService.UpdateStatus("({1} / {2}) {0}", statusText, currentItem, totalItems);
        }

    }
}