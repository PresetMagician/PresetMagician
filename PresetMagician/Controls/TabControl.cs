using System.Windows;

namespace PresetMagician.Controls
{
    public class TabControl : System.Windows.Controls.TabControl
    {
        static TabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControl),
                new FrameworkPropertyMetadata(typeof(TabControl)));
        }
    }
}