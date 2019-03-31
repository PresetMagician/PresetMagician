using System;
using System.Reflection;
using PresetMagician.Core.Services;
using PresetMagician.Utils;

namespace PresetMagician.VendorPresetParser
{
    public static class MethodTimeLogger
    {
         
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            GlobalMethodTimeLogger.Log(methodBase, elapsed);
           
        }

        
    }
    /// <summary>
    ///     Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
    /// </summary>
    public static class VendorPresetParserInitializer
    {
        /// <summary>
        ///     Initializes the module.
        /// </summary>
        public static void Initialize(VendorPresetParserService vendorPresetParserService)
        {
            vendorPresetParserService.RegisterPresetParserAssembly(Assembly.GetExecutingAssembly());
            vendorPresetParserService.RegisterNullPresetParserType(typeof(NullPresetParser));
        }
    }
}