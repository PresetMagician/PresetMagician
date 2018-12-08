using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using Catel.IO;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using Portable.Licensing;
using Portable.Licensing.Validation;
using PresetMagicianShell.Properties;
using Path = Catel.IO.Path;

namespace PresetMagicianShell.Services
{
    public class LicenseService : ILicenseService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly string DefaultLocalLicenseFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserLocal), "license.lic");

        private License license;

        public List<IValidationFailure> ValidateLicense(string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.Open);
            license = License.Load(stream);

            var validationFailures = license.Validate()
                .ExpirationDate()
                .When(lic => lic.Type == LicenseType.Trial)
                .And()
                .Signature(Resources.PublicKey)
                .AssertValidLicense().ToList();

            stream.Close();

            foreach (var validationFailure in validationFailures)
            {
                Log.Warning("License validation failure: " + validationFailure.Message);
            }

            return validationFailures;
        }

        public List<IValidationFailure> UpdateLicense(string filePath)
        {
            var validationFailures = ValidateLicense(filePath);

            var updateLicense = validationFailures.ToList();

            if (updateLicense.Any())
            {
                return updateLicense;
            }

            if (File.Exists(DefaultLocalLicenseFilePath))
            {
                File.Delete(DefaultLocalLicenseFilePath);
            }

            File.Copy(filePath, DefaultLocalLicenseFilePath);

            return updateLicense;
        }

        public bool CheckLicense()
        {
            try
            {
                var validationFailures = ValidateLicense(DefaultLocalLicenseFilePath);

                if (!validationFailures.Any())
                {
                    return true;
                }
            }
            catch (FileNotFoundException)
            {
            }

            return false;
        }

        public class SystemCodeInfo
        {
            public string MachineName => Environment.MachineName;

            public String SystemCode => getSystemHash();

            public String getSystemHash()
            {
                ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
                string serial = (string) os["SerialNumber"];

                return HashUtils.getFormattedSHA256Hash(serial);
            }

            public static String getSystemInfo()
            {
                SystemCodeInfo systemInfo = new SystemCodeInfo();

                string output = JsonConvert.SerializeObject(systemInfo);
                return Convert.ToBase64String(Encoding.ASCII.GetBytes(output));
            }
        }
    }
}