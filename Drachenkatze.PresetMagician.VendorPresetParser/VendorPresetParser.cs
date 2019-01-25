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
        public static Dictionary<int, IVendorPresetParser> GetPresetHandlerList()
        {
            var pluginHandlers = new Dictionary<int, IVendorPresetParser>();
            var type = typeof(IVendorPresetParser);
            
            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type));

            foreach (var parser in types)
            {
                IVendorPresetParser instance = (IVendorPresetParser) Activator.CreateInstance(parser);
                foreach (var pluginId in instance.GetSupportedPlugins())
                {
                    pluginHandlers.Add(pluginId, instance);
                }
            }

            return pluginHandlers;
        }
        private static IVendorPresetParser GetPresetHandler(Plugin vstPlugin, IRemoteVstService remoteVstService)
        {
            vstPlugin.Debug("Resolving PresetHandler for plugin {0}", vstPlugin);

            var type = typeof(IVendorPresetParser);

            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type));


            foreach (var parser in types)
            {
                IVendorPresetParser instance = (IVendorPresetParser) Activator.CreateInstance(parser);
                instance.RemoteVstService = remoteVstService;
                instance.Init();

                if (instance.IsNullParser)
                {
                    continue;
                }

                instance.Plugin = vstPlugin;
                if (instance.CanHandle())
                {
                    vstPlugin.Debug("Using PresetHandler {0} for plugin {1}", instance, vstPlugin);

                    return instance;
                }
            }

            vstPlugin.Debug("No PresetHandler found for plugin {0}, using NullPresetParser", vstPlugin);
            return new NullPresetParser();
        }

        public static void DeterminatePresetParser(Plugin plugin, IRemoteVstService remoteVstService)
        {
            plugin.PresetParser = GetPresetHandler(plugin, remoteVstService);

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