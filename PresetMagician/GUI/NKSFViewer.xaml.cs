using Drachenkatze.PresetMagician.NKSF.NKSF;
using Newtonsoft.Json;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfHexaEditor.Core.Bytes;

namespace Drachenkatze.PresetMagician.GUI.GUI
{
    /// <summary>
    /// Interaction logic for NKSFViewer.xaml
    /// </summary>
    public partial class NKSFViewer
    {
        public NKSFViewer()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

                openFileDialog.InitialDirectory = downloadsPath;
                openFileDialog.Filter = "NKSF Files (*.nksf)|*.nksf";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fileStream = new FileStream(filePath = openFileDialog.FileName, FileMode.Open))
                    {
                        NKSFRiff n = new NKSFRiff();
                        n.Read(fileStream);

                        SummaryInformation.Text = FormatJson(n.kontaktSound.summaryInformation.getJSON());
                        PluginId.Text = FormatJson(n.kontaktSound.pluginId.getJSON());
                        ControllerAssignments.Text = FormatJson(n.kontaktSound.controllerAssignments.getJSON());

                        var ms = new MemoryStream();

                        ms.Write(n.kontaktSound.pluginChunk.Chunk, 0, n.kontaktSound.pluginChunk.Chunk.Length);

                        PluginChunk.Stream = ms;
                    }
                }
            }
        }

        private void SavePresetChunk_Click(object sender, RoutedEventArgs e)
        {
            if (PluginChunk.Stream == null)
            {
                System.Windows.MessageBox.Show("No data loaded");
                return;
            }
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

                saveFileDialog.InitialDirectory = downloadsPath;
                saveFileDialog.Filter = "Binary Files (*.bin)|*.bin";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (var fileStream = new FileStream(filePath = saveFileDialog.FileName, FileMode.Create))
                    {
                        PluginChunk.Stream.Seek(0, SeekOrigin.Begin);
                        PluginChunk.Stream.CopyTo(fileStream);
                    }
                }
            }
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}