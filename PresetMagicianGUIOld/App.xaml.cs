using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Drachenkatze.PresetMagician.GUI.Properties;
using Drachenkatze.PresetMagician.GUI.ViewModels;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Newtonsoft.Json;
using Platform.Text;
using Portable.Licensing;
using Portable.Licensing.Validation;

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

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
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

    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public void App_start(object sender, StartupEventArgs e)
        {
            App.vstPaths = new VSTPathViewModel();
            App.vstPlugins = new VSTPluginViewModel();
            App.vstPresets = new VSTPresetViewModel();
            App.vstHost = new VstHost();

            readVSTPathsFromConfig();
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

        public void App_start2(object sender, StartupEventArgs e)
        {
            FileStream stream = new FileStream(@"C:\Users\Drachenkatze\Desktop\test.xml", FileMode.Open);
            var license = License.Load(stream);
            var validationFailures = license.Validate()
                                .ExpirationDate()
                                    .When(lic => lic.Type == LicenseType.Trial)
                                .And()
                                .Signature(GUI.Properties.Resources.PublicKey)
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

            SystemCodeInfo systemInfo = new SystemCodeInfo();

            string output = JsonConvert.SerializeObject(systemInfo);
            Debug.WriteLine(TextConversion.ToBase64String(Encoding.ASCII.GetBytes(output)));

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