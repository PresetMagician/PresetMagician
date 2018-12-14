using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.Fody;
using Catel.MVVM;

namespace PresetMagicianShell.ViewModels
{
    public class VstPluginViewModel : ViewModelBase
    {

        public VstPluginViewModel(Models.Plugin plugin)
        {
            Plugin = plugin;
            DoSomething = new Command(OnDoSomethingExecute);
        }

        public Command DoSomething  { get; set; }

        private void OnDoSomethingExecute ()
        {
            Debug.WriteLine("Direct: "+Plugin.NumPresets);
        }

        [Model]
        [Expose("RootBank")]
        [Expose("Presets")]
        [Expose("IsScanned")]
        public Models.Plugin Plugin
        {
            get { return GetValue<Models.Plugin>(PluginProperty); }
            private set { SetValue(PluginProperty, value); }
        }

        /// <summary>
        /// Register the Room property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PluginProperty = RegisterProperty("Plugin", typeof(Models.Plugin));

        /// <summary>
        /// Gets or sets the table collection.
        /// </summary>
        [ViewModelToModel("Plugin")]
        public ObservableCollection<Models.PluginInfoItem> PluginInfoItems
        {
            get { return GetValue<ObservableCollection<Models.PluginInfoItem>>(PluginInfoItemsProperty); }
            set { SetValue(PluginInfoItemsProperty, value); }
        }

        /// <summary>
        /// Register the Tables property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PluginInfoItemsProperty = RegisterProperty("PluginInfoItems", typeof(ObservableCollection<Models.PluginInfoItem>));

    }

}