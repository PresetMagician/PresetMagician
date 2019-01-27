using System.Windows;

namespace PresetMagician.Extensions
{
    /// <summary>
    /// Class with extension methods for <see cref="FrameworkElement"/>.
    /// </summary>
    internal static class FrameworkElementExtensions
    {
        public static void ForceMeasure(this FrameworkElement element)
        {
            element.InvalidateMeasure();

            // Calling UpdateLayout on not already loaded elements causes weird side effects
            if (element.IsLoaded)
            {
                element.UpdateLayout();
            }
        }

        public static void ForceMeasureAndArrange(this FrameworkElement element)
        {
            element.InvalidateMeasure();
            element.InvalidateArrange();

            // Calling UpdateLayout on not already loaded elements causes weird side effects
            if (element.IsLoaded)
            {
                element.UpdateLayout();
            }
        }
    }
}