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
        private readonly GlobalService _globalService; 

        public RibbonView(GlobalFrontendService globalFrontendService, GlobalService globalService)
        {
            _globalFrontendService = globalFrontendService;
            _globalService = globalService;
            InitializeComponent();
            Ribbon.AddAboutButton();

            _globalFrontendService.ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;
            ScreenTip.HelpPressed += OnScreenTipHelpPressed;
        }

        private void OnScreenTipHelpPressed(object sender, ScreenTipHelpEventArgs e)
        {
            var link = Settings.Links.HelpLink + (string) e.HelpTopic + "?version="+_globalService.PresetMagicianVersion;
            Process.Start(link);
        }

        private void ApplicationStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedRibbonTabIndex")
            {
                Ribbon.SelectedTabIndex = _globalFrontendService.ApplicationState.SelectedRibbonTabIndex;
            }
        }
    }
}