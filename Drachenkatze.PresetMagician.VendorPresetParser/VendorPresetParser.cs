using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class VendorPresetParser
    {
        public static IVendorPresetParser GetPresetHandler(VSTPlugin vstPlugin)
        {
            var type = typeof(IVendorPresetParser);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
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

            return new NullPresetParser()
            {
            };
        }
    }
}