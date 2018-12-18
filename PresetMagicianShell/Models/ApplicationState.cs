using Catel.Data;

namespace PresetMagicianShell.Models
{
    public class ApplicationState: ModelBase
    {
        private bool _isPluginsRefreshing = false;
        public bool IsPluginsRefreshing
        {
            get => _isPluginsRefreshing;
            set
            {
                if (value)
                {
                    AllowPluginScan = false;
                    AllowReportUnsupportedPlugins = false;
                    AllowModifyPresetExportList = false;
                }

                _isPluginsRefreshing = value;
            }
        }
        private bool _isPluginsScanning = false;

        public bool IsPluginsScanning
        {
            get => _isPluginsScanning;
            set
            {
                if (value)
                {
                    AllowPluginScan = false;
                    AllowReportUnsupportedPlugins = false;
                    AllowModifyPresetExportList = false;
                }

                _isPluginsScanning = value;
            }
        }

        public bool AllowPluginScan { get; private set; }
        public bool AllowReportUnsupportedPlugins { get; private set; }
        public bool AllowModifyPresetExportList { get; private set; }
    }
}