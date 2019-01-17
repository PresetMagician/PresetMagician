using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.VstHost.Util;
using SharedModels;

namespace Drachenkatze.PresetMagician.VSTHost
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var host = new VstHost();

            Directory.CreateDirectory("pngs");

            foreach (var file in Directory.EnumerateFiles(@"C:\Program Files\VstPlugins\", "*.dll", SearchOption.AllDirectories))
            {
                var plugin = new Plugin();
               
                    plugin.DllPath = file;
                    Debug.WriteLine(plugin.DllPath);

                    host.LoadVST(plugin);
                    /*var img = ScreenCapture.CaptureVstScreenshot(plugin.PluginContext);
                    img.Save(@"pngs\" + plugin.DllFilename.Replace(".dll", "") + ".png", ImageFormat.Png);*/
                

                host.UnloadVST(plugin);
            }
        }
    }
}