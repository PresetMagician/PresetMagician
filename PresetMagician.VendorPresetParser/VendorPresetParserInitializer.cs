using System.Reflection;
using PresetMagician.Core.Services;

namespace PresetMagician.VendorPresetParser
{
    /// <summary>
    /// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
    /// </summary>
    public static class VendorPresetParserInitializer
    {
        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize(VendorPresetParserService vendorPresetParserService)
        {
            vendorPresetParserService.RegisterPresetParserAssembly(Assembly.GetExecutingAssembly());
            vendorPresetParserService.RegisterNullPresetParserType(typeof(NullPresetParser));
        }
    }
}