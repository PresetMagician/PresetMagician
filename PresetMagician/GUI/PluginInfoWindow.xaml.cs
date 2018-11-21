using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace Drachenkatze.PresetMagician.GUI.GUI
{
    /// <summary>
    /// Interaction logic for PluginInfoWindow.xaml
    /// </summary>
    public partial class PluginInfoWindow : Window
    {
        public PluginInfoWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}