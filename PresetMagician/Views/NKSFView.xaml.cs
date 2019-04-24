using System.ComponentModel;
using Be.Windows.Forms;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    /// <summary>
    ///     Interaction logic for NKSFViewer.xaml
    /// </summary>
    public partial class NKSFView
    {
        /// <summary>
        /// </summary>
        public NKSFView() : this(null)
        {
        }

        public NKSFView(NKSFViewModel viewModel)
            : base(viewModel, DataWindowMode.Close)
        {
            AddCustomButton(new DataWindowButton("Open NKS File", "OpenNKSFile"));
            AddCustomButton(new DataWindowButton("Open Chunk with Hex Editor", "OpenWithHexEditor"));

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PluginChunk")
            {
                var s = (NKSFViewModel) sender;

                var provider = new DynamicByteProvider(s.PluginChunk.ToArray());
                PluginChunkControl.ByteProvider = provider;
            }
        }
    }
}