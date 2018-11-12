using PresetMagicianGUI.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PresetMagicianGUI.ViewModels;
using PresetMagician.VST;
using Portable.Licensing;
using Portable.Licensing.Validation;
using System.Diagnostics;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X9;
using System.Management;

namespace PresetMagicianGUI
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public void App_Exit(object sender, ExitEventArgs e)
        {
            IEnumerable<string> vstPaths;

            vstPaths = from path in App.vstPaths.VstPaths select path.FullName;

            Settings set = Settings.Default;

            set.AppVstPaths = String.Join(",", vstPaths);

            set.Save();
        }

        public void App_start(object sender, StartupEventArgs e)
        {
            /*FileStream stream = new FileStream(@"C:\Users\Drachenkatze\Desktop\test.xml", FileMode.Open);
            var license = License.Load(stream);
            var validationFailures = license.Validate()
                                .ExpirationDate()
                                    .When(lic => lic.Type == LicenseType.Trial)
                                .And()
                                .Signature(PresetMagicianGUI.Properties.Resources.PublicKey)
                                .AssertValidLicense();
            foreach (var failure in validationFailures)
                Debug.WriteLine(failure.GetType().Name + ": " + failure.Message + " - " + failure.HowToResolve);

            Debug.WriteLine("FOO");

            var signer = SignerUtilities.GetSigner(X9ObjectIdentifiers.ECDsaWithSha512.Id);
            Debug.WriteLine(signer.ToString());

            ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
            string serial = (string)os["SerialNumber"];
            Debug.WriteLine(serial);
            return;*/

            App.vstPaths = new VSTPathViewModel();
            App.vstPlugins = new VSTPluginViewModel();
            App.vstPresets = new VSTPresetViewModel();
            App.vstHost = new VstHost();

            Settings set = Settings.Default;

            IEnumerable<string> vstPaths = set.AppVstPaths.Split(',');

            foreach (string i in vstPaths)
            {
                try
                {
                    App.vstPaths.VstPaths.Add(new DirectoryInfo(i));
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public static void setStatusBar(string status)
        {
            TextBlock textBlock = (TextBlock)Application.Current.MainWindow.FindName("statusMessage");

            textBlock.Text = status;
        }

        public static void activateTab(int index)
        {
            TabControl tabControl = (TabControl)Application.Current.MainWindow.FindName("MainTabs");

            tabControl.SelectedIndex = index;
        }

        public static VSTPathViewModel vstPaths { get; set; }
        public static VSTPluginViewModel vstPlugins { get; set; }
        public static VSTPresetViewModel vstPresets { get; set; }
        public static VstHost vstHost { get; set; }
    }
}