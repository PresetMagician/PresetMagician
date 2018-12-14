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
        public string BankName { get; set; }
        public PresetBank ParentBank { get; set; } = null;

        public PresetBank(string bankName = "All Banks")
        {
            PresetBanks = new ObservableCollection<PresetBank>();
            
            PresetBanks.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
                delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)                    
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        foreach (var i in e.NewItems)
                        {
                            ((PresetBank) i).ParentBank = this;
                        }
                    }
                }
            );
            BankName = bankName;
        }

        public List<string> GetBankPath ()
        {
           
                List<string> bankPaths = new List<string>();

                if (ParentBank != null)
                {
                    bankPaths.AddRange(ParentBank.GetBankPath());
                }

                bankPaths.Add(BankName);

                return bankPaths;
        }

        public string BankPath
        {
            get
            {
                return string.Join<string>(" / ", GetBankPath());
            }
        }

        #region Properties

        /// <summary>
        /// Gets or sets the Presets value.
        /// </summary>

        public ObservableCollection<PresetBank> PresetBanks { get; set; }

        #endregion
    }
}