using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CsvHelper;

namespace PresetMagician.TestVstHost
{
        partial class PluginForm : Form
    {
        public PluginForm()
        {
            InitializeComponent();
        }

        public IVstPluginContext PluginContext { get; set; }

        private void DataToForm()
        {
            FillPropertyList();
            FillProgramList();
            FillProgram();
            FillParameterList();
        }

        private void FillPropertyList()
        {
            PluginContext.AcceptPluginInfoData(false);
            PluginPropertyListVw.Items.Clear();

            // plugin product
            AddProperty("Plugin Name", PluginContext.PluginCommandStub.GetEffectName());
            AddProperty("Product", PluginContext.PluginCommandStub.GetProductString());
            AddProperty("Vendor", PluginContext.PluginCommandStub.GetVendorString());
            AddProperty("Vendor Version", PluginContext.PluginCommandStub.GetVendorVersion().ToString());
            AddProperty("Vst Support", PluginContext.PluginCommandStub.GetVstVersion().ToString());
            AddProperty("Plugin Category", PluginContext.PluginCommandStub.GetCategory().ToString());

            
            // plugin info
            AddProperty("Flags", PluginContext.PluginInfo.Flags.ToString());
            AddProperty("Plugin ID", PluginContext.PluginInfo.PluginID.ToString());
            AddProperty("Plugin Version", PluginContext.PluginInfo.PluginVersion.ToString());
            AddProperty("Audio Input Count", PluginContext.PluginInfo.AudioInputCount.ToString());
            AddProperty("Audio Output Count", PluginContext.PluginInfo.AudioOutputCount.ToString());
            AddProperty("Initial Delay", PluginContext.PluginInfo.InitialDelay.ToString());
            AddProperty("Program Count", PluginContext.PluginInfo.ProgramCount.ToString());
            AddProperty("Parameter Count", PluginContext.PluginInfo.ParameterCount.ToString());
            AddProperty("Tail Size", PluginContext.PluginCommandStub.GetTailSize().ToString());

            // can do
            AddProperty("CanDo:" + VstPluginCanDo.Bypass,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.MidiProgramNames,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.Offline,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.ReceiveVstEvents,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.ReceiveVstMidiEvent,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.ReceiveVstTimeInfo,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.SendVstEvents,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.SendVstMidiEvent,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent))
                    .ToString());

