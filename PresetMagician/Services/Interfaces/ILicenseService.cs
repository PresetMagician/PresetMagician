using System.Collections.Generic;
using Portable.Licensing;
using Portable.Licensing.Validation;

namespace PresetMagician.Services.Interfaces
{
    public interface ILicenseService
    {
        List<IValidationFailure> ValidateLicense(string filePath);
        List<IValidationFailure> UpdateLicense(string filePath);
        bool CheckLicense();
        License GetCurrentLicense();

        int GetPresetExportLimit();
    }
}