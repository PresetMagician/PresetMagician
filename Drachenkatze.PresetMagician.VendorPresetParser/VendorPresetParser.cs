using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Anotar.Catel;
using PresetMagician.Models;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public static class VendorPresetParser
    {
        private static IVendorPresetParser GetPresetHandler(Plugin vstPlugin)
        {
            LogTo.Debug("Resolving PresetHandler for plugin {0}", vstPlugin);

            var type = typeof(IVendorPresetParser);

            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type));


            foreach (var parser in types)
            {
                IVendorPresetParser instance = (IVendorPresetParser) Activator.CreateInstance(parser);

                if (instance.IsNullParser)
                {
                    continue;
                }

                instance.Plugin = vstPlugin;
                if (instance.CanHandle())
                {
                    LogTo.Debug("Using PresetHandler {0} for plugin {1}", instance, vstPlugin);
                    return instance;
                }
            }

            LogTo.Debug("No PresetHandler found for plugin {0}, using NullPresetParser", vstPlugin);
            return new NullPresetParser();
        }

        public static void DeterminatePresetParser(Plugin plugin)
        {
            plugin.PresetParser = GetPresetHandler(plugin);

            if (plugin.PresetParser == null)
            {
                plugin.IsSupported = false;
            }
            else
            {
                plugin.IsSupported = !plugin.PresetParser.IsNullParser;
            }
        }
    }
}