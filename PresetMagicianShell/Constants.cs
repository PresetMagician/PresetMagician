using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    new UpdateChannel("Stable", "http://downloads.sesolutions.net.au/csvtexteditor/stable"),
                    new UpdateChannel("Beta", "http://downloads.sesolutions.net.au/csvtexteditor/beta"),
                    new UpdateChannel("Alpha", "http://downloads.sesolutions.net.au/csvtexteditor/alpha")
                );

                public static readonly UpdateChannel DefaultChannel = AvailableChannels[0];
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