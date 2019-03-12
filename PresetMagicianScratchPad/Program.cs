using System;
using System.Collections.Generic;
using Ceras;

namespace PresetMagicianScratchPad
{
    public class PluginLocation
    {
        public string PluginName { get; set; }
    }

    public class Plugin
    {
        public PluginLocation PluginLocation { get; set; }
    }

    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var pluginLocations = new List<PluginLocation>();
            var pluginLocation = new PluginLocation();

            pluginLocations.Add(pluginLocation);
            var plugin = new Plugin();
            plugin.PluginLocation = pluginLocation;
            var plugin2 = new Plugin();
            plugin2.PluginLocation = pluginLocation;

            var serializerConfig = new SerializerConfig();
            serializerConfig.DefaultTargets = TargetMember.None;
            var s = new CerasSerializer(serializerConfig);

            var data = s.Serialize(plugin);
            var data2 = s.Serialize(plugin2);
            var data3 = s.Serialize(pluginLocations);
            
            s = new CerasSerializer(serializerConfig);

            s.Deserialize<List<PluginLocation>>(data3);
            s.Deserialize<Plugin>(data);
            s.Deserialize<Plugin>(data2);
        }
    }
}