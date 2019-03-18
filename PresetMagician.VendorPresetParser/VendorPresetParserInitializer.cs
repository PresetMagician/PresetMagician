using System.Reflection;
using PresetMagician.Core.Services;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    /// <summary>
    /// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
    /// </summary>

    public static class VendorPresetParserInitializer
    {
        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            VendorPresetParserService.RegisterPresetParserAssembly(Assembly.GetExecutingAssembly());
            VendorPresetParserService.RegisterNullPresetParserType(typeof(NullPresetParser));
        }
    }
}