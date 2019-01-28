using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Catel.Logging;
using MethodTimer;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public static class VendorPresetParser
    {
        public static Dictionary<int, IVendorPresetParser> GetPresetHandlerList()
        {
            var pluginHandlers = new Dictionary<int, IVendorPresetParser>();


            foreach (var parser in GetPresetParsers())
            {
                foreach (var pluginId in parser.GetSupportedPlugins())
                {
                    pluginHandlers.Add(pluginId, parser);
                }
            }

            return pluginHandlers;
        }

        private static List<IVendorPresetParser> _presetParsers;

        public static List<IVendorPresetParser> GetPresetParsers()
        {
            if (_presetParsers != null)
            {
                return _presetParsers;
            }

            _presetParsers = new List<IVendorPresetParser>();

            var interfaceType = typeof(IVendorPresetParser);

            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(interfaceType));

            foreach (var parser in types)
            {
                IVendorPresetParser instance = (IVendorPresetParser) Activator.CreateInstance(parser);
                _presetParsers.Add(instance);
            }

            return _presetParsers;
        }

        [Time]
        private static IVendorPresetParser GetPresetHandler(IRemotePluginInstance pluginInstance)
        {
            pluginInstance.Plugin.Logger.Debug("Resolving Preset Parser");

            var orderedPresetParsers = GetPresetParsers().ToList();


            foreach (var parser in orderedPresetParsers)
            {
                parser.Init();

                if (parser.IsNullParser)
                {
                    continue;
                }

                parser.PluginInstance = pluginInstance;

                if (parser.CanHandle())
                {
                    pluginInstance.Plugin.Logger.Debug("Using PresetHandler {0}",
                        parser.PresetParserType);

                    return parser;
                }
            }

            pluginInstance.Plugin.Logger.Debug("No PresetHandler found, using NullPresetParser");
            return new NullPresetParser();
        }

        public static void DeterminatePresetParser(IRemotePluginInstance pluginInstance)
        {
            pluginInstance.Plugin.PresetParser = GetPresetHandler(pluginInstance);

            if (pluginInstance.Plugin.PresetParser == null)
            {
                pluginInstance.Plugin.IsSupported = false;
            }
            else
            {
                pluginInstance.Plugin.IsSupported = !pluginInstance.Plugin.PresetParser.IsNullParser;
            }
        }
    }
}