using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Drachenkatze.PresetMagician.GUI.GUI
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            SystemCode.Text = App.getSystemInfo();
        }

        private void AppExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void GetEvalLicense_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://presetmagician.drachenkatze.org/license/trial");
        }

        private void SelectLicense_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

                openFileDialog.InitialDirectory = downloadsPath;
                //openFileDialog.FileName = "PresetMagician License.lic";
                openFileDialog.Filter = "License Files (*.lic)|*.lic";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    File.Copy(filePath, App.getLicenseFile());

                    var validationErrors = App.CheckLicense();

                    if (!validationErrors.Any())
                    {
                        MainWindow mainWindow = new MainWindow();
                        System.Windows.Application.Current.MainWindow = mainWindow;
                        mainWindow.Show();
                        Close();
                    }
                  
                }
            }
        }
    }
}
