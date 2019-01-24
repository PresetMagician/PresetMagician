using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using Catel.Threading;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using Newtonsoft.Json;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private readonly IDatabaseService _databaseService;
        private IsolatedProcess _frontendProcess;

        public VstService(IDatabaseService databaseService)
        {
            Argument.IsNotNull(() => databaseService);

            _databaseService = databaseService;
            _databaseService.Context.Plugins.Include(plugin => plugin.AdditionalBankFiles).Load();

            Plugins = _databaseService.Context.Plugins.Local;

            // TODO remove
            VstHost = new VstHost.VST.VstHost();
        }

        public async Task SavePlugins()
        {
            await _databaseService.Context.SaveChangesAsync();
        }

        public byte[] GetPresetData(Preset preset)
        {
            return _databaseService.Context.GetPresetData(preset);
        }

        public async Task<IRemoteVstService> LoadVstInteractive(Plugin plugin)
        {
            return await LoadVst(plugin, true, false);
        }

        public async Task<IRemoteVstService> LoadVst(Plugin plugin, bool backgroundProcessing = true, bool sharedPool = true)
        {
            IRemoteVstService vstService = null;
            try
            {
                if (sharedPool)
                {
                    vstService = await ProcessPool.GetRemoteVstService();
                    plugin.PooledRemoteVstService = true;
                }
                else
                {
                    _frontendProcess = new IsolatedProcess();
                    await _frontendProcess.WaitForStartup();
                    vstService = _frontendProcess.GetVstService();
                    plugin.PooledRemoteVstService = false;
                }

                await TaskHelper.Run(() =>  plugin.Guid = vstService.LoadPlugin(plugin.DllPath, backgroundProcessing), true);
                
                plugin.PluginName = vstService.GetPluginName(plugin.Guid);
                plugin.PluginVendor = vstService.GetPluginVendor(plugin.Guid);
                plugin.PluginInfo = vstService.GetPluginInfo(plugin.Guid);
                plugin.PluginId = plugin.PluginInfo.PluginID;
                
                plugin.PluginInfo.Flags = JsonConvert.DeserializeObject<VstPluginFlags>(plugin.PluginInfo.StringFlags);

                if (plugin.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth))
                {
                    plugin.PluginType = Plugin.PluginTypes.Instrument;
                }
                else
                {
                    plugin.PluginType = Plugin.PluginTypes.Effect;
                }

                plugin.PluginInfoItems.Clear();
                plugin.PluginInfoItems.AddRange(vstService.GetPluginInfoItems(plugin.Guid));
                plugin.IsLoaded = true;
                plugin.RemoteVstService = vstService;
            }
            catch (Exception e)
            {
                plugin.OnLoadError(e);
            }

            return vstService;
        }

        public async Task UnloadVst(Plugin plugin)
        {
            if (plugin.PooledRemoteVstService)
            {
            var vstService = await ProcessPool.GetRemoteVstService();
            vstService.UnloadPlugin(plugin.Guid);
            
           
                ProcessPool.KillRemotevstService();
            }
            else
            {
                plugin.RemoteVstService.UnloadPlugin(plugin.Guid);
            }
            
            plugin.IsLoaded = false;
        }

        public VstHost.VST.VstHost VstHost { get; set; }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public ObservableCollection<Plugin> Plugins { get; }
        public FastObservableCollection<Plugin> CachedPlugins { get; } = new FastObservableCollection<Plugin>();

        public FastObservableCollection<Preset> SelectedPresets { get; } = new FastObservableCollection<Preset>();
        public FastObservableCollection<Preset> PresetExportList { get; } = new FastObservableCollection<Preset>();


        #region SelectedPlugin

        private Plugin _selectedPlugin;

        public Plugin SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                _selectedPlugin = value;
                SelectedPluginChanged.SafeInvoke(this);
            }
        }

        public event EventHandler SelectedPluginChanged;

        #endregion

        #region SelectedExportPreset

        private Preset _selectedExportPreset;

        public Preset SelectedExportPreset
        {
            get => _selectedExportPreset;
            set
            {
                _selectedExportPreset = value;
                SelectedExportPresetChanged.SafeInvoke(this);
            }
        }

        public event EventHandler SelectedExportPresetChanged;

        #endregion
    }
}