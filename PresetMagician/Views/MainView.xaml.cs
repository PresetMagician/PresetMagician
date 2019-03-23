using System.ComponentModel;
using Catel.IoC;
using PresetMagician.Core.Services;
using PresetMagician.Models;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();

            var serviceLocator = ServiceLocator.Default;

            serviceLocator.RegisterInstance(DockingManager);
            serviceLocator.RegisterInstance(LayoutDocumentPane);
            
        }
    }
}