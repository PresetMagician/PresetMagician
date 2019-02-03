using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using Catel.MVVM;
using Catel.Windows;
using ControlzEx.Behaviors;
using ControlzEx.Windows.Shell;
using Fluent;
using Fluent.Helpers;
using PresetMagician.Extensions;
using SystemCommands = ControlzEx.Windows.Shell.SystemCommands;

namespace PresetMagician.Controls
{
    [TemplatePart(Name = PART_Icon, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_ContentPresenter, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_RibbonTitleBar, Type = typeof(RibbonTitleBar))]
    [TemplatePart(Name = PART_WindowCommands, Type = typeof(WindowCommands))]
    public class RibbonDataWindow : DataWindow
    {
        private FrameworkElement iconImage;

        static RibbonDataWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonDataWindow),
                new FrameworkPropertyMetadata(typeof(RibbonDataWindow)));

            BorderThicknessProperty.OverrideMetadata(typeof(RibbonDataWindow),
                new FrameworkPropertyMetadata(new Thickness(1)));
            WindowStyleProperty.OverrideMetadata(typeof(RibbonDataWindow),
                new FrameworkPropertyMetadata(WindowStyle.None));
        }

        public RibbonDataWindow()
        {
        }

        public RibbonDataWindow(DataWindowMode mode, IEnumerable<DataWindowButton> additionalButtons = null,
            DataWindowDefaultButton defaultButton = DataWindowDefaultButton.OK, bool setOwnerAndFocus = true,
            InfoBarMessageControlGenerationMode infoBarMessageControlGenerationMode =
                InfoBarMessageControlGenerationMode.Inline, bool focusFirstControl = true) : base(mode,
            additionalButtons, defaultButton, setOwnerAndFocus, infoBarMessageControlGenerationMode, focusFirstControl)
        {
        }

        public RibbonDataWindow(IViewModel viewModel) : base(viewModel)
        {
        }

        public RibbonDataWindow(IViewModel viewModel, DataWindowMode mode,
            IEnumerable<DataWindowButton> additionalButtons = null,
            DataWindowDefaultButton defaultButton = DataWindowDefaultButton.OK, bool setOwnerAndFocus = true,
            InfoBarMessageControlGenerationMode infoBarMessageControlGenerationMode =
                InfoBarMessageControlGenerationMode.Inline, bool focusFirstControl = true) : base(viewModel, mode,
            additionalButtons, defaultButton, setOwnerAndFocus, infoBarMessageControlGenerationMode, focusFirstControl)
        {
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;

            // WindowChromeBehavior initialization has to occur in constructor. Otherwise the load event is fired early and performance of the window is degraded.
            InitializeWindowChromeBehavior();
            this.SizeToContent = SizeToContent.Manual;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        #region Overrides

        /// <summary>
        ///     Initializes the WindowChromeBehavior which is needed to render the custom WindowChrome.
        /// </summary>
        private void InitializeWindowChromeBehavior()
        {
            var behavior = new WindowChromeBehavior();
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.ResizeBorderThicknessProperty,
                new Binding {Path = new PropertyPath(ResizeBorderThicknessProperty), Source = this});
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.GlassFrameThicknessProperty,
                new Binding {Path = new PropertyPath(GlassFrameThicknessProperty), Source = this});
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.IgnoreTaskbarOnMaximizeProperty,
                new Binding {Path = new PropertyPath(IgnoreTaskbarOnMaximizeProperty), Source = this});

            Interaction.GetBehaviors(this).Add(behavior);
        }

        #endregion

        // Size change to collapse ribbon
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaintainIsCollapsed();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SizeToContent == SizeToContent.Manual)
            {
                return;
            }

            this.RunInDispatcherAsync(() =>
            {
                // Fix for #454 while also keeping #473
                var availableSize = new Size(TitleBar.ActualWidth, TitleBar.ActualHeight);
                TitleBar.Measure(availableSize);
                TitleBar.ForceMeasureAndArrange();
            }, DispatcherPriority.ApplicationIdle);
        }

        private void MaintainIsCollapsed()
        {
            if (IsAutomaticCollapseEnabled == false)
            {
                return;
            }

            if (ActualWidth < Ribbon.MinimalVisibleWidth
                || ActualHeight < Ribbon.MinimalVisibleHeight)
            {
                IsCollapsed = true;
            }
            else
            {
                IsCollapsed = false;
            }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            TitleBar = GetTemplateChild(PART_RibbonTitleBar) as RibbonTitleBar;

            if (iconImage != null)
            {
                iconImage.MouseDown -= HandleIconMouseDown;
            }

            if (WindowCommands == null)
            {
                WindowCommands = new WindowCommands();
            }

            iconImage = GetPart<FrameworkElement>(PART_Icon);

            if (iconImage != null)
            {
                iconImage.MouseDown += HandleIconMouseDown;
            }

            GetPart<UIElement>(PART_Icon)?.SetValue(WindowChrome.IsHitTestVisibleInChromeProperty, true);
            GetPart<UIElement>(PART_WindowCommands)?.SetValue(WindowChrome.IsHitTestVisibleInChromeProperty, true);
        }

        /// <inheritdoc />
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            // todo: remove fix if we update to ControlzEx 4.0
            if (WindowState == WindowState.Maximized
                && SizeToContent != SizeToContent.Manual)
            {
                SizeToContent = SizeToContent.Manual;
            }

            this.RunInDispatcherAsync(() => TitleBar?.ForceMeasureAndArrange(), DispatcherPriority.Background);
        }

        private void HandleIconMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (e.ClickCount == 1)
                    {
                        e.Handled = true;

                        WindowSteeringHelper.ShowSystemMenu(this, PointToScreen(new Point(0, TitleBarHeight)));
                    }
                    else if (e.ClickCount == 2)
                    {
                        e.Handled = true;

#pragma warning disable 618
                        SystemCommands.CloseWindow(this);
#pragma warning restore 618
                    }

                    break;

                case MouseButton.Right:
                    e.Handled = true;

                    WindowSteeringHelper.ShowSystemMenu(this, e);
                    break;
            }
        }

        /// <summary>
        ///     Gets the template child with the given name.
        /// </summary>
        /// <typeparam name="T">The interface type inheirted from DependencyObject.</typeparam>
        /// <param name="name">The name of the template child.</param>
        internal T GetPart<T>(string name)
            where T : DependencyObject
        {
            return GetTemplateChild(name) as T;
        }
        // ReSharper disable InconsistentNaming
