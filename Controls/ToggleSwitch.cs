using System.Windows;
using System.Windows.Controls.Primitives;

namespace SlideDock.Controls
{
    public class ToggleSwitch : ToggleButton
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(typeof(ToggleSwitch))
            );
        }
    }
}
