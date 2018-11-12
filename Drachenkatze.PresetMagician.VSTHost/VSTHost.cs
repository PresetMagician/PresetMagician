using Jacobi.Vst.Interop.Host;
using Jacobi.Vst.Samples.Host;
using System;
using System.Diagnostics;

namespace Drachenkatze.PresetMagician.VSTHost
{
    public class VSTHost
    {
        public void test()
        {
            Debug.WriteLine("start");

            VstPluginContext ctx = OpenPlugin(@"C:\Program Files (x86)\Steinberg\VstPlugins\Novation\V-Station\V-Station x64.dll");
            Debug.WriteLine("loaded");
            ctx.Dispose();
            Debug.WriteLine("disposed, goodbye");
        }

        public VstPluginContext OpenPlugin(string pluginPath)
        {
            try
            {
                HostCommandStub hostCmdStub = new HostCommandStub();
                hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);

                VstPluginContext ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

                // add custom data to the context
                ctx.Set("PluginPath", pluginPath);
                ctx.Set("HostCmdStub", hostCmdStub);

                // actually open the plugin itself
                ctx.PluginCommandStub.Open();

                return ctx;
            }
            catch (Exception e)
            {
                //ssageBox.Show(this, e.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
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
    }
}