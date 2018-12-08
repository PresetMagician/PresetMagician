using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

                Debug.WriteLine("A ReflectionTypeLoadException occured, adding all {0} types that were loaded correctly", foundAssemblyTypes.Length);

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

        public static IVendorPresetParser GetPresetHandler(VSTPlugin vstPlugin)
        {
            var type = typeof(IVendorPresetParser);
            IEnumerable<Type> types = null;

            Assembly currentAssem = Assembly.GetExecutingAssembly();

                types = currentAssem.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type));
          

            foreach (var parser in types)
            {
                IVendorPresetParser instance = (IVendorPresetParser)Activator.CreateInstance(parser);

                if (instance.IsNullParser)
                {
                    continue;
                }

                instance.VstPlugin = vstPlugin;
                if (instance.CanHandle())
                {
                    return instance;
                }
            }

            return new NullPresetParser(); 
        }
    }
}