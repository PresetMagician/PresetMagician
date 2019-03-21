using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Fluent;
using Orchestra;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for RibbonView.xaml
    /// </summary>
    public partial class RibbonView
    {
        private readonly GlobalFrontendService _globalFrontendService;


        public RibbonView(GlobalFrontendService globalFrontendService)
        {
            _globalFrontendService = globalFrontendService;
            InitializeComponent();
            Ribbon.AddAboutButton();

            _globalFrontendService.ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;
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
                Ribbon.SelectedTabIndex = _globalFrontendService.ApplicationState.SelectedRibbonTabIndex;
            }

            if (e.PropertyName == nameof(ApplicationState.ShowPresetsRibbon))
            {
                if (_globalFrontendService.ApplicationState.ShowPresetsRibbon)
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