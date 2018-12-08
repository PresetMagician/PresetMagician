using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.Reflection;
using PresetMagicianShell.Helpers;
using PresetMagicianShell.Views;

namespace PresetMagicianShell.ViewModels
{
    
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<IView> Documents { get; }

        public MainViewModel()
        {
            

            Documents = new ObservableCollection<IView>();

           
   
        }
    }
}