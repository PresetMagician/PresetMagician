using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    class VstPluginInfoViewModel : VstPluginViewModel
    {
        public VstPluginInfoViewModel(Models.Plugin plugin, IVstService vstService, IOpenFileService openFileService) : base(vstService, openFileService)
        {
            Plugin = plugin;
        }

    }
}
