using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Syroot.Windows.IO;
using Application = System.Windows.Application;

namespace Drachenkatze.PresetMagician.GUI.GUI
{
    /// <summary>
    ///     Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            SystemCode.Text = App.getSystemInfo();
        }

        private void AppExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GetEvalLicense_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://presetmagician.drachenkatze.org/license/trial");
        }

        private void SelectLicense_Click(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                var downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

                openFileDialog.InitialDirectory = downloadsPath;
                openFileDialog.Filter = "License Files (*.lic)|*.lic";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    if (File.Exists(App.getLicenseFile())) File.Delete(App.getLicenseFile());
                    File.Copy(filePath, App.getLicenseFile());

                    var validationErrors = App.CheckLicense();

                    if (!validationErrors.Any())
                    {
                        var mainWindow = new MainWindow();
                        Application.Current.MainWindow = mainWindow;
                        mainWindow.Show();
                        Close();
                    }
                }
            }
        }
    }
}