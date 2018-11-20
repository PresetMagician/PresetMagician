using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Start(object sender, StartupEventArgs e)
        {
            var vstPlugin = new VSTPlugin(@"C:\Users\Drachenkatze\Documents\deleteme\Plex64.dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);
            Debug.WriteLine(plugin.PresetSaveModeDescription);
            vstHost.UnloadVST(plugin);
        }
    }
}