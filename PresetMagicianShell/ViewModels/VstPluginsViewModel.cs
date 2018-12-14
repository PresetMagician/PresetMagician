using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VSTHost.VST;
using NuGet;
using Orchestra.Services;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.ViewModels
{
    public class VstPluginsViewModel : ViewModelBase
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IUIVisualizerService _uiVisualizerService;

        private readonly IViewModelFactory _viewModelFactory;
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IStatusService _statusService;
        private readonly VstHost _vstHost;

        public VstPluginsViewModel(IStatusService statusService, IPleaseWaitService pleaseWaitService,
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator,
            IViewModelFactory viewModelFactory, IUIVisualizerService uiVisualizerService)
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


        public ObservableCollection<Plugin> VstPlugins { get; }

        public bool IsScanning { get; private set; }
        public int ScanProgressPercent { get; private set; }
        public string ScanProgressText { get; private set; }

        public override string Title { get; protected set; } = "VST Plugins";

        public Command<object> EnablePlugin { get; set; }

        public Command<object> DisablePlugin { get; set; }

        public Command<object> ShowPluginInfo { get; set; }

        public TaskCommand RefreshPluginList { get; set; }

        public TaskCommand ScanPlugins { get; set; }

        private void OnEnablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();

            foreach (var plugin in plugins) plugin.Enabled = true;
        }

        private void OnDisablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();

            foreach (var plugin in plugins) plugin.Enabled = false;
        }

        private void OnShowPluginInfoExecute(object parameter)
        {
            var plugin = parameter as Plugin;

            _uiVisualizerService.ShowDialogAsync<VstPluginInfoViewModel>(plugin);
        }

        private async Task OnRefreshPluginListExecute()
        {
            var vstPluginDLLs = new ObservableCollection<string>();
            var vstPlugins = new ObservableCollection<VSTPlugin>();

            await TaskHelper.Run(() =>
            {
                foreach (var i in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories)
                foreach (var path in _vstHost.EnumeratePlugins(i.Path))
                    vstPluginDLLs.Add(path);
            }, true);

            VstPlugins.RemoveAll(item => !vstPluginDLLs.Contains(item.DllPath));

            foreach (var dllPath in vstPluginDLLs)
            {
                var foundPlugin = (from plugin in VstPlugins where plugin.DllPath == dllPath select plugin)
                    .FirstOrDefault();

                if (foundPlugin == null)
                    VstPlugins.Add(new Plugin
                    {
                        DllPath = dllPath
                    });
            }
        }

        private async Task OnScanPluginsExecute()
        {
            IsScanning = true;
            var newList = (from plugin in VstPlugins where plugin.Enabled select plugin).ToList();

            await TaskHelper.Run(() =>
            {
                foreach (var vst in newList)
                    try
                    {
                        UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Loading {vst.DllPath}");
                        _vstHost.LoadVST(vst);
                        vst.DeterminatePresetParser();

                        UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Scanning banks for {vst.DllPath}");
                        vst.PresetParser.ScanBanks();
                        vst.RootBank = vst.PresetParser.RootBank;
                        vst.NumPresets = vst.PresetParser.Presets.Count;
                        vst.Presets = vst.PresetParser.Presets;
                        vst.IsScanned = true;

                        UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Unloading {vst.DllPath}");
                        _vstHost.UnloadVST(vst);

                        UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Done scanning {vst.DllPath}");
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        foreach (var i in e.LoaderExceptions) _logger.Info(i.Message);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("Unable to load {0}, exception occurred: {1}", vst.DllPath, e.ToString());
                    }
            }, true);

            IsScanning = false;
        }

        private void UpdateStatus(int currentItem, int totalItems, string statusText)
        {
            var progressText = String.Format("{1} / {2}) {0}", statusText, currentItem, totalItems);
            ScanProgressPercent = totalItems / currentItem * 100;
            ScanProgressText = progressText;
            _pleaseWaitService.UpdateStatus(currentItem, totalItems, "{0} {1} " + statusText);
            _statusService.UpdateStatus(progressText);
        }
    }
}