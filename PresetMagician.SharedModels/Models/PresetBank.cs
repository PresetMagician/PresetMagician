using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Catel.Collections;
using Catel.Data;
using Catel.Fody;
using Catel.Runtime.Serialization;

namespace SharedModels
{
    public class PresetBank : ObservableObject
    {
        private Dictionary<string, PresetBank> _createCache = new Dictionary<string, PresetBank>();

        private string _bankName;

        [IncludeInSerialization]
        public string BankName
        {
            get => _bankName;
            set
            {
                var oldValue = _bankName;
                _bankName = value;
                RaisePropertyChanged(nameof(BankName), (object) oldValue, _bankName);
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
            var oldBankPath = BankPath;
            var oldBelowNksThreshold = IsBelowNksThreshold;
            var oldBankDepth = BankDepth;

            foreach (var presetBank in PresetBanks)
            {
                presetBank.Refresh();
            }

            UpdateBankDepth();

            RaisePropertyChanged(nameof(PresetBanks));
            RaisePropertyChanged(nameof(BankPath), (object) oldBankPath, BankPath);
           // RaisePropertyChanged(nameof(BankDepth), oldBankDepth, BankDepth);
            //RaisePropertyChanged(nameof(IsBelowNksThreshold), oldBelowNksThreshold, IsBelowNksThreshold);
        }

        public PresetBank() : this("All Banks")
        {
        }

        public PresetBank(string bankName = "All Banks")
        {
            PresetBanks = new FastObservableCollection<PresetBank>();
            PresetBanks.AutomaticallyDispatchChangeNotifications = false;

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
            if (_parentBank == null && _createCache.ContainsKey(bankPath))
            {
                return _createCache[bankPath];
            }

            var bank = CreateRecursiveInternal(bankPath);

            if (_parentBank == null && !_createCache.ContainsKey(bankPath))
            {
                _createCache.Add(bankPath, bank);
            }

            return bank;
        }

        protected PresetBank CreateRecursiveInternal(string bankPath)
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
                return foundBank.CreateRecursiveInternal(string.Join<string>("/", bankParts));
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


        [ExcludeFromBackup]
        [ExcludeFromSerialization]
        public bool IsSelected { get; set; }

        [ExcludeFromBackup]
        [ExcludeFromSerialization]
        public bool IsExpanded { get; set; }

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
            if (BankDepth > 1)
            {
                IsBelowNksThreshold = true;
            }
            else
            {
                IsBelowNksThreshold = false;
            }
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

        public bool IsBelowNksThreshold { get; private set; }
        

        #endregion
    }
}