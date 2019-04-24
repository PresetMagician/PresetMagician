using System;
using Be.Windows.Forms;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for VstPluginChunkView.xaml
    /// </summary>
    public partial class VstPluginChunkView
    {
        public VstPluginChunkView()
        {
            InitializeComponent();
        }

        public VstPluginChunkView(VstPluginChunkViewModel viewModel)
            : base(viewModel, DataWindowMode.Close)
        {
            AddCustomButton(new DataWindowButton("Load Bank Chunk", "LoadBankChunk"));
            AddCustomButton(new DataWindowButton("Save Bank Chunk", "SaveBankChunk"));
            AddCustomButton(new DataWindowButton("Refresh", "Refresh"));
            AddCustomButton(new DataWindowButton("Open Bank with Hex Editor", "OpenBankWithHexEditor"));
            AddCustomButton(new DataWindowButton("Open Preset with Hex Editor", "OpenPresetWithHexEditor"));

            InitializeComponent();

            viewModel.BankChunkChanged += ViewModelOnBankChunkChanged;
            var provider = new DynamicByteProvider(viewModel.ChunkBankMemoryStream.ToArray());
            PluginBankChunkControl.ByteProvider = provider;

            var provider2 = new DynamicByteProvider(viewModel.ChunkPresetMemoryStream.ToArray());
            PluginPresetChunkControl.ByteProvider = provider2;
        }

        private void ViewModelOnBankChunkChanged(object sender, EventArgs e)
        {
            var provider = new DynamicByteProvider(((VstPluginChunkViewModel)ViewModel).ChunkBankMemoryStream.ToArray());
            PluginBankChunkControl.ByteProvider = provider;
        }
    }
}