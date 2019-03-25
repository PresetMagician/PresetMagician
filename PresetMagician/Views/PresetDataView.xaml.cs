using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Be.Windows.Forms;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class PresetDataView
    {
        public PresetDataView()
        {
            InitializeComponent();
        }

        public PresetDataView(PresetDataViewModel viewModel)
            : base(viewModel, DataWindowMode.Close)
        {
            AddCustomButton(new DataWindowButton("Open with HxD", "OpenWithHxD"));

            InitializeComponent();

           
            viewModel.InitializedAsync += ViewModelOnInitializedAsync;
        }

        private Task ViewModelOnInitializedAsync(object sender, EventArgs e)
        {
            var vm = ViewModel as PresetDataViewModel;
            if (vm.PresetData != null)
            {
                var provider = new DynamicByteProvider(vm.PresetData);
                PresetDataControl.ByteProvider = provider;
            }

            return Task.CompletedTask;
        }
    }
}