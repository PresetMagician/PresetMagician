using System.Windows;
using Drachenkatze.PresetMagician.GUI.GUI;
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

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ab = new AboutDialog();
            ab.Show();
        }

        private void VSTPluginListControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void VSTPresetListControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            App.setStatusBar("Registered to " + App.license.Customer.Name + " - 1.21 Gigawatts ready to engage.");
            MessageBox.Show("Welcome to the first alpha of PresetMagician! Expect many bugs, many crashes, but apart from that - enjoy :)");
        }
    }
}