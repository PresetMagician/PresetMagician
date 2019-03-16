using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using MethodTimer;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.ViewModels
{
    public class TypesCharacteristicsViewModel : ViewModelBase
    {
        private readonly DataPersisterService _dataPersisterService;
        private readonly IUIVisualizerService _visualizerService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly GlobalService _globalService;
        private readonly TypesService _typesService;
        private readonly CharacteristicsService _characteristicsService;
        private readonly IAdvancedMessageService _advancedMessageService;

        public TypesCharacteristicsViewModel(DataPersisterService dataPersisterService, GlobalService globalService,
            IUIVisualizerService visualizerService, IViewModelFactory viewModelFactory,
            CharacteristicsService characteristicsService, TypesService typesService, IAdvancedMessageService advancedMessageService)
        {
            _dataPersisterService = dataPersisterService;
            _visualizerService = visualizerService;
            _viewModelFactory = viewModelFactory;
            _globalService = globalService;
            _characteristicsService = characteristicsService;
            _typesService = typesService;
            _advancedMessageService = advancedMessageService;

            Title = "Edit Types / Characteristics";

            AddTypeCommand = new TaskCommand(OnAddTypeCommandExecuteAsync);
            EditTypeCommand = new TaskCommand(OnEditTypeCommandExecuteAsync, EditTypeCanExecute);
            DeleteTypeCommand = new TaskCommand(OnDeleteTypeCommandExecuteAsync, DeleteTypeCanExecute);
            ShowTypeUsageCommand = new TaskCommand(OnShowTypeUsageCommandExecuteAsync, ShowTypeUsageCanExecute);

            AddCharacteristicCommand = new TaskCommand(OnAddCharacteristicCommandExecuteAsync);
            EditCharacteristicCommand =
                new TaskCommand(OnEditCharacteristicCommandExecuteAsync, EditCharacteristicCanExecute);
            DeleteCharacteristicCommand =
                new TaskCommand(OnDeleteCharacteristicCommandExecuteAsync, DeleteCharacteristicCanExecute);
            ShowCharacteristicUsageCommand = new TaskCommand(OnShowCharacteristicUsageCommandExecuteAsync, ShowCharacteristicUsageCanExecute);

            SelectedTypes.CollectionChanged += SelectedTypesOnCollectionChanged;
            SelectedCharacteristics.CollectionChanged += SelectedCharacteristicsOnCollectionChanged;

            Types = _typesService.TypeUsages;
            Characteristics = _characteristicsService.CharacteristicUsages;

            _characteristicsService.UpdateCharacteristicsUsages();
            _typesService.UpdateTypesUsages();
            
            TypesView = (ListCollectionView) CollectionViewSource.GetDefaultView(_typesService.TypeUsages);
            TypesView.IsLiveSorting = false;
            TypesView.IsLiveFiltering = false;

            CharacteristicsView =
                (ListCollectionView) CollectionViewSource.GetDefaultView(
                    _characteristicsService.CharacteristicUsages);
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
                _globalService.GlobalTypes.CancelEdit();
                _globalService.GlobalCharacteristics.CancelEdit();

                foreach (var plugin in _globalService.Plugins)
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
                _globalService.GlobalTypes.EndEdit();
                _globalService.GlobalCharacteristics.EndEdit();

                foreach (var plugin in _globalService.Plugins)
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
                _globalService.GlobalTypes.BeginEdit();
                _globalService.GlobalCharacteristics.BeginEdit();

                foreach (var plugin in _globalService.Plugins)
                {
                    plugin.BeginEdit();
                }
            }, true);

            await base.InitializeAsync();
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if ( TypesView != null && (e.PropertyName == nameof(ShowTypeRedirects) || e.PropertyName == nameof(ShowIgnoredTypes)))
            {
                TypesView.Filter = o => TypeFilter(o as TypeUsage);
            }

            if (CharacteristicsView != null && (e.PropertyName == nameof(ShowCharacteristicRedirects) ||
                                                e.PropertyName == nameof(ShowIgnoredCharacteristics)))
            {
                CharacteristicsView.Filter = o => CharacteristicFilter(o as CharacteristicUsage);
            }

            base.OnPropertyChanged(e);
        }

        private bool TypeFilter(TypeUsage type)
        {
            if (!ShowTypeRedirects && type.Type.IsRedirect)
            {
                return false;
            }

            if (!ShowIgnoredTypes && type.Type.IsIgnored)
            {
                return false;
            }

            return true;
        }

        private bool CharacteristicFilter(CharacteristicUsage characteristic)
        {
            if (!ShowCharacteristicRedirects && characteristic.Characteristic.IsRedirect)
            {
                return false;
            }

            if (!ShowIgnoredCharacteristics && characteristic.Characteristic.IsIgnored)
            {
                return false;
            }

            return true;
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
            _globalService.GlobalTypes.Add(type);
            _typesService.UpdateTypesUsages();
            SelectedType = _typesService.GetTypeUsageByType(type);
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
            var viewModel = _viewModelFactory.CreateViewModel<TypeViewModel>(SelectedType.Type);
            viewModel.Title = "Edit Type";
            await _visualizerService.ShowDialogAsync(viewModel);
        }

        private bool EditTypeCanExecute()
        {
            return SelectedType != null;
        }

        
        public TaskCommand DeleteTypeCommand { get; private set; }

        private async Task OnDeleteTypeCommandExecuteAsync()
        {
            var selectedTypes = (from c in SelectedTypes select c.Type).ToList();
            var selectedTypesWithRedirects = new List<Type>();

            foreach (var selectedType in selectedTypes)
            {
                if (_typesService.IsRedirectTarget(selectedType))
                {
                    selectedTypesWithRedirects.Add(selectedType);
                }
            }

            var typesToDelete = selectedTypes.Except(selectedTypesWithRedirects).ToList();

            foreach (var plugin in _globalService.Plugins)
            {
                foreach (var preset in plugin.Presets)
                {
                    preset.Metadata.Types.RemoveItems(typesToDelete);
                }
            }

            _globalService.GlobalTypes.RemoveItems(typesToDelete);
            _typesService.UpdateTypesUsages();

            if (selectedTypesWithRedirects.Count > 0)
            {
                var typesNames = string.Join(Environment.NewLine,
                    from c in selectedTypesWithRedirects select c.FullTypeName);

                await _advancedMessageService.ShowErrorAsync(
                    "Unable to delete the following types because they are a redirect target:" +
                    $"{Environment.NewLine}{Environment.NewLine}{typesNames}",
                    "Unable to delete types",
                    HelpLinks.REFERENCE_TYPECHARACTERISTICEDITOR_DELETE);
            }
        }

        private bool DeleteTypeCanExecute()
        {
            return SelectedTypes.Count > 0;
        }
        
        public TaskCommand ShowTypeUsageCommand { get; private set; }

        private async Task OnShowTypeUsageCommandExecuteAsync()
        {
            var pluginNames = string.Join(Environment.NewLine,
                from c in SelectedType.Plugins select c.PluginName);

            await _advancedMessageService.ShowInformationAsync(
                $"The type {SelectedType.Type.FullTypeName} is used by the following plugins:" +
                $"{Environment.NewLine}{Environment.NewLine}{pluginNames}",
                "Show type usage");
        }
        
        private bool ShowTypeUsageCanExecute()
        {
            return SelectedType != null;
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
            var characteristic = new Characteristic();
            characteristic.CharacteristicName = "New Characteristic";
            var viewModel = _viewModelFactory.CreateViewModel<CharacteristicViewModel>(characteristic);

            viewModel.Title = "Add Characteristic";
            viewModel.SavedAsync += CharacteristicViewModelOnSavedAsync;
            await _visualizerService.ShowDialogAsync(viewModel);
        }

        private Task CharacteristicViewModelOnSavedAsync(object sender, EventArgs e)
        {
            var characteristic = ((CharacteristicViewModel) sender).Characteristic;
            _globalService.GlobalCharacteristics.Add(characteristic);
            _characteristicsService.UpdateCharacteristicsUsages();
            SelectedCharacteristic = _characteristicsService.GetCharacteristicUsageByCharacteristic(characteristic);
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
            var viewModel =
                _viewModelFactory.CreateViewModel<CharacteristicViewModel>(SelectedCharacteristic.Characteristic);
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
            var selectedCharacteristics = (from c in SelectedCharacteristics select c.Characteristic).ToList();
            var selectedCharacteristicsWithRedirects = new List<Characteristic>();

            foreach (var selectedCharacteristic in selectedCharacteristics)
            {
                if (_characteristicsService.IsRedirectTarget(selectedCharacteristic))
                {
                    selectedCharacteristicsWithRedirects.Add(selectedCharacteristic);
                }
            }

            var characteristicsToDelete = selectedCharacteristics.Except(selectedCharacteristicsWithRedirects).ToList();

            foreach (var plugin in _globalService.Plugins)
            {
                foreach (var preset in plugin.Presets)
                {
                    preset.Metadata.Characteristics.RemoveItems(characteristicsToDelete);
                }
            }

            _globalService.GlobalCharacteristics.RemoveItems(characteristicsToDelete);
            _characteristicsService.UpdateCharacteristicsUsages();

            if (selectedCharacteristicsWithRedirects.Count > 0)
            {
                var characteristicsNames = string.Join(Environment.NewLine,
                    from c in selectedCharacteristicsWithRedirects select c.CharacteristicName);

                await _advancedMessageService.ShowErrorAsync(
                    "Unable to delete the following characteristics because they are a redirect target:" +
                    $"{Environment.NewLine}{Environment.NewLine}{characteristicsNames}",
                    "Unable to delete characteristics",
                    HelpLinks.REFERENCE_TYPECHARACTERISTICEDITOR_DELETE);
            }
        }

        private bool DeleteCharacteristicCanExecute()
        {
            return SelectedCharacteristics.Count > 0;
        }
        
        public TaskCommand ShowCharacteristicUsageCommand { get; private set; }

        private async Task OnShowCharacteristicUsageCommandExecuteAsync()
        {
            var pluginNames = string.Join(Environment.NewLine,
                from c in SelectedCharacteristic.Plugins select c.PluginName);

            await _advancedMessageService.ShowInformationAsync(
                $"The characteristic {SelectedCharacteristic.Characteristic.CharacteristicName} is used by the following plugins:" +
                $"{Environment.NewLine}{Environment.NewLine}{pluginNames}",
                "Show characteristic usage usage");
        }
        
        private bool ShowCharacteristicUsageCanExecute()
        {
            return SelectedCharacteristic != null;
        }

        #endregion

        public bool ShowTypeRedirects { get; set; } = true;
        public bool ShowIgnoredTypes { get; set; } = true;
        public ListCollectionView TypesView { get; set; }
        public TypeUsage SelectedType { get; set; }
        public ObservableCollection<TypeUsage> SelectedTypes { get; set; } = new ObservableCollection<TypeUsage>();
        public FastObservableCollection<TypeUsage> Types { get; set; }

        public bool ShowCharacteristicRedirects { get; set; } = true;
        public bool ShowIgnoredCharacteristics { get; set; } = true;
        public ListCollectionView CharacteristicsView { get; set; }
        public CharacteristicUsage SelectedCharacteristic { get; set; }

        public ObservableCollection<CharacteristicUsage> SelectedCharacteristics { get; set; } =
            new ObservableCollection<CharacteristicUsage>();

        public FastObservableCollection<CharacteristicUsage> Characteristics { get; set; }
        public HelpLinks HelpLinks { get; } = new HelpLinks();
    }
}