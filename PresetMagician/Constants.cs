using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Orc.Squirrel;

namespace PresetMagician
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HelpLinks
    {
        public static string COMMANDS_ANALYZE => "COMMANDS_ANALYZE";
        public static string COMMANDS_COMPRESS => "COMMANDS_COMPRESS";
        public static string SETTINGS_PLUGIN_DLL => "SETTINGS_PLUGIN_DLL";
        public static string SETTINGS_PLUGIN_FXBFXPNOTES = "SETTINGS_PLUGIN_FXBFXPNOTES";
        public static string CONCEPTS_VST_WORKER_POOL => "CONCEPTS_VST_WORKER_POOL";
    }
}


namespace PresetMagician
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

            public static readonly string Documentation = $"https://presetmagician.gitbook.io/help/";
            public static readonly string HelpLink = $"https://presetmagician.com/help/";
            public static readonly string Support = $"{_masterProtocol}{_supportSite}/support";
            public static readonly string SupportEmail = "support-confidential@presetmagician.com";
            public static readonly string SupportEmailName = "PresetMagician Support";
            public static readonly string Chat = "https://gitter.im/PresetMagician/general";

            public static readonly string GetTrialLicense = $"{_protocol}{_site}/license/trial";
            public static readonly string SubmitPlugins = $"{_protocol}{_site}/plugins/submit";
            public static readonly string SubmitPluginsLive = $"https://presetmagician.com/plugins/submit";
            public static readonly string SubmitResource = $"{_protocol}{_site}/plugins/submitResource";
            public static readonly string GetOnlineResources = $"{_protocol}{_site}/plugins/getResources/";
            public static readonly string GetOnlineResource = $"{_protocol}{_site}/plugins/getResource/";

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
                    new UpdateChannel("Stable", "https://presetmagician.com/downloads/stable"),
                    new UpdateChannel("Beta", "https://presetmagician.com/downloads/beta")
                        {IsPrerelease = true},
                    new UpdateChannel("Alpha", "https://presetmagician.com/downloads/alpha")
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
            public const string QuickScanPlugins = "Plugin.QuickScanPlugins";
            public const string ScanSelectedPlugins = "Plugin.ScanSelectedPlugins";
            public const string QuickScanSelectedPlugins = "Plugin.QuickScanSelectedPlugins";
            public const string ScanSelectedPlugin = "Plugin.ScanSelectedPlugin";
            public const string AllToPresetExportList = "Plugin.AllToPresetExportList";
            public const string SelectedToPresetExportList = "Plugin.SelectedToPresetExportList";
            public const string NotExportedAllToPresetExportList = "Plugin.NotExportedAllToPresetExportList";
            public const string NotExportedSelectedToPresetExportList = "Plugin.NotExportedSelectedToPresetExportList";
            public const string ReportUnsupportedPlugins = "Plugin.ReportUnsupportedPlugins";
            public const string ForceReportPluginsToLive = "Plugin.ForceReportPluginsToLive";
            public const string ForceReportPluginsToDev = "Plugin.ForceReportPluginsToDev";
        }

        public static class PluginTools
        {
            public const string EnablePlugins = "PluginTools.EnablePlugins";
            public const string DisablePlugins = "PluginTools.DisablePlugins";
            public const string ViewSettings = "PluginTools.ViewSettings";
            public const string ViewErrors = "PluginTools.ViewErrors";
            public const string ShowPluginInfo = "PluginTools.ShowPluginInfo";
            public const string ShowPluginEditor = "PluginTools.ShowPluginEditor";
            public const string ShowPluginChunk = "PluginTools.ShowPluginChunk";
            public const string LoadPlugin = "PluginTools.LoadPlugin";
            public const string UnloadPlugin = "PluginTools.UnloadPlugin";

            public const string ReportSinglePluginToLive = "PluginTools.ReportSinglePluginToLive";
        }

        public static class Preset
        {
            public const string ActivatePresetView = "Preset.ActivatePresetView";
            public const string Export = "Preset.Export";
            public const string ClearList = "Preset.ClearList";
            public const string ClearSelected = "Preset.ClearSelected";
            public const string ApplyMidiNote = "Preset.ApplyMidiNote";
        }

        public static class PresetTools
        {
            public const string ShowPresetData = "PresetTools.ShowPresetData";
        }

        public static class Tools
        {
            public const string NksfView = "Tools.NksfView";
            public const string SettingsView = "Tools.SettingsView";
            public const string UpdateLicense = "Tools.UpdateLicense";
            public const string CompressDatabase = "Tools.CompressDatabase";
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