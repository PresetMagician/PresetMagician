using System.Windows;
using Drachenkatze.PresetMagician.GUI.GUI;
using Drachenkatze.PresetMagician.GUI.ViewModels;

namespace Drachenkatze.PresetMagician.GUI
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var ab = new AboutDialog();
            ab.ShowDialog();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            string statusBarMessage;

            if (App.license.Type == Portable.Licensing.LicenseType.Trial)
            {
                statusBarMessage = "Trial for " + App.license.Customer.Name;
                statusBarMessage += " (Expires on " + App.license.Expiration.ToShortDateString() + ")";
            } else
            {
                statusBarMessage = "Registered to " + App.license.Customer.Name;
            }

            statusBarMessage += " - 1.21 Gigawatts ready to engage.";
            App.setStatusBar(statusBarMessage);
            MessageBox.Show("Welcome to the first alpha of PresetMagician! Expect many bugs, many crashes, but apart from that - enjoy :)");
        }

        private void NKSFInspector_Click(object sender, RoutedEventArgs e)
        {
            var win = new NKSFViewer();
            win.Show();
        }
    }
}