using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ControlzEx.Standard;
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using PresetMagician.Helpers;
using SharedModels;
using IWin32Window = System.Windows.Forms.IWin32Window;
using Size = System.Drawing.Size;


namespace PresetMagician.Views
{
    /// <summary>
    /// The frame in which a custom plugin editor UI is displayed.
    /// </summary>
    public partial class PluginEditorFrame : Form
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        public PluginEditorFrame()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the Plugin Command Stub.
        /// </summary>
        public Jacobi.Vst.Core.Host.IVstPluginCommandStub PluginCommandStub { get; set; }

        /// <summary>
        /// Shows the custom plugin editor UI.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public new DialogResult ShowDialog(IWin32Window owner)
        {
            Rectangle wndRect = new Rectangle();

            this.Text = PluginCommandStub.GetEffectName();

            if (PluginCommandStub.EditorGetRect(out wndRect))
            {
                this.Size = this.SizeFromClientSize(new Size(wndRect.Width, wndRect.Height));
                PluginCommandStub.EditorOpen(this.Handle);
            }

            return base.ShowDialog(owner);
        }

        /// <summary>
        /// Shows the custom plugin editor UI.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public new DialogResult ShowDialog()
        {
            Rectangle wndRect = new Rectangle();

            this.Text = PluginCommandStub.GetEffectName();

            if (PluginCommandStub.EditorGetRect(out wndRect))
            {
                this.Size = this.SizeFromClientSize(new Size(wndRect.Width, wndRect.Height));
                PluginCommandStub.EditorOpen(this.Handle);
            }

            return base.ShowDialog();
        }

        public new void Show()
        {
            Rectangle wndRect = new Rectangle();

            this.Text = PluginCommandStub.GetEffectName();

            if (PluginCommandStub.EditorGetRect(out wndRect))
            {
                this.Size = this.SizeFromClientSize(new Size(wndRect.Width, wndRect.Height));
                PluginCommandStub.EditorOpen(this.Handle);
            }

            base.Show();
        }

       

        

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel == false)
            {
                PluginCommandStub.EditorClose();
            }
        }
    }
}