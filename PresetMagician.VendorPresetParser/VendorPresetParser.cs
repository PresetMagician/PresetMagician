using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Anotar.Catel;
using MethodTimer;
using SharedModels;
using SharedModels.Exceptions;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public static class VendorPresetParser
    {
        public static Dictionary<int, IVendorPresetParser> GetPresetHandlerListByPlugin()
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

        private static List<IVendorPresetParser> _presetParsersCache;

        public static List<IVendorPresetParser> GetPresetParsers()
        {
            if (_presetParsersCache != null)
            {
                return _presetParsersCache;
            }

            _presetParsersCache = new List<IVendorPresetParser>();

            var interfaceType = typeof(IVendorPresetParser);

            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(interfaceType));

            foreach (var parser in types)
            {
                var instance = (IVendorPresetParser) Activator.CreateInstance(parser);
                _presetParsersCache.Add(instance);
            }

            return _presetParsersCache;
        }

        [Time]
        private static IVendorPresetParser GetPresetParser(IRemotePluginInstance pluginInstance)
        {
            pluginInstance.Plugin.Logger.Debug("Resolving Preset Parser");

            var list = GetPresetHandlerListByPlugin();
            if (list.ContainsKey(pluginInstance.Plugin.VstPluginId))
            {
                var directlyFoundParser = list[pluginInstance.Plugin.VstPluginId];
                directlyFoundParser.PluginInstance = pluginInstance;
                directlyFoundParser.Init();
                
                if (directlyFoundParser.CanHandle())
                {
                    pluginInstance.Plugin.Logger.Debug($"Directly found PresetHandler {directlyFoundParser.PresetParserType}");

                    return directlyFoundParser;
                }
            }
            
            var orderedPresetParsers = GetPresetParsers();

            foreach (var parser in orderedPresetParsers)
            {
                parser.PluginInstance = pluginInstance;

                try
                {
                    parser.Init();

                    if (parser.IsNullParser)
                    {
                        continue;
                    }

                    if (!parser.CanHandle())
                    {
                        continue;
                    }

                    pluginInstance.Plugin.Logger.Debug($"Using PresetHandler {parser.PresetParserType}");
                }
                catch (ConnectivityLostException e)
                {
                    // Let the upstream caller handle this
                    throw e;
                }
                catch (Exception e)
                {
                    LogTo.Error($"Error while trying to use PresetParser {parser.GetType().FullName}: {e.GetType().FullName} {e.Message}.");
                    LogTo.Debug(e.StackTrace);
                    continue;
                }

                return parser;
            }

            pluginInstance.Plugin.Logger.Debug("No PresetHandler found, using NullPresetParser");
            return new NullPresetParser();
        }

        public static void DeterminatePresetParser(IRemotePluginInstance pluginInstance)
        {
            if (!pluginInstance.Plugin.HasMetadata)
            {
                throw new NoMetadataAvailableException(pluginInstance);
            }
            
            pluginInstance.Plugin.PresetParser = GetPresetParser(pluginInstance);

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