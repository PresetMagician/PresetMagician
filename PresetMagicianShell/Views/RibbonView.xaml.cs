using Orchestra;

namespace PresetMagicianShell.Views
{
    /// <summary>
    /// Interaction logic for RibbonView.xaml
    /// </summary>
    public partial class RibbonView
    {
        public RibbonView()
        {
            InitializeComponent();
            ribbon.AddAboutButton();
        }
    }
}