#pragma warning disable SA1310 // Field names must not contain underscore
        private const string PART_Icon = "PART_Icon";
        private const string PART_ContentPresenter = "PART_ContentPresenter";
        private const string PART_RibbonTitleBar = "PART_RibbonTitleBar";
        private const string PART_WindowCommands = "PART_WindowCommands";
#pragma warning restore SA1310 // Field names must not contain underscore
        // ReSharper restore InconsistentNaming

        #region Properties

        #region TitelBar

        /// <summary>
        ///     Gets ribbon titlebar
        /// </summary>
        public RibbonTitleBar TitleBar
        {
            get => (RibbonTitleBar) GetValue(TitleBarProperty);
            private set => SetValue(titleBarPropertyKey, value);
        }

        private static readonly DependencyPropertyKey titleBarPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(TitleBar), typeof(RibbonTitleBar), typeof(RibbonDataWindow),
                new PropertyMetadata());

        /// <summary>
        ///     <see cref="DependencyProperty" /> for <see cref="TitleBar" />.
        /// </summary>
        public static readonly DependencyProperty TitleBarProperty = titleBarPropertyKey.DependencyProperty;

        #endregion

        /// <summary>
        ///     Gets or sets the height which is used to render the window title.
        /// </summary>
        public double TitleBarHeight
        {
            get => (double) GetValue(TitleBarHeightProperty);
            set => SetValue(TitleBarHeightProperty, value);
        }

        /// <summary>
        ///     <see cref="DependencyProperty" /> for <see cref="TitleBarHeight" />.
        /// </summary>
        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register(nameof(TitleBarHeight), typeof(double), typeof(RibbonDataWindow),
                new PropertyMetadata(0.0D));

        /// <summary>
        ///     Gets or sets the <see cref="Brush" /> which is used to render the window title.
        /// </summary>
        public Brush TitleForeground
        {
            get => (Brush) GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }

        /// <summary>
        ///     <see cref="DependencyProperty" /> for <see cref="TitleForeground" />.
        /// </summary>
        public static readonly DependencyProperty TitleForegroundProperty =
            DependencyProperty.Register(nameof(TitleForeground), typeof(Brush), typeof(RibbonDataWindow),
                new PropertyMetadata());

        /// <summary>
        ///     Gets or sets the <see cref="Brush" /> which is used to render the window title background.
        /// </summary>
        public Brush TitleBackground
        {
            get => (Brush) GetValue(TitleBackgroundProperty);
            set => SetValue(TitleBackgroundProperty, value);
        }

        /// <summary>
        ///     <see cref="DependencyProperty" /> for <see cref="TitleBackground" />.
        /// </summary>
        public static readonly DependencyProperty TitleBackgroundProperty =
            DependencyProperty.Register(nameof(TitleBackground), typeof(Brush), typeof(RibbonDataWindow),
                new PropertyMetadata());

        /// <summary>
        ///     Using a DependencyProperty as the backing store for WindowCommands.  This enables animation, styling, binding,
        ///     etc...
        /// </summary>
        public static readonly DependencyProperty WindowCommandsProperty =
            DependencyProperty.Register(nameof(WindowCommands), typeof(WindowCommands), typeof(RibbonDataWindow),
                new PropertyMetadata());

        /// <summary>
        ///     Gets or sets the window commands
        /// </summary>
        public WindowCommands WindowCommands
        {
            get => (WindowCommands) GetValue(WindowCommandsProperty);
            set => SetValue(WindowCommandsProperty, value);
        }

        /// <summary>
        ///     Gets or sets resize border thickness
        /// </summary>
        public Thickness ResizeBorderThickness
        {
            get => (Thickness) GetValue(ResizeBorderThicknessProperty);
            set => SetValue(ResizeBorderThicknessProperty, value);
        }

        /// <summary>
        ///     Using a DependencyProperty as the backing store for ResizeBorderTickness.  This enables animation, styling,
        ///     binding, etc...
        /// </summary>
        public static readonly DependencyProperty ResizeBorderThicknessProperty =
            DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(RibbonDataWindow),
                new PropertyMetadata(new Thickness(8D))); //WindowChromeBehavior.GetDefaultResizeBorderThickness()));

        /// <summary>
        ///     Gets or sets glass border thickness
        /// </summary>
        public Thickness GlassFrameThickness
        {
            get => (Thickness) GetValue(GlassFrameThicknessProperty);
            set => SetValue(GlassFrameThicknessProperty, value);
        }

        /// <summary>
        ///     Using a DependencyProperty as the backing store for GlassFrameThickness.
        ///     GlassFrameThickness != 0 enables the default window drop shadow.
        /// </summary>
        public static readonly DependencyProperty GlassFrameThicknessProperty =
            DependencyProperty.Register(nameof(GlassFrameThickness), typeof(Thickness), typeof(RibbonDataWindow),
                new PropertyMetadata(new Thickness(1)));

        /// <summary>
        ///     Gets or sets whether icon is visible
        /// </summary>
        public bool IsIconVisible
        {
            get => (bool) GetValue(IsIconVisibleProperty);
            set => SetValue(IsIconVisibleProperty, value);
        }

        /// <summary>
        ///     Gets or sets whether icon is visible
        /// </summary>
        public static readonly DependencyProperty IsIconVisibleProperty =
            DependencyProperty.Register(nameof(IsIconVisible), typeof(bool), typeof(RibbonDataWindow),
                new FrameworkPropertyMetadata(true));

        // todo check if IsCollapsed and IsAutomaticCollapseEnabled should be reduced to one shared property for DataWindow and Ribbon

        /// <summary>
        ///     Gets whether window is collapsed
        /// </summary>
        public bool IsCollapsed
        {
            get => (bool) GetValue(IsCollapsedProperty);
            set => SetValue(IsCollapsedProperty, value);
        }

        /// <summary>
        ///     Using a DependencyProperty as the backing store for IsCollapsed.
        ///     This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register(nameof(IsCollapsed),
            typeof(bool), typeof(RibbonDataWindow), new PropertyMetadata(false));

        /// <summary>
        ///     Defines if the Ribbon should automatically set <see cref="IsCollapsed" /> when the width or height of the owner
        ///     window drop under <see cref="Ribbon.MinimalVisibleWidth" /> or <see cref="Ribbon.MinimalVisibleHeight" />
        /// </summary>
        public bool IsAutomaticCollapseEnabled
        {
            get => (bool) GetValue(IsAutomaticCollapseEnabledProperty);
            set => SetValue(IsAutomaticCollapseEnabledProperty, value);
        }

        /// <summary>
        ///     Using a DependencyProperty as the backing store for IsCollapsed.
        ///     This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsAutomaticCollapseEnabledProperty =
            DependencyProperty.Register(nameof(IsAutomaticCollapseEnabled), typeof(bool), typeof(RibbonDataWindow),
                new PropertyMetadata(true));

        /// <summary>
        ///     Defines if the taskbar should be ignored and hidden while the window is maximized.
        /// </summary>
        public bool IgnoreTaskbarOnMaximize
        {
            get => (bool) GetValue(IgnoreTaskbarOnMaximizeProperty);
            set => SetValue(IgnoreTaskbarOnMaximizeProperty, value);
        }

        /// <summary>
        ///     <see cref="DependencyProperty" /> for <see cref="IgnoreTaskbarOnMaximize" />.
        /// </summary>
        public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty =
            DependencyProperty.Register(nameof(IgnoreTaskbarOnMaximize), typeof(bool), typeof(RibbonDataWindow),
                new PropertyMetadata(default(bool)));

        #endregion
    }
}