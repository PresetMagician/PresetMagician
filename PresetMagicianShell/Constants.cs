using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Catel.Windows.Input;
using Orc.Squirrel;

namespace PresetMagicianShell
{
    public static class Settings
    {
        public static class Application
        {
            public static class AutomaticUpdates
            {
                public const bool CheckForUpdatesDefaultValue = true;

                public static readonly ImmutableArray<UpdateChannel> AvailableChannels = ImmutableArray.Create(
                    new UpdateChannel("Stable", "https://presetmagician.drachenkatze.org/downloads/stable"),
                    new UpdateChannel("Beta", "https://presetmagician.drachenkatze.org/downloads/beta") { IsPrerelease = true },
                    new UpdateChannel("Alpha", "https://presetmagician.drachenkatze.org/downloads/alpha") { IsPrerelease = true }
                );

                public static readonly UpdateChannel DefaultChannel = AvailableChannels[2];
            }
        }
    }

    public static class Commands
    {
        public static class Tools
        {
            public const string NksfView = "Tools.NksfView";
        }
    }
}