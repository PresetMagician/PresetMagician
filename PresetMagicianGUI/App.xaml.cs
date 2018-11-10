using PresetMagicianGUI.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PresetMagicianGUI.ViewModels;
using PresetMagician.VST;

namespace PresetMagicianGUI
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public void App_Exit(object sender, ExitEventArgs e)
        {
            IEnumerable<string> vstPaths;

            vstPaths = from path in App.vstPaths.VstPaths select path.FullName;

            Settings set = Settings.Default;

            set.AppVstPaths = String.Join(",", vstPaths);

            set.Save();
        }

        public void App_start(object sender, StartupEventArgs e)
        {
            App.vstPaths = new VSTPathViewModel();
            App.vstPlugins = new VSTPluginViewModel();
            App.vstPresets = new VSTPresetViewModel();
            App.vstHost = new VstHost();

            Settings set = Settings.Default;

            IEnumerable<string> vstPaths = set.AppVstPaths.Split(',');

            foreach (string i in vstPaths)
            {
                try
                {
                    App.vstPaths.VstPaths.Add(new DirectoryInfo(i));
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public static void setStatusBar(string status)
        {
            TextBlock textBlock = (TextBlock)Application.Current.MainWindow.FindName("statusMessage");

            textBlock.Text = status;
        }

        public static void activateTab(int index)
        {
            TabControl tabControl = (TabControl)Application.Current.MainWindow.FindName("MainTabs");

            tabControl.SelectedIndex = index;
        }

        public static VSTPathViewModel vstPaths { get; set; }
        public static VSTPluginViewModel vstPlugins { get; set; }
        public static VSTPresetViewModel vstPresets { get; set; }
        public static VstHost vstHost { get; set; }
    }
}