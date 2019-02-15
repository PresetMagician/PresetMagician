using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using Catel.Fody;
using Catel.Runtime.Serialization;

namespace SharedModels
{
    public class PresetBank : ModelBase
    {
        private string _bankName;

        public string BankName
        {
            get => _bankName;
            set
            {
                _bankName = value;
                RaisePropertyChanged(nameof(BankName));
                Refresh();
            }
        }

        private PresetBank _parentBank;

        public PresetBank ParentBank
        {
            get { return _parentBank; }
            set
            {
                _parentBank = value;
                Refresh();
            }
        }

        public void Refresh()
        {
            foreach (var presetBank in PresetBanks)
            {
                presetBank.Refresh();
            }

            UpdateBankDepth();

            RaisePropertyChanged(nameof(PresetBanks));
            RaisePropertyChanged(nameof(BankPath));
            RaisePropertyChanged(nameof(BankDepth));
            RaisePropertyChanged(nameof(IsBelowNksThreshold));
            

        }

        public PresetBank(string bankName = "All Banks")
        {
            PresetBanks = new FastObservableCollection<PresetBank>();

            PresetBanks.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
            {
                GetRootBank().SetDirty(nameof(PresetBanks));
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

        public PresetBank GetRootBank()
        {
            if (_parentBank == null)
            {
                return this;
            }

            return _parentBank.GetRootBank();
        }

        public bool IsEqualOrBelow(PresetBank bank)
        {
            if (bank == this || IsChildOf(bank))
            {
                return true;
            }

            return false;
        }

        public bool IsChildOf(PresetBank bank)
        {
            if (this == bank)
            {
                return true;
            }

            foreach (var childBank in PresetBanks)
            {
                if (childBank.IsChildOf(bank))
                {
                    return true;
                }
            }

            return false;
        }

        public PresetBank First()
        {
            return PresetBanks.First();
        }

        public List<string> GetBankPath()
        {
            List<string> bankPaths = new List<string>();

            if (ParentBank != null && ParentBank.ParentBank != null)
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
                return string.Join<string>("/", bankPath);
            }
        }

        public PresetBank CreateRecursive(string bankPath)
        {
            var bankParts = bankPath.Split('/').ToList();
            PresetBank foundBank = null;

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
                return foundBank.CreateRecursive(string.Join<string>("/", bankParts));
            }

            return foundBank;
        }

        public PresetBank FindBankPath(string bankPath)
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

        public FastObservableCollection<PresetBank> PresetBanks { get; set; }

        /// <summary>
        /// a virtual bank is a bank which is not user modifiable and is not respected during export.
        /// Usually, we have one root bank which is hidden to the user and a "All Presets" bank to indicate
        /// all visible banks.
        /// </summary>
        public bool IsVirtualBank { get; set; }


        [ExcludeFromBackup] [ExcludeFromSerialization] public bool IsSelected { get; set; }
        [ExcludeFromBackup] [ExcludeFromSerialization] public bool IsExpanded { get; set; }

        public bool ContainsBankName(string bankName)
        {
            foreach (var bank in PresetBanks)
            {
                if (bank.BankName == bankName)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateBankDepth()
        {
            BankDepth = GetBankDepth();
        }

        private int GetBankDepth()
        {
           
                    var bankDepth = 0;
                    if (_parentBank != null && !_parentBank.IsVirtualBank)
                    {
                        return _parentBank.GetBankDepth() + 1;
                    }

                    return bankDepth;
                
            
        }

        public int BankDepth { get; private set; }

        public bool IsBelowNksThreshold
        {
            get { return BankDepth > 1; }
        }

        #endregion
    }
}