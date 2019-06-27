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
using PresetMagician.Core.Services;
using PresetMagician.Properties;
using PresetMagician.Services.Interfaces;
using Path = Catel.IO.Path;

namespace PresetMagician.Services
{
    public class LicenseService : ILicenseService
    {
        private const int _defaultTrialPresetExportLimit = 50;

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly GlobalFrontendService _globalFrontendService;

        private static readonly string _defaultLocalLicenseFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming), "license.lic");

        public LicenseService(GlobalFrontendService globalFrontendService)
        {
            Argument.IsNotNull(() => globalFrontendService);

            _globalFrontendService = globalFrontendService;
            _globalFrontendService.ApplicationState.SystemCode = SystemCodeInfo.getSystemInfo();
        }

        public License CurrentLicense { get; private set; }
        public License ValidatingLicense { get; private set; }


        public License GetCurrentLicense()
        {
            return CurrentLicense;
        }

        public int GetPresetExportLimit()
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
            ValidatingLicense = License.Load(stream);

            var validationFailures = ValidatingLicense.Validate()
                .ExpirationDate()
                .When(lic => lic.Type == LicenseType.Trial)
                .And()
                .Signature(Resources.PublicKey)
                .AssertValidLicense().ToList();

            stream.Close();

            /*if (ValidatingLicense.AdditionalAttributes.Contains("SystemCode"))
            {
                var licenseSystemCode = ValidatingLicense.AdditionalAttributes.Get("SystemCode");

                if (licenseSystemCode != SystemCodeInfo.getSystemInfo())
                {
                    var fail = new GeneralValidationFailure
                    {
                        Message =
                            "System Code does not match this system. Please create a new license at presetmagician.com"
                    };
                    validationFailures.Add(fail);
                }
            }
            else
            {
                if (!ValidatingLicense.AdditionalAttributes.Contains("IgnoreSystemCode"))
                {
                    var fail = new GeneralValidationFailure
                    {
                        Message =
                            "This license does not contain a system code. Please create a new license at presetmagician.com"
                    };
                    validationFailures.Add(fail);
                }
            }*/

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
            CurrentLicense = ValidatingLicense;
            _globalFrontendService.ApplicationState.ValidLicense = true;
            _globalFrontendService.ApplicationState.ActiveLicense = CurrentLicense;
            _globalFrontendService.ApplicationState.PresetExportLimit = GetPresetExportLimit();

            return updateLicense;
        }

        public bool CheckLicense()
        {
            try
            {
                var validationFailures = ValidateLicense(_defaultLocalLicenseFilePath);

                if (!validationFailures.Any())
                {
                    CurrentLicense = ValidatingLicense;
                    _globalFrontendService.ApplicationState.ValidLicense = true;
                    _globalFrontendService.ApplicationState.ActiveLicense = CurrentLicense;
                    _globalFrontendService.ApplicationState.PresetExportLimit = GetPresetExportLimit();

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

            public string SystemCode => getSystemHash();

            public string getSystemHash()
            {
                ManagementObject os = new ManagementObject("Win32_OperatingSystem=@");
                string serial = (string) os["SerialNumber"];

                return HashUtils.getFormattedSHA256Hash(serial);
            }

            public static string getSystemInfo()
            {
                SystemCodeInfo systemInfo = new SystemCodeInfo();

                string output = JsonConvert.SerializeObject(systemInfo);
                return Convert.ToBase64String(Encoding.ASCII.GetBytes(output));
            }
        }
    }
}