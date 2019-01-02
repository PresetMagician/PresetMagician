using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Catel.IoC;
using PresetMagician.Services.Interfaces;
using Xceed.Wpf.AvalonDock;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private IRuntimeConfigurationService _runtimeConfigurationService;
        public MainView()
        {
            InitializeComponent();
            
            var serviceLocator = ServiceLocator.Default;

            serviceLocator.RegisterInstance(DockingManager);
            serviceLocator.RegisterInstance(LayoutDocumentPane);
            serviceLocator.RegisterInstance(PluginSettings, "PluginSettings");

            _runtimeConfigurationService = serviceLocator.ResolveType<IRuntimeConfigurationService>();

            PluginSettings.PropertyChanged += OnPluginSettingsPropertyChanged;

        }

        private void OnPluginSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsAutoHidden")
            {
                Debug.WriteLine(PluginSettings.IsAutoHidden);
                _runtimeConfigurationService.ApplicationState.IsPluginSettingsVisible = !PluginSettings.IsAutoHidden;
            }
        } 
    }
}