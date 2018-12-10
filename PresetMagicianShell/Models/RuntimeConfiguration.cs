using System;
using System.Collections.ObjectModel;
using Catel.Data;
using PresetMagicianShell.Models.Settings;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagicianShell.Models
{
    [Serializable]
    public class RuntimeConfiguration: ModelBase
    {
        public ObservableCollection<VstDirectory> VstDirectories
        {
            get => GetValue<ObservableCollection<VstDirectory>>(VstDirectoriesProperty);
            set => SetValue(VstDirectoriesProperty, value);
        }

        public static readonly PropertyData VstDirectoriesProperty = RegisterProperty("VstDirectories", typeof(ObservableCollection<VstDirectory>), () => new ObservableCollection<VstDirectory>());


        #region Plugins property

        /// <summary>
        /// Gets or sets the Plugins value.
        /// </summary>
        public ObservableCollection<Plugin> Plugins
        {
            get { return GetValue<ObservableCollection<Plugin>>(PluginsProperty); }
            set { SetValue(PluginsProperty, value); }
        }

        /// <summary>
        /// Plugins property data.
        /// </summary>
        public static readonly PropertyData PluginsProperty = RegisterProperty("Plugins", typeof(ObservableCollection<Plugin>), () => new ObservableCollection<Plugin>());

        #endregion
    }
}