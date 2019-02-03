using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.DwayneNeed.Interop
{
    // This is always the root element of a HwndSourceHwndHost's inner frame.
    // It allows the logical parent to be surgically disabled in order to
    // work around the problem with IKIS routed events walking the logical
    // tree and escaping.
    public class HwndSourceHostRoot : Decorator
    {
        public static readonly DependencyProperty IsLogicalParentEnabledProperty =
            DependencyProperty.Register("IsLogicalParentEnabled", typeof(bool), typeof(HwndSourceHostRoot),
                new UIPropertyMetadata(true));

        public bool IsLogicalParentEnabled
        {
            get { return (bool) GetValue(IsLogicalParentEnabledProperty); }
            set { SetValue(IsLogicalParentEnabledProperty, value); }
        }

        protected override DependencyObject GetUIParentCore()
        {
            if (IsLogicalParentEnabled)
            {
                return base.GetUIParentCore();
            }
            else
            {
                return null;
            }
        }

        //TODO: must solve HwndHost brittle base class problem first.
        //protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        //{
        //    // Draw something so that rendering does something.  This
        //    // is part of the fix for rendering artifacts, see WndProc
        //    // hooking WM_WINDOWPOSCHANGED too.
        //    drawingContext.DrawRectangle(Brushes.Red, null, new Rect(RenderSize));

        //    base.OnRender(drawingContext);
        //}

        public event EventHandler OnMeasure;

        /// <summary>
        ///     This virtual is called when the system measured the child
        ///     and the result was different.  The default behavior is
        ///     to invalidate measure on the element.  However, since
        ///     we are not visually connected to the containing
        ///     HwndSourceHost, this doesn't do anything useful.  We
        ///     manually tell the HwndSourceHost so it can invalidate
        ///     measure on itself.
        /// </summary>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            EventHandler handler = OnMeasure;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            base.OnChildDesiredSizeChanged(child);
        }
    }
}