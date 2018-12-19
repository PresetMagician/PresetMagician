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
        #region Links
        public class Links
        {
            private static string MasterSite = "presetmagician.com";
            private static string MasterProtocol = "https://";
            private static string SupportSite = "support.presetmagician.com";

            #if DEBUG
            private static string Site = "localhost/presetmagiciansite/public";
            private static string Protocol = "http://";
            #else
            private static string Site = "presetmagician.com"
            private static string Protocol = "https://";
            #endif

            public static readonly string Documentation = $"{MasterProtocol}{SupportSite}/documentation";
            public static readonly string Support = $"{MasterProtocol}{SupportSite}/support";
            public static readonly string SupportEmail = "support-confidential@presetmagician.com";
            public static readonly string SupportEmailName = "PresetMagician Support";
            public static readonly string Chat = $"https://gitter.im/PresetMagician/general";

            public static readonly string GetTrialLicense = $"{Protocol}{Site}/license/trial";
            public static readonly string SubmitPlugins = $"{Protocol}{Site}/plugins/submit";
        }
        #endregion

        #region Application
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

            public static class Directories
            {
                public const string VstDirectories = "Directories.VstDirectories"; 
            }
            
        }
    }
    #endregion

    #region Commands
    public static class Commands
    {
        public static class Application
        {
            public const string CancelOperation = "Application.CancelOperation";
            public const string ClearLastOperationErrors = "Application.ClearLastOperationErrors";
        }

        public static class Plugin
        {
            public const string RefreshPlugins = "Plugin.RefreshPlugins";
            public const string ScanPlugins = "Plugin.ScanPlugins";
        }
        public static class Tools
        {
            public const string NksfView = "Tools.NksfView";
            public const string SettingsView = "Tools.SettingsView";
        }

        public static class Help
        {
            public const string OpenSupportLink = "Help.OpenSupportLink";
            public const string OpenChatLink = "Help.OpenChatLink";
            public const string OpenDocumentationLink = "Help.OpenDocumentationLink";
        }
    }

    #endregion
}