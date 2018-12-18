using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using Catel;
using Catel.IO;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using Portable.Licensing;
using Portable.Licensing.Validation;
using PresetMagicianShell.Properties;
using PresetMagicianShell.Services.EventArgs;
using Path = Catel.IO.Path;

namespace PresetMagicianShell.Services
{
    public class LicenseService : ILicenseService
    {
        private const int _defaultTrialPresetExportLimit = 50;

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly string _defaultLocalLicenseFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming), "license.lic");

        public License CurrentLicense { get; private set; }

        public event EventHandler LicenseChanged;

        public License GetCurrentLicense()
        {
            return CurrentLicense;
        }

        public int getPresetExportLimit()
        {
            if (CurrentLicense.Type == LicenseType.Standard)
            {
                return 0;
            }

            if (!CurrentLicense.AdditionalAttributes.Contains("PresetExportLimit"))
            {
                return _defaultTrialPresetExportLimit;
            }
            else
            {
                var presetExportLimit = 0;
                if (!Int32.TryParse(CurrentLicense.AdditionalAttributes.Get("PresetExportLimit"),
                    out presetExportLimit))
                {
                    return presetExportLimit;
                }
                
                
            }

            return _defaultTrialPresetExportLimit;
        }

        public List<IValidationFailure> ValidateLicense(string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.Open);
            CurrentLicense = License.Load(stream);

            var validationFailures = CurrentLicense.Validate()
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

            if (File.Exists(_defaultLocalLicenseFilePath))
            {
                File.Delete(_defaultLocalLicenseFilePath);
            }

            File.Copy(filePath, _defaultLocalLicenseFilePath);
            LicenseChanged.SafeInvoke(this);

            return updateLicense;
        }

        public bool CheckLicense()
        {
            try
            {
                var validationFailures = ValidateLicense(_defaultLocalLicenseFilePath);

                if (!validationFailures.Any())
                {
                    LicenseChanged.SafeInvoke(this);
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