using System;
using System.Collections.Generic;
using Portable.Licensing;
using Portable.Licensing.Validation;

namespace PresetMagicianShell.Services
{
    public interface ILicenseService
    {
        List<IValidationFailure> ValidateLicense(string filePath);
        List<IValidationFailure> UpdateLicense(string filePath);
        bool CheckLicense();
        License GetCurrentLicense();

        event EventHandler LicenseChanged;
    }
}