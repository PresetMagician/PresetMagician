using MahApps.Metro.IconPacks;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;

namespace PresetMagicianShell.Views
{
    public partial class SettingsView
    {
        public Control Icon { get; protected set; }

        public SettingsView()
        {
            InitializeComponent();

            var x = new PackIconEntypo();
            x.Kind = PackIconEntypoKind.Air;


            Icon = x;

        }
    }
}
