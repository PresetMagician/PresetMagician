using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;
using Microsoft.DwayneNeed.Win32;
using Microsoft.DwayneNeed.Win32.User32;
using PresetMagician.Core.Models;
using PresetMagician.Utils.Logger;
using PresetMagician.VstHost.Util;
using Brushes = System.Windows.Media.Brushes;

namespace PresetMagician.VstHost.VST
{
    public class RemoteVstPlugin
    {
        private string _dllPath;
        private Timer _shutdownTimer;
        public bool BackgroundProcessing { get; set; }
        public MiniLogger Logger { get; set; }


        public string DllPath
        {
            get { return _dllPath; }
            set
            {
                DllFilename = Path.GetFileName(value);
                _dllPath = value;
            }
        }

        public string DllFilename { get; set; } = "";
        public VstPluginContext PluginContext { get; set; }
        public bool IsEditorOpen { get; set; }
        public bool IsLoaded { get; set; }

        private Window _editorWindow;
        private WindowInteropHelper _editorWindowHelper;

        private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);

        public bool OpenEditorHidden()
        {
            return OpenEditorInternal(true, false);
        }

        public bool OpenEditor(bool topmost)
        {
            return OpenEditorInternal(false, topmost);
        }

        private bool OpenEditorInternal(bool hidden, bool topmost)
        {
            Rectangle wndRect = new Rectangle();

            if (hidden)
            {
                _editorWindow = new PluginWindow //make sure the window is invisible
                {
                    Width = 100,
                    Height = 100,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = 0,
                    Top = 0,
                    ResizeMode = ResizeMode.NoResize,
                    Margin = new Thickness(0),
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = false,
                    SizeToContent = SizeToContent.Manual,
                    AllowsTransparency = true,
                    Opacity = 0,
                    Background = Brushes.Transparent
                };
            }
            else
            {
                _editorWindow = new PluginWindow() //make sure the window is invisible
                {
                    Width = 100,
                    Height = 100,
                    ShowInTaskbar = true,
                    ShowActivated = true,
                    Title = "Plugin Editor: " + PluginContext.PluginCommandStub.GetEffectName(),
                    ResizeMode = ResizeMode.NoResize,
                    Margin = new Thickness(0),
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = false,
                    Topmost = topmost,
                    SizeToContent = SizeToContent.WidthAndHeight,
                };
            }

            _editorWindowHelper = new WindowInteropHelper(_editorWindow);
            _editorWindowHelper.EnsureHandle();
            
            if (PluginContext.PluginCommandStub.EditorGetRect(out wndRect))
            {
                _editorWindow.Width = wndRect.Width;
                    _editorWindow.Height = wndRect.Height;
                    PluginContext.PluginCommandStub.EditorOpen(_editorWindowHelper.Handle);
            }
            else
            {
                PluginContext.PluginCommandStub.EditorOpen(_editorWindowHelper.Handle);

                if (PluginContext.PluginCommandStub.EditorGetRect(out wndRect))
                {
                    _editorWindow.Width = wndRect.Width;
                    _editorWindow.Height = wndRect.Height;
                }
                else
                {
                    Logger.Warning($"Unable to open the editor for the plugin {PluginContext.PluginCommandStub.GetEffectName()} because it didn't give us a rectangle to work with.");
                    return false;
                }

            }

            if (wndRect.Width == 0 || wndRect.Height == 0)
            {
                Logger.Warning($"Unable to open the editor for the plugin {PluginContext.PluginCommandStub.GetEffectName()} because it give us an empty rectangle to work with.");
                return false;
            }
            
            _editorWindow.Show();


            if (hidden)
            {
                int value = 1; // TRUE to disable
                DwmSetWindowAttribute(_editorWindowHelper.Handle,
                    DWMWA_TRANSITIONS_FORCEDISABLED,
                    ref value,
                    Marshal.SizeOf(value));
            }

            var success = PluginContext.PluginCommandStub.EditorOpen(_editorWindowHelper.Handle);

            if (hidden)
            {
                var handle = new HWND(_editorWindowHelper.Handle);
                NativeMethods.SetWindowPos(
                    handle,
                    HWND.BOTTOM,
                    0,
                    0,
                    0,
                    0,
                    SWP.NOMOVE | SWP.NOSIZE);
            }


            if (!success)
            {
                _editorWindow.Close();
                return false;
            }

            if (PluginContext.PluginCommandStub.EditorGetRect(out Rectangle wndRect2))
            {
                if (wndRect2.Width != 0 && wndRect2.Height != 0)
                {
                    _editorWindow.Width = wndRect2.Width;
                    _editorWindow.Height = wndRect2.Height;
                }
            }

            IsEditorOpen = true;
            return true;
        }

