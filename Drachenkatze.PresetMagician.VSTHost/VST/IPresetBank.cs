using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public interface IPresetBank
    {
        string BankName { get; set; }
        IPresetBank ParentBank { get; set; }
        string BankPath { get; }

        /// <summary>
        /// Gets or sets the Presets value.
        /// </summary>

        ObservableCollection<IPresetBank> PresetBanks { get; set; }

        List<string> GetBankPath();
    }
}
