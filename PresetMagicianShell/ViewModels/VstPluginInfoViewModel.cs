using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using PresetMagicianShell.Models;

namespace PresetMagicianShell.ViewModels
{
    class VstPluginInfoViewModel : VstPluginViewModel
    {
        public VstPluginInfoViewModel(Models.Plugin plugin) : base(plugin)
        {

        }

    }
}
