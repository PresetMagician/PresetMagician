using System.ComponentModel;
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
            AddCustomButton(new DataWindowButton("Open Bank with HxD", "OpenWithHxDBank"));
            AddCustomButton(new DataWindowButton("Open Preset with HxD", "OpenWithHxDPreset"));

            InitializeComponent();

            var provider = new DynamicByteProvider(viewModel.ChunkBankMemoryStream.ToArray());
            PluginBankChunkControl.ByteProvider = provider;

            var provider2 = new DynamicByteProvider(viewModel.ChunkPresetMemoryStream.ToArray());
            PluginPresetChunkControl.ByteProvider = provider2;
        }

       
    }
}