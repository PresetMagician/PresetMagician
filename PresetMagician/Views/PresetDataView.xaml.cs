using System.Windows.Controls;
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

            var provider = new DynamicByteProvider(viewModel.Preset.PresetData);
            PresetDataControl.ByteProvider = provider;
        }
    }
}
