using System.Threading.Tasks;
using Catel.MVVM;
using MethodTimer;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class TypesCharacteristicsViewModel: ViewModelBase
    {
        private DataPersisterService _dataPersisterService;
        
        public TypesCharacteristicsViewModel(DataPersisterService dataPersisterService)
        {
            _dataPersisterService = dataPersisterService;
            Title = "Edit Types / Characteristics";
        }

        protected override Task<bool> CancelAsync()
        {
            Types.CancelEdit();
            Characteristics.CancelEdit();
            
            foreach (var plugin in _dataPersisterService.Plugins)
            {
                plugin.CancelEdit();
            }
            return base.CancelAsync();
        }

        protected override Task<bool> SaveAsync()
        {
            Types.EndEdit();
            Characteristics.EndEdit();
            
            foreach (var plugin in _dataPersisterService.Plugins)
            {
                plugin.EndEdit();
            }
            return base.SaveAsync();
        }

        [Time]
        protected override Task InitializeAsync()
        {
            Types = Type.GlobalTypes;
            Characteristics = Characteristic.GlobalCharacteristics;

            Types.BeginEdit();
            Characteristics.BeginEdit();
            
            foreach (var plugin in _dataPersisterService.Plugins)
            {
                plugin.BeginEdit();
            }
            
            return base.InitializeAsync();
        }

        public EditableCollection<Type> Types { get; set; }
        public EditableCollection<Characteristic> Characteristics { get; set; }
    }
}