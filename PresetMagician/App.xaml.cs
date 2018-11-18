using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Drachenkatze.PresetMagician.GUI.GUI;
using Drachenkatze.PresetMagician.GUI.Properties;
using Drachenkatze.PresetMagician.GUI.ViewModels;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Newtonsoft.Json;
using Platform.Text;
using Portable.Licensing;
using Portable.Licensing.Validation;
using SplashScreen = Drachenkatze.PresetMagician.GUI.GUI.SplashScreen;

namespace Drachenkatze.PresetMagician.GUI
{
    public class SystemCodeInfo
    {
        public SystemCodeInfo()
        {
        }

        public String MachineName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        public String SystemCode
        {
            get
            {
                return getSystemHash();
            }
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public String getSystemHash()
        {
            ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
            string serial = (string)os["SerialNumber"];

            SHA256 sha256Hash = SHA256.Create();

            return GetHash(sha256Hash, serial);
        }
    }

    public partial class App : Application
    {
        private SplashScreen splash;

        private delegate void StringParameterDelegate(string value);

        private readonly object stateLock = new object();

        private async Task DoSomeWork()
        {
            for (int i = 0; i < 100; i++)
            {
                splash.setSplashMessage("Initializing Kittens..." + i);
                Thread.Sleep(50);
            }
        }

        public void App_start(object sender, StartupEventArgs e)
        {
            /*splash = new SplashScreen();
            splash.Show();*/

            App.vstPaths = new VSTPathViewModel();
            App.vstPlugins = new VSTPluginViewModel();
            App.vstPresets = new VSTPresetViewModel();
            App.vstHost = new VstHost();

            readVSTPathsFromConfig();

            try
            {
                var validationFailures = CheckLicense();

                if (!validationFailures.Any())
                {
                    var mw = new MainWindow();
                    mw.Show();
                }
                else
                {
                    var regWindow = new RegistrationWindow();
                    regWindow.Show();
                }
            }
            catch (FileNotFoundException)
            {
                var regWindow = new RegistrationWindow();
                regWindow.Show();
            }
        }

        public static string getLicenseFile()
        {
            var appDataDir = getAppDataDir();
            return Path.Combine(appDataDir.FullName, "license.lic");
        }

        public static License license;

        public static IEnumerable<IValidationFailure> CheckLicense()
        {
            var licenseFile = getLicenseFile();

            FileStream stream = new FileStream(licenseFile, FileMode.Open);
            license = License.Load(stream);

            var validationFailures = license.Validate()
                                .ExpirationDate()
                                    .When(lic => lic.Type == LicenseType.Trial)
                                .And()
                                .Signature(Drachenkatze.PresetMagician.GUI.Properties.Resources.PublicKey)
                                .AssertValidLicense();

            return validationFailures;
        }

        public static DirectoryInfo getAppDataDir()
        {
            DirectoryInfo appData = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Drachenkatze\PresetMagician"));
            if (!appData.Exists)
            {
                appData.Create();
            }
            return appData;
        }

        private void readVSTPathsFromConfig()
        {
            Settings set = Settings.Default;

            IEnumerable<string> vstPaths = set.VstPluginPaths.Split(',');

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

        public void App_Exit(object sender, ExitEventArgs e)
        {
            IEnumerable<string> vstPaths;

            vstPaths = from path in App.vstPaths.VstPaths select path.FullName;

            Settings set = Settings.Default;

            set.VstPluginPaths = String.Join(",", vstPaths);

            set.Save();
        }

        public static String getSystemInfo()
        {
            SystemCodeInfo systemInfo = new SystemCodeInfo();

            string output = JsonConvert.SerializeObject(systemInfo);
            return TextConversion.ToBase64String(Encoding.ASCII.GetBytes(output));
        }

        public void App_start2(object sender, StartupEventArgs e)
        {
            FileStream stream = new FileStream(@"C:\Users\Drachenkatze\Desktop\test.xml", FileMode.Open);
            var license = License.Load(stream);
            var validationFailures = license.Validate()
                                .ExpirationDate()
                                    .When(lic => lic.Type == LicenseType.Trial)
                                .And()
                                .Signature(Drachenkatze.PresetMagician.GUI.Properties.Resources.PublicKey)
                                .AssertValidLicense();

            foreach (var failure in validationFailures)
            {
                Debug.WriteLine(failure.GetType().Name + ": " + failure.Message + " - " + failure.HowToResolve);
            }

            if (!validationFailures.Any())
            {
                Debug.WriteLine("License OK!");
            }
            stream.Close();

            /*var signer = SignerUtilities.GetSigner(X9ObjectIdentifiers.ECDsaWithSha512.Id);
            Debug.WriteLine(signer.ToString());

            ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
            string serial = (string)os["SerialNumber"];
            Debug.WriteLine(serial);*/
            return;
        }

        public static void setStatusBar(string status)
        {
            TextBlock textBlock = (TextBlock)Application.Current.MainWindow.FindName("statusMessage");

            //textBlock.Text = status;
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