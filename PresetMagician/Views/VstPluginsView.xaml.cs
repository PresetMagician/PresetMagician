using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Catel;
using Catel.IoC;
using Catel.Windows;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    /// <summary>
    /// Interaction logic for VstPluginListControl.xaml
    /// </summary>
    public partial class VstPluginsView 
    {
  
        public VstPluginsView()
        {
            InitializeComponent();
        }



     
    }
}