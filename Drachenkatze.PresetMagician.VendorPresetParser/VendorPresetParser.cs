using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class VendorPresetParser
    {
        public static Type[] GetAllTypesSafely(Assembly assembly, bool logLoaderExceptions)
        {
            Type[] foundAssemblyTypes;

            try
            {
                foundAssemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                foundAssemblyTypes = (from type in typeLoadException.Types
                    where type != null
                    select type).ToArray();

                Debug.WriteLine(
                    "A ReflectionTypeLoadException occured, adding all {0} types that were loaded correctly",
                    foundAssemblyTypes.Length);

                if (logLoaderExceptions)
                {
                    Debug.WriteLine("The following loading exceptions occurred:");
                    foreach (var error in typeLoadException.LoaderExceptions)
                    {
                        Debug.WriteLine("  " + error.Message);
                    }
                }
            }

            return foundAssemblyTypes;
        }

        public static IVendorPresetParser GetPresetHandler(IVstPlugin vstPlugin)
        {
            LogTo.Debug("Resolving PresetHandler for plugin {0}", vstPlugin);

            var type = typeof(IVendorPresetParser);
            IEnumerable<Type> types = null;

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type));


            foreach (var parser in types)
            {
                IVendorPresetParser instance = (IVendorPresetParser) Activator.CreateInstance(parser);

                if (instance.IsNullParser)
                {
                    continue;
                }

                instance.VstPlugin = vstPlugin;
                if (instance.CanHandle())
                {
                    LogTo.Debug("Using PresetHandler {0} for plugin {1}", instance, vstPlugin);
                    return instance;
                }
            }

            LogTo.Debug("No PresetHandler found for plugin {0}, using NullPresetParser", vstPlugin);
            return new NullPresetParser();
        }
    }
}