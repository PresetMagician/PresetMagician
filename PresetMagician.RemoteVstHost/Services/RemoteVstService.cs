using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.ServiceModel;
using Newtonsoft.Json;
using PresetMagician.Models;
using PresetMagician.VstHost.VST;
using SharedModels;

namespace PresetMagician.ProcessIsolation.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = true, IncludeExceptionDetailInFaults = true)]
    public class RemoteVstService : IRemoteVstService
    {
        private static readonly VstHost.VST.VstHost _vstHost = new VstHost.VST.VstHost(true);
        private readonly Dictionary<Guid, RemoteVstPlugin> _plugins = new Dictionary<Guid, RemoteVstPlugin>();

        public bool Ping()
        {
            return true;
        }

        public Guid LoadPlugin(string dllPath, bool backgroundProcessing = true)
        {
            var plugin = new RemoteVstPlugin(_vstHost) {DllPath = dllPath, BackgroundProcessing = backgroundProcessing};

            var guid = Guid.NewGuid();

            try
            {
                _vstHost.LoadVst(plugin);
                _plugins.Add(guid, plugin);
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }

            return guid;
        }

        public void UnloadPlugin(Guid guid)
        {
            var plugin = GetPluginByGuid(guid);
            _vstHost.UnloadVst(plugin);
        }
        
        public void ReloadPlugin(Guid guid)
        {
            var plugin = GetPluginByGuid(guid);
            _vstHost.ReloadPlugin(plugin);
        }

        private RemoteVstPlugin GetPluginByGuid(Guid guid)
        {
            if (!_plugins.ContainsKey(guid))
            {
                throw new FaultException($"Plugin with GUID {guid} does not exist in this instance");
            }

            return _plugins[guid];
        }

        public bool OpenEditorHidden(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            return plugin.OpenEditorHidden();
        }
        
        public bool OpenEditor(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            return plugin.OpenEditor();
        }

        public void CloseEditor(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            plugin.CloseEditor();
        }

        public byte[] CreateScreenshot(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);

            var bitmap = plugin.CreateScreenshot();

            if (bitmap == null)
            {
                return null;
            }

            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);

            return ms.ToArray();
        }

        public string GetPluginName(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);

            var pluginName = plugin.PluginContext.PluginCommandStub.GetEffectName();


            if (string.IsNullOrEmpty(pluginName))
            {
                pluginName = plugin.PluginContext.PluginCommandStub.GetProductString();

                if (string.IsNullOrEmpty(pluginName))
                {
                    // Extreme fallback: Use plugin DLL name
                    pluginName = plugin.DllFilename.Replace(".dll", "");
                }
            }

            return pluginName;
        }

        public string GetPluginVendor(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            var pluginVendor = plugin.PluginContext.PluginCommandStub.GetVendorString();

            if (string.IsNullOrEmpty(pluginVendor))
            {
                pluginVendor = "Unknown";
            }

            return pluginVendor;
        }

        public List<PluginInfoItem> GetPluginInfoItems(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);


            return plugin.GetPluginInfoItems(plugin.PluginContext);
        }

        public VstPluginInfoSurrogate GetPluginInfo(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);

            var vstInfo = new VstPluginInfoSurrogate
            {
                StringFlags = JsonConvert.SerializeObject(plugin.PluginContext.PluginInfo.Flags),
                PluginID = plugin.PluginContext.PluginInfo.PluginID,
                InitialDelay = plugin.PluginContext.PluginInfo.InitialDelay,
                ProgramCount = plugin.PluginContext.PluginInfo.ProgramCount,
                ParameterCount = plugin.PluginContext.PluginInfo.ParameterCount,
                PluginVersion = plugin.PluginContext.PluginInfo.PluginVersion,
                AudioInputCount = plugin.PluginContext.PluginInfo.AudioInputCount,
                AudioOutputCount = plugin.PluginContext.PluginInfo.AudioOutputCount
            };

            return vstInfo;
        }

        public void SetProgram(Guid pluginGuid, int program)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            plugin.PluginContext.PluginCommandStub.SetProgram(program);
        }
        
        public string GetCurrentProgramName(Guid pluginGuid)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            return plugin.PluginContext.PluginCommandStub.GetProgramName();
        }

        public byte[] GetChunk(Guid pluginGuid, bool isPreset)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            return plugin.PluginContext.PluginCommandStub.GetChunk(isPreset);
        }

        public void SetChunk(Guid pluginGuid, byte[] data, bool isPreset)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            plugin.PluginContext.PluginCommandStub.SetChunk(data, isPreset);
        }

        public void ExportNksAudioPreview(Guid pluginGuid, PresetExportInfo preset, byte[] presetData, string userContentDirectory, int initialDelay)
        {
            var plugin = GetPluginByGuid(pluginGuid);
            var exporter = new NKSExport(_vstHost) {UserContentDirectory = userContentDirectory};
            exporter.ExportPresetAudioPreviewRealtime(plugin, preset, presetData, initialDelay);
        }
        
        public void ExportNks(Guid pluginGuid, PresetExportInfo preset, byte[] presetData, string userContentDirectory)
        {
            var exporter = new NKSExport(_vstHost) {UserContentDirectory = userContentDirectory};
            exporter.ExportNKSPreset(preset, presetData);
        }
    }
}