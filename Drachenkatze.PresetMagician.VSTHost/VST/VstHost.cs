using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using Jacobi.Vst.Interop.Host;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class NoOfflineSupportException : Exception
    {
    }

    public class EffectsNotSupportedException : Exception
    {
    }

    public class NoRealtimeProcessingException : Exception
    {
    }

    public class VstHost
    {
        //public VSTPluginExport pluginExporter;

        public VstHost()
        {
            //pluginExporter = new VSTPluginExport();
        }

        /// <summary>
        ///     Returns all found DLLs for a specific directory
        /// </summary>
        /// <param name="pluginDirectory"></param>
        /// <returns></returns>
        public ObservableCollection<string> EnumeratePlugins(string pluginDirectory)
        {
            var vstPlugins = new ObservableCollection<string>();
            foreach (var file in Directory.EnumerateFiles(
                pluginDirectory, "*.dll", SearchOption.AllDirectories))
            {
                vstPlugins.Add(file);
            }

            return vstPlugins;
        }

        public enum PluginTypes
        {
            Effect,
            Instrument,
            Unknown
        }

      
        public void LoadVST(IVstPlugin vst)
        {
            Debug.WriteLine("Attempting to load");
            var hostCommandStub = new HostCommandStub();
            hostCommandStub.PluginCalled += (sender, args) =>
            {
                Debug.WriteLine(args.Message);
            };

            try
            {
                var ctx = VstPluginContext.Create(vst.DllPath, hostCommandStub);

                vst.PluginContext = ctx;
                ctx.Set("PluginPath", vst.DllPath);
                ctx.Set("HostCmdStub", hostCommandStub);
                ctx.PluginCommandStub.Open();
                vst.PluginContext.PluginCommandStub.MainsChanged(true);
                vst.OnLoaded();
            }
            catch (Exception e)
            {
                vst.OnLoadError(e.ToString());
            }
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            var hostCmdStub = (HostCommandStub) sender;

            // can be null when called from inside the plugin main entry point.
            if (hostCmdStub.PluginContext.PluginInfo != null)
            {
                Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
            }
            else
            {
                Debug.WriteLine("The loading Plugin called:" + e.Message);
            }
        }

        public void UnloadVST(IVstPlugin vst)
        {
            vst.PluginContext?.Dispose();
        }
    }
}