using System;
using System.Diagnostics;
using System.Windows;
using Catel.IoC;
using Orchestra;
using Orchestra.Services;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using ThemeHelper2 = Catel.ThemeHelper;
namespace PresetMagicianShell.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class ThemeControlsWindow
    {
        public ThemeControlsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox mb = new MessageBox();
            mb.Caption = "Caption";
            mb.Text = "Text";
            mb.ShowDialog();
        }
        
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            Debug.WriteLine("bla 1");
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Themes/Master.xaml")});
            var serviceLocator = ServiceLocator.Default;
            var themeService = serviceLocator.ResolveType<IThemeService>();

            // Note: we only have to create style forwarders once
            Debug.WriteLine("bla 2");
            ThemeHelper.EnsureApplicationThemes(Application.Current.GetType().Assembly, true);
            Debug.WriteLine("bla 3");
            ThemeHelper.EnsureApplicationThemes(typeof(ThemeHelper).Assembly, true);
            Debug.WriteLine("bla 4");
            ThemeHelper.EnsureApplicationThemes(typeof(ApplicationInitializationServiceBase).Assembly, false);
            Debug.WriteLine("bla 5");
            ThemeHelper.EnsureApplicationThemes(GetType().Assembly, false);
            Debug.WriteLine("bla 6");
            StyleHelper.CreateStyleForwardersForDefaultStyles();

            Debug.WriteLine("bla 7");
            FluentRibbonHelper.ApplyTheme();
            Debug.WriteLine("bla 8");
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Catel.MVVM;component/themes/generic.xaml", UriKind.RelativeOrAbsolute)});

            
        }
    }
}