namespace PresetMagician.TestVstHost
{
     partial class PluginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PluginPropertyListVw = new System.Windows.Forms.ListView();
            this.PropertyNameHdr = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.PropertyValueHdr = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ProgramListCmb = new System.Windows.Forms.ComboBox();
            this.PluginParameterListVw = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.ParameterNameHdr = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.ParameterValueHdr = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.ParameterLabelHdr = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.ParameterFloatValueHdr =
                ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
            this.ProgramIndexNud = new System.Windows.Forms.NumericUpDown();
            this.OKBtn = new System.Windows.Forms.Button();
            this.GenerateNoiseBtn = new System.Windows.Forms.Button();
            this.EditorBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.saveChunkDialog = new System.Windows.Forms.SaveFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.loadChunkDialog = new System.Windows.Forms.OpenFileDialog();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.ParamsMax = new System.Windows.Forms.Button();
            this.ParamsMin = new System.Windows.Forms.Button();
            this.ParamsRamp = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SetParamButton = new System.Windows.Forms.Button();
            this.ParamsRndBtn = new System.Windows.Forms.Button();
            this.speakerConfigBtn = new System.Windows.Forms.Button();
            this.wasapiDevicesCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.startStop = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.midiInputDropdown = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.ProgramIndexNud)).BeginInit();
            this.SuspendLayout();
            this.groupBox1.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.PluginPropertyListVw);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(1290, 183);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Plugin Properties";
            this.PluginPropertyListVw.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.PluginPropertyListVw.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                {this.PropertyNameHdr, this.PropertyValueHdr});
            this.PluginPropertyListVw.FullRowSelect = true;
            this.PluginPropertyListVw.HideSelection = false;
            this.PluginPropertyListVw.Location = new System.Drawing.Point(5, 18);
            this.PluginPropertyListVw.MultiSelect = false;
            this.PluginPropertyListVw.Name = "PluginPropertyListVw";
            this.PluginPropertyListVw.Size = new System.Drawing.Size(1247, 153);
            this.PluginPropertyListVw.TabIndex = 0;
            this.PluginPropertyListVw.UseCompatibleStateImageBehavior = false;
            this.PluginPropertyListVw.View = System.Windows.Forms.View.Details;
            this.PropertyNameHdr.Text = "Property Name";
            this.PropertyNameHdr.Width = 180;
            this.PropertyValueHdr.Text = "Property Value";
            this.PropertyValueHdr.Width = 180;
            this.groupBox2.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top |
                                                         System.Windows.Forms.AnchorStyles.Bottom) |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.ProgramListCmb);
            this.groupBox2.Controls.Add(this.PluginParameterListVw);
            this.groupBox2.Controls.Add(this.ProgramIndexNud);
            this.groupBox2.Location = new System.Drawing.Point(12, 202);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1290, 492);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Programs && Parameters";
            this.ProgramListCmb.FormattingEnabled = true;
            this.ProgramListCmb.Location = new System.Drawing.Point(54, 19);
            this.ProgramListCmb.Name = "ProgramListCmb";
            this.ProgramListCmb.Size = new System.Drawing.Size(335, 21);
            this.ProgramListCmb.TabIndex = 3;
            this.PluginParameterListVw.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top |
                                                         System.Windows.Forms.AnchorStyles.Bottom) |
                                                        System.Windows.Forms.AnchorStyles.Left) |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.PluginParameterListVw.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.columnHeader1, this.ParameterNameHdr, this.ParameterValueHdr, this.ParameterLabelHdr,
                this.ParameterFloatValueHdr, this.columnHeader2, this.columnHeader3, this.columnHeader4
            });
            this.PluginParameterListVw.FullRowSelect = true;
            this.PluginParameterListVw.HideSelection = false;
            this.PluginParameterListVw.Location = new System.Drawing.Point(0, 48);
            this.PluginParameterListVw.MultiSelect = false;
            this.PluginParameterListVw.Name = "PluginParameterListVw";
            this.PluginParameterListVw.Size = new System.Drawing.Size(1253, 439);
            this.PluginParameterListVw.TabIndex = 2;
            this.PluginParameterListVw.UseCompatibleStateImageBehavior = false;
            this.PluginParameterListVw.View = System.Windows.Forms.View.Details;
            this.PluginParameterListVw.SelectedIndexChanged +=
                new System.EventHandler(this.PluginParameterListVw_SelectedIndexChanged);
            this.columnHeader1.Text = "Index";
            this.ParameterNameHdr.Text = "Parameter Name";
            this.ParameterNameHdr.Width = 120;
            this.ParameterValueHdr.Text = "Value";
            this.ParameterValueHdr.Width = 50;
            this.ParameterLabelHdr.Text = "Label";
            this.ParameterLabelHdr.Width = 80;
            this.ParameterFloatValueHdr.Text = "FloatVal";
            this.columnHeader2.Text = "DisplayIndex";
            this.columnHeader3.Text = "minInt";
            this.columnHeader4.Text = "maxInt";
            this.ProgramIndexNud.Location = new System.Drawing.Point(7, 20);
            this.ProgramIndexNud.Name = "ProgramIndexNud";
            this.ProgramIndexNud.Size = new System.Drawing.Size(41, 20);
            this.ProgramIndexNud.TabIndex = 0;
            this.ProgramIndexNud.ValueChanged += new System.EventHandler(this.ProgramIndexNud_ValueChanged);
            this.OKBtn.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(1228, 732);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 3;
            this.OKBtn.Text = "Close";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.GenerateNoiseBtn.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.GenerateNoiseBtn.Location = new System.Drawing.Point(20, 733);
            this.GenerateNoiseBtn.Name = "GenerateNoiseBtn";
            this.GenerateNoiseBtn.Size = new System.Drawing.Size(84, 23);
            this.GenerateNoiseBtn.TabIndex = 4;
            this.GenerateNoiseBtn.Text = "Process Noise";
            this.GenerateNoiseBtn.UseVisualStyleBackColor = true;
            this.GenerateNoiseBtn.Click += new System.EventHandler(this.GenerateNoiseBtn_Click);
            this.EditorBtn.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.EditorBtn.Location = new System.Drawing.Point(110, 733);
            this.EditorBtn.Name = "EditorBtn";
            this.EditorBtn.Size = new System.Drawing.Size(75, 23);
            this.EditorBtn.TabIndex = 5;
            this.EditorBtn.Text = "Editor...";
            this.EditorBtn.UseVisualStyleBackColor = true;
            this.EditorBtn.Click += new System.EventHandler(this.EditorBtn_Click);
            this.button1.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(191, 733);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Refresh_Click);
            this.button2.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Location = new System.Drawing.Point(19, 700);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Save Chunk";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.button3.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.button3.Location = new System.Drawing.Point(110, 700);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "Load Chunk";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            this.button4.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(191, 700);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "Hex Open Chunk";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            this.button5.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.button5.Location = new System.Drawing.Point(274, 700);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(127, 23);
            this.button5.TabIndex = 10;
            this.button5.Text = "Save Chunk+CSV";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            this.ParamsMax.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.ParamsMax.Location = new System.Drawing.Point(656, 700);
            this.ParamsMax.Name = "ParamsMax";
            this.ParamsMax.Size = new System.Drawing.Size(75, 23);
            this.ParamsMax.TabIndex = 11;
            this.ParamsMax.Text = "Params Max";
            this.ParamsMax.UseVisualStyleBackColor = true;
            this.ParamsMax.Click += new System.EventHandler(this.ParamsMax_Click);
            this.ParamsMin.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.ParamsMin.Location = new System.Drawing.Point(737, 700);
            this.ParamsMin.Name = "ParamsMin";
            this.ParamsMin.Size = new System.Drawing.Size(75, 23);
            this.ParamsMin.TabIndex = 12;
            this.ParamsMin.Text = "Params Min";
            this.ParamsMin.UseVisualStyleBackColor = true;
            this.ParamsMin.Click += new System.EventHandler(this.ParamsMin_Click);
            this.ParamsRamp.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.ParamsRamp.Location = new System.Drawing.Point(818, 700);
            this.ParamsRamp.Name = "ParamsRamp";
            this.ParamsRamp.Size = new System.Drawing.Size(93, 23);
            this.ParamsRamp.TabIndex = 13;
            this.ParamsRamp.Text = "Params Ramp";
            this.ParamsRamp.UseVisualStyleBackColor = true;
            this.ParamsRamp.Click += new System.EventHandler(this.ParamsRamp_Click);
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            this.SetParamButton.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.SetParamButton.Location = new System.Drawing.Point(1016, 700);
            this.SetParamButton.Name = "SetParamButton";
            this.SetParamButton.Size = new System.Drawing.Size(93, 23);
            this.SetParamButton.TabIndex = 14;
            this.SetParamButton.Text = "Set Param";
            this.SetParamButton.UseVisualStyleBackColor = true;
            this.SetParamButton.Click += new System.EventHandler(this.SetParamButton_Click);
            this.ParamsRndBtn.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.ParamsRndBtn.Location = new System.Drawing.Point(917, 700);
            this.ParamsRndBtn.Name = "ParamsRndBtn";
            this.ParamsRndBtn.Size = new System.Drawing.Size(93, 23);
            this.ParamsRndBtn.TabIndex = 15;
            this.ParamsRndBtn.Text = "Params Rnd";
            this.ParamsRndBtn.UseVisualStyleBackColor = true;
            this.ParamsRndBtn.Click += new System.EventHandler(this.ParamsRndButton_Click);
            this.speakerConfigBtn.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.speakerConfigBtn.Location = new System.Drawing.Point(274, 732);
            this.speakerConfigBtn.Name = "speakerConfigBtn";
            this.speakerConfigBtn.Size = new System.Drawing.Size(127, 23);
            this.speakerConfigBtn.TabIndex = 16;
            this.speakerConfigBtn.Text = "Show Speaker Config";
            this.speakerConfigBtn.UseVisualStyleBackColor = true;
            this.speakerConfigBtn.Click += new System.EventHandler(this.SpeakerConfigBtn_Click);
            this.wasapiDevicesCombo.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.wasapiDevicesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wasapiDevicesCombo.FormattingEnabled = true;
            this.wasapiDevicesCombo.Location = new System.Drawing.Point(492, 733);
            this.wasapiDevicesCombo.Name = "wasapiDevicesCombo";
            this.wasapiDevicesCombo.Size = new System.Drawing.Size(239, 21);
            this.wasapiDevicesCombo.TabIndex = 17;
            this.label1.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(407, 737);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Output Device:";
            this.startStop.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.startStop.Appearance = System.Windows.Forms.Appearance.Button;
            this.startStop.AutoSize = true;
            this.startStop.Location = new System.Drawing.Point(741, 732);
            this.startStop.Name = "startStop";
            this.startStop.Size = new System.Drawing.Size(66, 23);
            this.startStop.TabIndex = 19;
            this.startStop.Text = "Start/Stop";
            this.startStop.UseVisualStyleBackColor = true;
            this.startStop.CheckedChanged += new System.EventHandler(this.StartStop_CheckedChanged);
            this.label2.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(815, 736);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Midi Input:";
            this.midiInputDropdown.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.midiInputDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.midiInputDropdown.FormattingEnabled = true;
            this.midiInputDropdown.Location = new System.Drawing.Point(877, 732);
            this.midiInputDropdown.Name = "midiInputDropdown";
            this.midiInputDropdown.Size = new System.Drawing.Size(232, 21);
            this.midiInputDropdown.TabIndex = 21;
            this.midiInputDropdown.SelectedIndexChanged +=
                new System.EventHandler(this.MidiInputDropdown_SelectedIndexChanged);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1316, 765);
            this.Controls.Add(this.midiInputDropdown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.startStop);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.wasapiDevicesCombo);
            this.Controls.Add(this.speakerConfigBtn);
            this.Controls.Add(this.ParamsRndBtn);
            this.Controls.Add(this.SetParamButton);
            this.Controls.Add(this.ParamsRamp);
            this.Controls.Add(this.ParamsMin);
            this.Controls.Add(this.ParamsMax);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.EditorBtn);
            this.Controls.Add(this.GenerateNoiseBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugin Details";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PluginForm_FormClosing);
            this.Load += new System.EventHandler(this.PluginForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.ProgramIndexNud)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView PluginPropertyListVw;
        private System.Windows.Forms.ColumnHeader PropertyNameHdr;
        private System.Windows.Forms.ColumnHeader PropertyValueHdr;
        private System.Windows.Forms.ListView PluginParameterListVw;
        private System.Windows.Forms.NumericUpDown ProgramIndexNud;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.ColumnHeader ParameterNameHdr;
        private System.Windows.Forms.ColumnHeader ParameterValueHdr;
        private System.Windows.Forms.ColumnHeader ParameterLabelHdr;
        private System.Windows.Forms.Button GenerateNoiseBtn;
        private System.Windows.Forms.Button EditorBtn;
        private System.Windows.Forms.ComboBox ProgramListCmb;
        private System.Windows.Forms.ColumnHeader ParameterFloatValueHdr;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.SaveFileDialog saveChunkDialog;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.OpenFileDialog loadChunkDialog;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ParamsMax;
        private System.Windows.Forms.Button ParamsMin;
        private System.Windows.Forms.Button ParamsRamp;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button SetParamButton;
        private System.Windows.Forms.Button ParamsRndBtn;
        private System.Windows.Forms.Button speakerConfigBtn;
        private System.Windows.Forms.ComboBox wasapiDevicesCombo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox startStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox midiInputDropdown;
    }
}