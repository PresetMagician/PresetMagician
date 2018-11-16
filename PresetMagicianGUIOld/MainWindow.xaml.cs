using System.Windows;
using Drachenkatze.PresetMagician.GUI.ViewModels;

namespace Drachenkatze.PresetMagician.GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new VSTPathViewModel();

            App.setStatusBar("1.21 Gigawatts ready to engage.");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void VSTPluginListControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void VSTPresetListControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}