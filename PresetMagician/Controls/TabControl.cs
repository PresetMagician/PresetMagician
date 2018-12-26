using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PresetMagician.Controls
{
    public class TabControl: System.Windows.Controls.TabControl
    {
        static TabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControl),
                new FrameworkPropertyMetadata(typeof(TabControl)));
        }
    }
}
