using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Catel;
using Fluent;
using Orchestra;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for RibbonView.xaml
    /// </summary>
    public partial class RibbonView
    {
        private IRuntimeConfigurationService _runtimeConfigurationService;


        public RibbonView(IRuntimeConfigurationService runtimeConfigurationService)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);

            InitializeComponent();
            Ribbon.AddAboutButton();

            _runtimeConfigurationService = runtimeConfigurationService;

            _runtimeConfigurationService.ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;
            ScreenTip.HelpPressed += OnScreenTipHelpPressed;
        }
        private static void OnScreenTipHelpPressed(object sender, ScreenTipHelpEventArgs e)
        {
            var link = Settings.Links.HelpLink + (string) e.HelpTopic;
            Process.Start(link);
        }

        private void ApplicationStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedRibbonTabIndex")
            {
                Ribbon.SelectedTabIndex = _runtimeConfigurationService.ApplicationState.SelectedRibbonTabIndex;
            }

            if (e.PropertyName == nameof(ApplicationState.ShowPresetsRibbon))
            {
                if (_runtimeConfigurationService.ApplicationState.ShowPresetsRibbon)
                {
                    PresetsTabGroup.Visibility = Visibility.Visible;
                }
                else
                {
                    PresetsTabGroup.Visibility = Visibility.Collapsed;
                }
                
            }
        }
    }
}