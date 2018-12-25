using System.Collections.Immutable;
using Orc.Squirrel;

namespace PresetMagicianShell
{
    public static class Settings
    {
        #region Links

        public class Links
        {
            private static readonly string _masterSite = "presetmagician.com";
            private static readonly string _masterProtocol = "https://";
            private static readonly string _supportSite = "support.presetmagician.com";

#if DEBUG
            private static readonly string _site = "localhost/presetmagiciansite/public";
            private static readonly string _protocol = "http://";
#else
            private static readonly string _site = "presetmagician.com";
            private static readonly string _protocol = "https://";
            #endif

            public static readonly string Documentation = $"{_masterProtocol}{_supportSite}/documentation";
            public static readonly string Support = $"{_masterProtocol}{_supportSite}/support";
            public static readonly string SupportEmail = "support-confidential@presetmagician.com";
            public static readonly string SupportEmailName = "PresetMagician Support";
            public static readonly string Chat = "https://gitter.im/PresetMagician/general";

            public static readonly string GetTrialLicense = $"{_protocol}{_site}/license/trial";
            public static readonly string SubmitPlugins = $"{_protocol}{_site}/plugins/submit";

            public static readonly string Homepage = $"{_masterProtocol}{_masterSite}";
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
                    new UpdateChannel("Beta", "https://presetmagician.drachenkatze.org/downloads/beta")
                        {IsPrerelease = true},
                    new UpdateChannel("Alpha", "https://presetmagician.drachenkatze.org/downloads/alpha")
                        {IsPrerelease = true}
                );

                public static readonly UpdateChannel DefaultChannel = AvailableChannels[2];
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
            public const string ApplyConfiguration = "Application.ApplyConfiguration";
            public const string NotImplemented = "Application.NotImplemented";
        }

        public static class Plugin
        {
            public const string RefreshPlugins = "Plugin.RefreshPlugins";
            public const string ScanPlugins = "Plugin.ScanPlugins";
            public const string AllToPresetExportList = "Plugin.AllToPresetExportList";
            public const string SelectedToPresetExportList = "Plugin.SelectedToPresetExportList";
            public const string ReportUnsupportedPlugins = "Plugin.ReportUnsupportedPlugins";
            public const string ReportAllPlugins = "Plugin.ReportAllPlugins";
        }

        public static class PluginTools
        {
            public const string EnablePlugins = "PluginTools.EnablePlugins";
            public const string DisablePlugins = "PluginTools.DisablePlugins";
            public const string ShowPluginInfo = "PluginTools.ShowPluginInfo";
            public const string ShowPluginEditor = "PluginTools.ShowPluginEditor";
            public const string ShowPluginChunk = "PluginTools.ShowPluginChunk";
            public const string LoadPlugin = "PluginTools.LoadPlugin";
            public const string UnloadPlugin = "PluginTools.UnloadPlugin";
        }

        public static class Preset
        {
            public const string ActivatePresetView = "Preset.ActivatePresetView";
            public const string Export = "Preset.Export";
            public const string ClearList = "Preset.ClearList";
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