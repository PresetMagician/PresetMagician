using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using MethodTimer;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Views;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.ViewModels
{
    public class TypesCharacteristicsViewModel: ViewModelBase
    {
        private DataPersisterService _dataPersisterService;
        private IUIVisualizerService _visualizerService;
        private IViewModelFactory _viewModelFactory;
        
        public TypesCharacteristicsViewModel(DataPersisterService dataPersisterService, IUIVisualizerService visualizerService, IViewModelFactory viewModelFactory)
        {
            _dataPersisterService = dataPersisterService;
            _visualizerService = visualizerService;
            _viewModelFactory = viewModelFactory;

            Title = "Edit Types / Characteristics";
            
            AddTypeCommand = new TaskCommand(OnAddTypeCommandExecuteAsync);
            EditTypeCommand = new TaskCommand(OnEditTypeCommandExecuteAsync, EditTypeCanExecute);
            DeleteTypeCommand= new TaskCommand(OnDeleteTypeCommandExecuteAsync, DeleteTypeCanExecute);
            
            AddCharacteristicCommand = new TaskCommand(OnAddCharacteristicCommandExecuteAsync);
            EditCharacteristicCommand = new TaskCommand(OnEditCharacteristicCommandExecuteAsync, EditCharacteristicCanExecute);
            DeleteCharacteristicCommand= new TaskCommand(OnDeleteCharacteristicCommandExecuteAsync, DeleteCharacteristicCanExecute);
            
            SelectedTypes.CollectionChanged += SelectedTypesOnCollectionChanged;
            SelectedCharacteristics.CollectionChanged += SelectedCharacteristicsOnCollectionChanged;
            
            Types = Type.GlobalTypes;
            Characteristics = Characteristic.GlobalCharacteristics;
            
            TypesView = (ListCollectionView) CollectionViewSource.GetDefaultView(Types);
            TypesView.IsLiveSorting = false;
            TypesView.IsLiveFiltering = false;
            
            CharacteristicsView = (ListCollectionView) CollectionViewSource.GetDefaultView(Characteristics);
            CharacteristicsView.IsLiveSorting = false;
            CharacteristicsView.IsLiveFiltering = false;
            
        }

        private void SelectedTypesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeleteTypeCommand.RaiseCanExecuteChanged();
        }
        
        private void SelectedCharacteristicsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeleteCharacteristicCommand.RaiseCanExecuteChanged();
        }


        protected override async Task<bool> CancelAsync()
        {
            

            await TaskHelper.Run(() =>
            {
                Types.CancelEdit();
                Characteristics.CancelEdit();

                foreach (var plugin in _dataPersisterService.Plugins)
                {
                    plugin.CancelEdit();
                }
            });
            
            
            
            var result = await base.CancelAsync();

            return result;
        }

        protected override async Task<bool> SaveAsync()
        {
            await TaskHelper.Run(() =>
            {
                Types.EndEdit();
                Characteristics.EndEdit();

                foreach (var plugin in _dataPersisterService.Plugins)
                {
                    plugin.EndEdit();
                }
            });
            
            _dataPersisterService.Save();
            return await base.SaveAsync();
        }

        [Time]
        protected override async Task InitializeAsync()
        {
            await TaskHelper.Run(() =>
            {
                

                Types.BeginEdit();
                Characteristics.BeginEdit();

                foreach (var plugin in _dataPersisterService.Plugins)
                {
                    plugin.BeginEdit();
                }
            }, true);
            
            await base.InitializeAsync();
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShowTypeRedirects) && TypesView != null)
            {
                TypesView.Filter = o => TypeFilter(o as Type);
            }
            
            if (e.PropertyName == nameof(ShowCharacteristicRedirects) && CharacteristicsView != null)
            {
                CharacteristicsView.Filter = o => CharacteristicFilter(o as Characteristic);
            }

            base.OnPropertyChanged(e);
        }

        private bool TypeFilter(Type type)
        {
            if (ShowTypeRedirects)
            {
                return true;
            }
           

            return !type.IsRedirect;
           
        }
        
        private bool CharacteristicFilter(Characteristic characteristic)
        {
            if (ShowCharacteristicRedirects)
            {
                return true;
            }
           

            return !characteristic.IsRedirect;
           
        }
        
        #region Type Commands

        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand AddTypeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnAddTypeCommandExecuteAsync()
        {
            var type = new Type();
            type.TypeName = "New Type";
            var viewModel = _viewModelFactory.CreateViewModel<TypeViewModel>(type);

            viewModel.Title = "Add Type";
            viewModel.SavedAsync += ViewModelOnSavedAsync;
            await _visualizerService.ShowDialogAsync(viewModel);
        }
        
        private Task ViewModelOnSavedAsync(object sender, EventArgs e)
        {
            var type = ((TypeViewModel) sender).Type;
            Types.Add(type);
            SelectedType = type;
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand EditTypeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnEditTypeCommandExecuteAsync()
        {
            var viewModel = _viewModelFactory.CreateViewModel<TypeViewModel>(SelectedType);
            viewModel.Title = "Edit Type";
            await _visualizerService.ShowDialogAsync(viewModel);
        }
        
        private bool EditTypeCanExecute()
        {
            return SelectedType != null;
        }
        
        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand DeleteTypeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnDeleteTypeCommandExecuteAsync()
        {
            foreach (var plugin in _dataPersisterService.Plugins)
            {
                foreach (var preset in plugin.Presets)
                {
                    preset.Metadata.Types.RemoveItems(SelectedTypes);
                }
            }

            var selection = SelectedTypes.ToList();
            Types.RemoveItems(selection);
        }
        
        private bool DeleteTypeCanExecute()
        {
            return SelectedTypes.Count > 0;
        }
        #endregion
        
        #region Characteristic Commands

        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand AddCharacteristicCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnAddCharacteristicCommandExecuteAsync()
        {
            var Characteristic = new Characteristic();
            Characteristic.CharacteristicName = "New Characteristic";
            var viewModel = _viewModelFactory.CreateViewModel<CharacteristicViewModel>(Characteristic);

            viewModel.Title = "Add Characteristic";
            viewModel.SavedAsync += CharacteristicViewModelOnSavedAsync;
            await _visualizerService.ShowDialogAsync(viewModel);
        }
        
        private Task CharacteristicViewModelOnSavedAsync(object sender, EventArgs e)
        {
            var Characteristic = ((CharacteristicViewModel) sender).Characteristic;
            Characteristics.Add(Characteristic);
            SelectedCharacteristic = Characteristic;
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand EditCharacteristicCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnEditCharacteristicCommandExecuteAsync()
        {
            var viewModel = _viewModelFactory.CreateViewModel<CharacteristicViewModel>(SelectedCharacteristic);
            viewModel.Title = "Edit Characteristic";
            await _visualizerService.ShowDialogAsync(viewModel);
        }
        
        private bool EditCharacteristicCanExecute()
        {
            return SelectedCharacteristic != null;
        }
        
        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand DeleteCharacteristicCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnDeleteCharacteristicCommandExecuteAsync()
        {
            foreach (var plugin in _dataPersisterService.Plugins)
            {
                foreach (var preset in plugin.Presets)
                {
                    preset.Metadata.Characteristics.RemoveItems(SelectedCharacteristics);
                }
            }

            var selection = SelectedCharacteristics.ToList();
            Characteristics.RemoveItems(selection);
        }
        
        private bool DeleteCharacteristicCanExecute()
        {
            return SelectedCharacteristics.Count > 0;
        }
        #endregion

        public bool ShowTypeRedirects { get; set; } = true;
        public ListCollectionView TypesView { get; set; }
        public Type SelectedType { get; set; }
        public ObservableCollection<Type> SelectedTypes { get; set; }= new ObservableCollection<Type>();
        public EditableCollection<Type> Types { get; set; }

        public bool ShowCharacteristicRedirects { get; set; } = true;
        public ListCollectionView CharacteristicsView { get; set; }
        public Characteristic SelectedCharacteristic { get; set; }
        public ObservableCollection<Characteristic> SelectedCharacteristics { get; set; }= new ObservableCollection<Characteristic>();
        public EditableCollection<Characteristic> Characteristics { get; set; }
    }
}