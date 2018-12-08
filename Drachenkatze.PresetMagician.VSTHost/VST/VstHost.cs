using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
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
        public VstHost()
        {
            pluginExporter = new VSTPluginExport();
        }

        public VSTPluginExport pluginExporter;

        public ObservableCollection<String> EnumeratePlugins(string pluginDirectory)
        {
            ObservableCollection<String> vstPlugins = new ObservableCollection<String>();
            foreach (string file in Directory.EnumerateFiles(
    pluginDirectory, "*.dll", SearchOption.AllDirectories))
            {
                vstPlugins.Add(file);
            }

            return vstPlugins;
        }

        [HandleProcessCorruptedStateExceptions]
        public VSTPlugin LoadVST(VSTPlugin vst)
        {
            HostCommandStub hostCommandStub = new HostCommandStub();

            hostCommandStub.Directory = Path.GetDirectoryName(vst.PluginDLLPath);

            try
            {
                VstPluginContext ctx = VstPluginContext.Create(vst.PluginDLLPath, hostCommandStub);

                vst.PluginContext = ctx;
                ctx.Set("PluginPath", vst.PluginDLLPath);
                ctx.Set("HostCmdStub", hostCommandStub);
                ctx.PluginCommandStub.Open();
                vst.PluginContext.PluginCommandStub.MainsChanged(true);
                vst.doCache();

                vst.LoadError = "Loaded.";
            }
            catch (Exception e)
            {
                Debug.WriteLine("load error: " + e.ToString());
                vst.LoadError = "Could not load plugin. " + e.ToString();
            }

            return vst;
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            HostCommandStub hostCmdStub = (HostCommandStub)sender;

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

        public void UnloadVST(VSTPlugin vst)
        {
            if (vst.PluginContext != null)
            {
                vst.PluginContext.Dispose();
            }
        }
    }
}