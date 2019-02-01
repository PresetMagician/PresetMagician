using System.ComponentModel;
using System.Diagnostics;
using Catel;
using Fluent;
using Orchestra;
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
            ribbon.AddAboutButton();

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
                ribbon.SelectedTabIndex = _runtimeConfigurationService.ApplicationState.SelectedRibbonTabIndex;
            }
        }
    }
}