using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.ExceptionServices;
using System.ServiceModel;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using PresetMagician.Models;
using PresetMagician.ProcessIsolation.Exceptions;
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

        public virtual bool Ping()
        {
            App.Ping();
            return true;
        }

        public Guid RegisterPlugin(string dllPath, bool backgroundProcessing = true)
        {
            App.Ping();
            var plugin = new RemoteVstPlugin {DllPath = dllPath, BackgroundProcessing = backgroundProcessing};
            var guid = Guid.NewGuid();
            _plugins.Add(guid, plugin);

            return guid;
        }


        public void LoadPlugin(Guid guid, bool debug = false)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);

            try
            {
                _vstHost.LoadVst(plugin, debug);
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
        }

        public void UnloadPlugin(Guid guid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);

            if (plugin.IsLoaded)
            {
                _vstHost.UnloadVst(plugin);
            }
        }

        public void ReloadPlugin(Guid guid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(guid);
            _vstHost.ReloadPlugin(plugin);
        }

        private RemoteVstPlugin GetPluginByGuid(Guid guid)
        {
            App.Ping();
            if (!_plugins.ContainsKey(guid))
            {
                throw new FaultException($"Plugin with GUID {guid} does not exist in this instance");
            }

            return _plugins[guid];
        }

        public bool OpenEditorHidden(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.OpenEditorHidden();
        }

        public bool OpenEditor(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.OpenEditor();
        }

        public void CloseEditor(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            plugin.CloseEditor();
        }

        public byte[] CreateScreenshot(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            if (!plugin.IsEditorOpen)
            {
                throw new VstPluginEditorNotOpenException(plugin);
            }

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
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.PluginContext.PluginCommandStub.GetEffectName();
        }

        public string GetEffectivePluginName(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

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

        public int GetPluginVendorVersion(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.PluginContext.PluginCommandStub.GetVendorVersion();
        }

        public string GetPluginProductString(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.PluginContext.PluginCommandStub.GetProductString();
        }

        public string GetPluginVendor(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);

            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            var pluginVendor = plugin.PluginContext.PluginCommandStub.GetVendorString();

            if (string.IsNullOrEmpty(pluginVendor))
            {
                pluginVendor = "Unknown";
            }

            return pluginVendor;
        }

        public List<PluginInfoItem> GetPluginInfoItems(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.GetPluginInfoItems(plugin.PluginContext);
        }

        public VstPluginInfoSurrogate GetPluginInfo(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            var vstInfo = new VstPluginInfoSurrogate
            {
                Flags = plugin.PluginContext.PluginInfo.Flags,
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

        [HandleProcessCorruptedStateExceptions]
        public void SetProgram(Guid pluginGuid, int program)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }


            plugin.PluginContext.PluginCommandStub.SetProgram(program);
        }

        public string GetCurrentProgramName(Guid pluginGuid)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.PluginContext.PluginCommandStub.GetProgramName();
        }

        public byte[] GetChunk(Guid pluginGuid, bool isPreset)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            return plugin.PluginContext.PluginCommandStub.GetChunk(isPreset);
        }

        public void SetChunk(Guid pluginGuid, byte[] data, bool isPreset)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            plugin.PluginContext.PluginCommandStub.SetChunk(data, isPreset);
        }

        public void ExportNksAudioPreview(Guid pluginGuid, PresetExportInfo preset, byte[] presetData,
            string userContentDirectory, int initialDelay)
        {
            App.Ping();
            var plugin = GetPluginByGuid(pluginGuid);
            if (!plugin.IsLoaded)
            {
                throw new VstPluginNotLoadedException(plugin);
            }

            var exporter = new NKSExport(_vstHost) {UserContentDirectory = userContentDirectory};
            exporter.ExportPresetAudioPreviewRealtime(plugin, preset, presetData, initialDelay);
        }

        public void ExportNks(Guid pluginGuid, PresetExportInfo preset, byte[] presetData, string userContentDirectory)
        {
            App.Ping();
            var exporter = new NKSExport(_vstHost) {UserContentDirectory = userContentDirectory};
            exporter.ExportNKSPreset(preset, presetData);
        }

        public bool Exists(string file)
        {
            App.Ping();
            return File.Exists(file);
        }

        public long GetSize(string file)
        {
            App.Ping();
            var fileInfo = new FileInfo(file);
            return fileInfo.Length;
        }

        public string GetHash(string file)
        {
            App.Ping();
            var data = File.ReadAllBytes(file);
            var hash = HashUtils.getIxxHash(data);
            GC.Collect();
            return hash;
        }

        public DateTime GetLastModifiedDate(string file)
        {
            App.Ping();
            return File.GetLastWriteTime(file);
        }

        public byte[] GetContents(string file)
        {
            App.Ping();
            return File.ReadAllBytes(file);
        }

        public void KillSelf()
        {
            App.KillSelf();
        }
    }
}