        public void ResizeEditor(int width, int height)
        {
            if (IsEditorOpen && width != 0 && height != 0)
            {
                _editorWindow.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    _editorWindow.Width = width;
                    _editorWindow.Height = height;
                }));
            }
        }

        public void RedrawEditor()
        {
            if (IsEditorOpen)
            {
                _editorWindow.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                    new Action(() => { _editorWindow.InvalidateVisual(); }));
            }
        }

        public Bitmap CreateScreenshot()
        {
            return !IsEditorOpen ? null : ScreenCapture.PrintWindow(_editorWindowHelper.Handle);
        }

        public void CloseEditor()
        {
            if (!IsEditorOpen)
            {
                return;
            }

            PluginContext.PluginCommandStub.EditorClose();
            _editorWindow.Close();
            IsEditorOpen = false;
        }

        public List<PluginInfoItem> GetPluginInfoItems(IVstPluginContext pluginContext)
        {
            var pluginInfoItems = new List<PluginInfoItem>();

            if (pluginContext == null)
            {
                return pluginInfoItems;
            }

            // plugin product
            pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Name",
                pluginContext.PluginCommandStub.GetEffectName()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Product",
                pluginContext.PluginCommandStub.GetProductString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Vendor",
                pluginContext.PluginCommandStub.GetVendorString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Vendor Version",
                pluginContext.PluginCommandStub.GetVendorVersion().ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Vst Support",
                pluginContext.PluginCommandStub.GetVstVersion().ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Category",
                pluginContext.PluginCommandStub.GetCategory().ToString()));

            // plugin info
            pluginInfoItems.Add(new PluginInfoItem("Base", "Flags", pluginContext.PluginInfo.Flags.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin ID",
                pluginContext.PluginInfo.PluginID.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin ID String",
                VstUtils.PluginIdNumberToIdString(pluginContext.PluginInfo.PluginID)));

            pluginInfoItems.Add(new PluginInfoItem("Base", "Plugin Version",
                pluginContext.PluginInfo.PluginVersion.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Audio Input Count",
                pluginContext.PluginInfo.AudioInputCount.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Audio Output Count",
                pluginContext.PluginInfo.AudioOutputCount.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Initial Delay",
                pluginContext.PluginInfo.InitialDelay.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Program Count",
                pluginContext.PluginInfo.ProgramCount.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Parameter Count",
                pluginContext.PluginInfo.ParameterCount.ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Base", "Tail Size",
                pluginContext.PluginCommandStub.GetTailSize().ToString()));

            // can do
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Bypass),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Bypass)).ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.MidiProgramNames),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MidiProgramNames))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Offline),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Offline)).ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ReceiveVstEvents),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstEvents))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ReceiveVstMidiEvent),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstMidiEvent))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ReceiveVstTimeInfo),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ReceiveVstTimeInfo))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.SendVstEvents),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstEvents))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.SendVstMidiEvent),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstMidiEvent))
                    .ToString()));

            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.ConformsToWindowRules),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.ConformsToWindowRules))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Metapass),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Metapass))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.MixDryWet),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.MixDryWet))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.Multipass),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.Multipass))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.NoRealTime),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.NoRealTime))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.PlugAsChannelInsert),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsChannelInsert))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.PlugAsSend),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.PlugAsSend))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.SendVstTimeInfo),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.SendVstTimeInfo))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x1in1out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in1out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x1in2out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x1in2out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x2in1out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in1out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x2in2out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in2out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x2in4out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x2in4out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x4in2out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in2out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x4in4out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in4out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x4in8out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x4in8out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x8in4out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in4out))
                    .ToString()));
            pluginInfoItems.Add(new PluginInfoItem("CanDo", nameof(VstPluginCanDo.x8in8out),
                pluginContext.PluginCommandStub.CanDo(VstCanDoHelper.ToString(VstPluginCanDo.x8in8out))
                    .ToString()));

            pluginInfoItems.Add(new PluginInfoItem("Program", "Current Program Index",
                pluginContext.PluginCommandStub.GetProgram().ToString()));
            pluginInfoItems.Add(new PluginInfoItem("Program", "Current Program Name",
                pluginContext.PluginCommandStub.GetProgramName()));

            return pluginInfoItems;
        }
    }
}