            AddProperty("CanDo:" + VstPluginCanDo.ConformsToWindowRules,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.Metapass,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.MixDryWet,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.Multipass,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.NoRealTime,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.PlugAsChannelInsert,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.PlugAsSend,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.SendVstTimeInfo,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo))
                    .ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x1in1out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x1in2out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x2in1out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x2in2out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x2in4out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x4in2out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x4in4out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x4in8out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x8in4out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out)).ToString());
            AddProperty("CanDo:" + VstPluginCanDo.x8in8out,
                PluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out)).ToString());
        }

        private void AddProperty(string propName, string propValue)
        {
            ListViewItem lvItem = new ListViewItem(propName);
            lvItem.SubItems.Add(propValue);

            PluginPropertyListVw.Items.Add(lvItem);
        }

        private void FillProgramList()
        {
            ProgramListCmb.Items.Clear();

            for (int index = 0; index < PluginContext.PluginInfo.ProgramCount; index++)
            {
                ProgramListCmb.Items.Add(
                    PluginContext.PluginCommandStub.GetProgramNameIndexed(index));
            }
        }

        private void FillProgram()
        {
            ProgramIndexNud.Value = PluginContext.PluginCommandStub.GetProgram();
            ProgramListCmb.Text = PluginContext.PluginCommandStub.GetProgramName();
        }

        private void FillParameterList(bool replaceExisting = false)
        {
            if (!replaceExisting)
            {
                PluginParameterListVw.Items.Clear();
            }
            

            for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
            {
                string name = PluginContext.PluginCommandStub.GetParameterName(i);
                string label = PluginContext.PluginCommandStub.GetParameterLabel(i);
                string display = PluginContext.PluginCommandStub.GetParameterDisplay(i);
                string floatVal = PluginContext.PluginCommandStub.GetParameter(i).ToString();
                var x = PluginContext.PluginCommandStub.GetParameterProperties(i);

                var flags = "unknown";
                var displayIndex = "unknown";
                var minInteger = "unknown";
                var maxInteger = "unknown";
                if (x != null)
                {
                    flags = x.Flags.ToString();
                    displayIndex = x.DisplayIndex.ToString();
                    minInteger = x.MinInteger.ToString();
                    maxInteger = x.MaxInteger.ToString();
                }

                AddParameter(i, name, display, label, floatVal, flags ,
                    displayIndex , minInteger, maxInteger, replaceExisting);
            }
        }

        private void AddParameter(int index, string paramName, string paramValue, string label, string floatval,
            string flags, string displayIndex, string minInt, string maxInt, bool replaceExisting = false)
        {
            

            if (replaceExisting)
            {
                PluginParameterListVw.Items[(int) index].SubItems[2].Text = paramValue;
                PluginParameterListVw.Items[(int) index].SubItems[3].Text = label;
                PluginParameterListVw.Items[(int) index].SubItems[4].Text = floatval;
            }
            else
            {
                ListViewItem lvItem = new ListViewItem(index.ToString());
                lvItem.SubItems.Add(paramName);
                lvItem.SubItems.Add(paramValue);
                lvItem.SubItems.Add(label);
                lvItem.SubItems.Add(floatval);
                lvItem.SubItems.Add(flags);
                lvItem.SubItems.Add(displayIndex);
                lvItem.SubItems.Add(minInt);
                lvItem.SubItems.Add(maxInt);
                PluginParameterListVw.Items.Add(lvItem);
            }
        }

        private void PluginForm_Load(object sender, EventArgs e)
        {
            if (PluginContext == null)
            {
                Close();
            }
            else
            {
                DataToForm();
            }
        }

        private void ProgramIndexNud_ValueChanged(object sender, EventArgs e)
        {
            if (ProgramIndexNud.Value < PluginContext.PluginInfo.ProgramCount &&
                ProgramIndexNud.Value >= 0)
            {
                PluginContext.PluginCommandStub.SetProgram((int) ProgramIndexNud.Value);

                FillProgram();
                FillParameterList();
            }
        }

        private void GenerateNoiseBtn_Click(object sender, EventArgs e)
        {
            // plugin does not support processing audio
            if ((PluginContext.PluginInfo.Flags & VstPluginFlags.CanReplacing) == 0)
            {
                MessageBox.Show(this, "This plugin does not process any audio.", this.Text, MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            int inputCount = PluginContext.PluginInfo.AudioInputCount;
            int outputCount = PluginContext.PluginInfo.AudioOutputCount;
            int blockSize = 1024;

            // wrap these in using statements to automatically call Dispose and cleanup the unmanaged memory.
            using (VstAudioBufferManager inputMgr = new VstAudioBufferManager(inputCount, blockSize))
            {
                using (VstAudioBufferManager outputMgr = new VstAudioBufferManager(outputCount, blockSize))
                {
                    foreach (VstAudioBuffer buffer in inputMgr)
                    {
                        Random rnd = new Random((int) DateTime.Now.Ticks);

                        for (int i = 0; i < blockSize; i++)
                        {
                            // generate a value between -1.0 and 1.0
                            buffer[i] = (float) ((rnd.NextDouble() * 2.0) - 1.0);
                        }
                    }

                    PluginContext.PluginCommandStub.SetBlockSize(blockSize);
                    PluginContext.PluginCommandStub.SetSampleRate(44100f);

                    VstAudioBuffer[] inputBuffers = inputMgr.ToArray();
                    VstAudioBuffer[] outputBuffers = outputMgr.ToArray();

                    PluginContext.PluginCommandStub.MainsChanged(true);
                    PluginContext.PluginCommandStub.StartProcess();
                    PluginContext.PluginCommandStub.ProcessReplacing(inputBuffers, outputBuffers);
                    PluginContext.PluginCommandStub.StopProcess();
                    PluginContext.PluginCommandStub.MainsChanged(false);

                    for (int i = 0; i < inputBuffers.Length && i < outputBuffers.Length; i++)
                    {
                        for (int j = 0; j < blockSize; j++)
                        {
                            if (inputBuffers[i][j] != outputBuffers[i][j])
                            {
                                if (outputBuffers[i][j] != 0.0)
                                {
                                    MessageBox.Show(this, "The plugin has processed the audio.", this.Text,
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }
                            }
                        }
                    }

                    MessageBox.Show(this, "The plugin has passed the audio unchanged to its outputs.", this.Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void EditorBtn_Click(object sender, EventArgs e)
        {
            EditorFrame dlg = new EditorFrame();
            dlg.PluginCommandStub = PluginContext.PluginCommandStub;

            PluginContext.PluginCommandStub.MainsChanged(true);
            dlg.Show(this);
            PluginContext.PluginCommandStub.MainsChanged(false);
        }

        private void PluginParameterListVw_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            FillParameterList(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            

            saveChunkDialog.DefaultExt = ".bin";
            if (saveChunkDialog.ShowDialog(this) == DialogResult.OK)
            {
                var chunk = PluginContext.PluginCommandStub.GetChunk(false);
                File.WriteAllBytes(saveChunkDialog.FileName, chunk);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (loadChunkDialog.ShowDialog(this) == DialogResult.OK)
            {
                var chunk = File.ReadAllBytes(loadChunkDialog.FileName);
                PluginContext.PluginCommandStub.SetChunk(chunk, false);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var hexEditor = @"C:\Users\Drachenkatze\AppData\Local\HHD Software\Hex Editor Neo\HexFrame.exe";

            if (!File.Exists(hexEditor))
            {
                hexEditor = @"C:\Program Files\HHD Software\Hex Editor Neo\HexFrame.exe";
            }


            var file = Path.GetTempFileName();
            var chunk = PluginContext.PluginCommandStub.GetChunk(false);

            if (chunk == null)
            {
                chunk = PluginContext.PluginCommandStub.GetChunk(true);

                if (chunk == null)
                {
                    MessageBox.Show("Error: bank and preset chunks are null");
                    return;
                }
                else
                {
                    MessageBox.Show("Warning: bank chunk is null, preset chunk is present. Continuing with preset chunk.");
                }
            }
            File.WriteAllBytes(file, chunk);

            var processStartInfo = new ProcessStartInfo(hexEditor)
            {
                Arguments = "/s "+"\"" + Regex.Replace(file, @"(\\+)$", @"$1$1") + "\"",
                UseShellExecute = true
            };


            var proc = new Process();

            proc.StartInfo = processStartInfo;
            proc.Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var prompt = new PromptForm("Enter patch name");
            var result = prompt.ShowDialog();

            if (result == DialogResult.OK)
            {
                var patchName = prompt.Prompt.Text;
                var synthName = PluginContext.PluginCommandStub.GetEffectName();
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"PresetMagician\TestVstHost\Patches\",
                    synthName);

                Directory.CreateDirectory(directory);

                var patchFile = Path.Combine(directory, patchName + ".bin");
                var ccFile = Path.Combine(directory, patchName + ".csv");
                File.WriteAllBytes(patchFile, PluginContext.PluginCommandStub.GetChunk(false));

                var parameters = new List<VstParameterCsv>();

                for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
                {
                    parameters.Add(new VstParameterCsv
                    {
                        Index = i,
                        DisplayValue = PluginContext.PluginCommandStub.GetParameterDisplay(i),
                        FloatValue = PluginContext.PluginCommandStub.GetParameter(i),
                        Name = PluginContext.PluginCommandStub.GetParameterName(i),
                        Label = PluginContext.PluginCommandStub.GetParameterLabel(i)
                    });
                 
                   
                   

                }

                using (var writer = new StreamWriter(ccFile))
                using (var csv = new CsvWriter(writer))
                {    
                    csv.WriteRecords(parameters);
                }

            }



        }

        private void ParamsMax_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
            {
                PluginContext.PluginCommandStub.SetParameter(i, 1);
            }

            timer1.Enabled = true;
        }

        private void ParamsMin_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
            {
                PluginContext.PluginCommandStub.SetParameter(i, 0);
            }
            timer1.Enabled = true;
        }

        private void ParamsRamp_Click(object sender, EventArgs e)
        {
            var currentRamp = 0;
            var maxRamp = 5;

            for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
            {

                PluginContext.PluginCommandStub.SetParameter(i, (float)1/maxRamp*currentRamp);
                currentRamp++;

                if (currentRamp > maxRamp)
                {
                    currentRamp = 0;
                }
            }

            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            FillParameterList(true);
        }

        private void SetParamButton_Click(object sender, EventArgs e)
        {
            if (PluginParameterListVw.SelectedItems.Count == 1)
            {
                var item = PluginParameterListVw.SelectedItems[0];

                var idx = Int32.Parse(item.Text);
                var name = PluginContext.PluginCommandStub.GetParameterName(idx);
                var prompt = new PromptForm($"New value for #{idx} {name}");
                var result = prompt.ShowDialog();

                if (result == DialogResult.OK)
                {
                    var val = float.Parse(prompt.Prompt.Text);
                    if (val < 0 || val > 1)
                    {
                        MessageBox.Show($"Value {val} out of range. allowed 0-1");
                        return;
                    }
                    PluginContext.PluginCommandStub.SetParameter(idx, val);
                    timer1.Enabled = true;
                }

            }
        }

        private void ParamsRndButton_Click(object sender, EventArgs e)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < PluginContext.PluginInfo.ParameterCount; i++)
            {
                
                PluginContext.PluginCommandStub.SetParameter(i, (float)rnd.NextDouble());
               
            }
            timer1.Enabled = true;
        }
    }
}
