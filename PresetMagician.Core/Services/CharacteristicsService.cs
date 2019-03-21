using System.Collections.Generic;
using System.Linq;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public class CharacteristicsService
    {
        private readonly GlobalService _globalService;

        public CharacteristicsService(GlobalService globalService)
        {
            _globalService = globalService;
            CharacteristicUsages =
                new WrappedEditableCollection<CharacteristicUsage, Characteristic>(_globalService
                    .GlobalCharacteristics);
        }

        public void UpdateCharacteristicsUsages()
        {
            var plugins = _globalService.Plugins;

            foreach (var c in CharacteristicUsages)
            {
                c.Plugins.Clear();
                c.UsageCount = 0;
            }

            foreach (var plugin in plugins)
            {
                foreach (var preset in plugin.Presets)
                {
                    foreach (var characteristic in preset.Metadata.Characteristics)
                    {
                        var item = CharacteristicUsages.GetFromOriginal(characteristic);
                        item.UsageCount++;
                        item.Plugins.Add(plugin);
                    }
                }
            }
        }

        public List<Characteristic> GetRedirectSources(Characteristic characteristic)
        {
            return (from t in _globalService.GlobalCharacteristics
                where t.RedirectCharacteristic == characteristic
                orderby t.CharacteristicName
                select t).ToList();
        }

        public bool IsRedirectTarget(Characteristic characteristic)
        {
            return (from t in _globalService.GlobalCharacteristics
                where t.RedirectCharacteristic == characteristic
                orderby t.CharacteristicName
                select t).Any();
        }

        public List<Characteristic> GetRedirectTargets(Characteristic characteristic)
        {
            return (from t in _globalService.GlobalCharacteristics
                where !t.IsRedirect && !t.IsIgnored && t != characteristic
                orderby t.CharacteristicName
                select t).ToList();
        }

        public bool HasCharacteristic(Characteristic characteristic)
        {
            return (from t in _globalService.GlobalCharacteristics
                where t != characteristic && t.CharacteristicName == characteristic.CharacteristicName
                select t).Any();
        }


        public CharacteristicUsage GetCharacteristicUsageByCharacteristic(Characteristic characteristic)
        {
            return (from c in CharacteristicUsages where c.Characteristic == characteristic select c).SingleOrDefault();
        }

        public WrappedEditableCollection<CharacteristicUsage, Characteristic> CharacteristicUsages { get; }
    }
}