using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    

    public class PresetBank : ModelBase, IPresetBank
    {
        public string BankName { get; set; }
        public IPresetBank ParentBank { get; set; }

        public PresetBank(string bankName = "All Banks")
        {
            PresetBanks = new ObservableCollection<IPresetBank>();
            
            PresetBanks.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)                    
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var i in e.NewItems)
                    {
                        ((PresetBank) i).ParentBank = this;
                    }
                }
            };
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
                var bankPath = GetBankPath();
                bankPath.RemoveAt(0);
                return string.Join<string>(" / ", bankPath);
            }
        }

        public IPresetBank CreateRecursive(string bankPath)
        {
            var bankParts = bankPath.Split('/').ToList();
            IPresetBank foundBank = null;
            
            foreach (var presetBank in PresetBanks)
            {
                if (presetBank.BankName == bankParts.First())
                {
                    foundBank = presetBank;
                    break;
                }
            }

            if (foundBank == null)
            {
                foundBank = new PresetBank(bankParts.First());
                PresetBanks.Add(foundBank);
            }
            
            bankParts.RemoveAt(0);

            if (bankParts.Count > 0)
            {
                foundBank.CreateRecursive(string.Join<string>(" / ", bankParts));    
            }

            return foundBank;
        }

        public IPresetBank FindBankPath(string bankPath)
        {
            if (BankPath == bankPath)
            {
                return this;
            }

            foreach (var presetBank in PresetBanks)
            {
                var result = presetBank.FindBankPath(bankPath);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the Presets value.
        /// </summary>

        public ObservableCollection<IPresetBank> PresetBanks { get; set; }

        #endregion
    }
}