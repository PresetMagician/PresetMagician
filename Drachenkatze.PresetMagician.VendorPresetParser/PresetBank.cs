using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class PresetBank : ModelBase
    {
        public String BankName { get; set; }

        public PresetBank()
        {
            Presets = new ObservableCollection<Preset>();
            BankName = "unset";
        }

        #region Presets property

        /// <summary>
        /// Gets or sets the Presets value.
        /// </summary>
        public ObservableCollection<Preset> Presets
        {
            get { return GetValue<ObservableCollection<Preset>>(PresetsProperty); }
            set { SetValue(PresetsProperty, value); }
        }

        /// <summary>
        /// Presets property data.
        /// </summary>
        public static readonly PropertyData PresetsProperty =
            RegisterProperty("Presets", typeof(ObservableCollection<Preset>));

        #endregion
    }
}