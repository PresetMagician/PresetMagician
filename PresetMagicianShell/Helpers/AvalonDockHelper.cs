// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvalonDockHelper.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2012 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PresetMagicianShell.Helpers
{
    using System;
    using System.Linq;
    using System.Windows;
   
    using Catel;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.MVVM.Views;
    using Xceed.Wpf.AvalonDock;
    using Xceed.Wpf.AvalonDock.Layout;


    /// <summary>
    /// Helper class for avalon dock.
    /// </summary>
    public class AvalonDockHelper
    {
        /// <summary>
        /// Docking manager reference.
        /// </summary>
        private static readonly DockingManager DockingManager;

        /// <summary>
        /// The layout document pane.
        /// </summary>
        private static readonly LayoutDocumentPane LayoutDocumentPane;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <remarks></remarks>
        static AvalonDockHelper()
        {
            DockingManager = ServiceLocator.Default.ResolveType<DockingManager>();
            DockingManager.DocumentClosed += OnDockingManagerDocumentClosed;

            LayoutDocumentPane = ServiceLocator.Default.ResolveType<LayoutDocumentPane>();
        }

        #region Properties

        #endregion

        #region Methods
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <param name="viewType">Type of the view.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The found document or <c>null</c> if no document was found.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="viewType"/> is <c>null</c>.</exception>
        public static LayoutDocument FindDocument(Type viewType, object tag = null)
        {
            Argument.IsNotNull("viewType", viewType);

            return (from document in LayoutDocumentPane.Children
                    where document is LayoutDocument && document.Content.GetType() == viewType &&
                          TagHelper.AreTagsEqual(tag, ((IView)document.Content).Tag)
                    select document).Cast<LayoutDocument>().FirstOrDefault();
        }

        public static LayoutDocument FindDocument<TService>(object tag=null)
        {
            var sl = ServiceLocator.Default;
            var viewModel = sl.ResolveType<TService>();
            var viewLocator = sl.ResolveType<IViewLocator>();
            var viewType = viewLocator.ResolveView(viewModel.GetType());

            return AvalonDockHelper.FindDocument(viewType, tag);
        }

        public static LayoutDocument CreateDocument<TService>(object tag = null)
        {
            var sl = ServiceLocator.Default;
            var viewModel = sl.ResolveType<TService>();
            var viewLocator = sl.ResolveType<IViewLocator>();
            var viewType = viewLocator.ResolveView(viewModel.GetType());

            var document = AvalonDockHelper.FindDocument(viewType, tag);
            if (document == null)
            {
                var view = ViewHelper.ConstructViewWithViewModel(viewType, viewModel);
                document = AvalonDockHelper.CreateDocument(view, tag);
            }

            AvalonDockHelper.ActivateDocument(document);

            return document;
        }
        /// <summary>
        /// Activates the document in the docking manager, which makes it the active document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="document"/> is <c>null</c>.</exception>
        public static void ActivateDocument(LayoutDocument document)
        {
            Argument.IsNotNull("document", document);

            LayoutDocumentPane.SelectedContentIndex = LayoutDocumentPane.IndexOfChild(document);
        }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The created layout document.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="view"/> is <c>null</c>.</exception>
        public static LayoutDocument CreateDocument(FrameworkElement view, object tag = null)
        {
            Argument.IsNotNull("view", view);

            var layoutDocument = WrapViewInLayoutDocument(view, tag);
            
            LayoutDocumentPane.Children.Add(layoutDocument);

            return layoutDocument;
        }

        /// <summary>
        /// Wraps the view in a layout document.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>A wrapped layout document.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="view"/> is <c>null</c>.</exception>
        private static LayoutDocument WrapViewInLayoutDocument(FrameworkElement view, object tag = null)
        {
            Argument.IsNotNull("view", view);

            var layoutDocument = new LayoutDocument();

            layoutDocument.CanFloat = false;
            // TODO: Make bindable => automatic updates
            layoutDocument.Title = ((IViewModel)view.DataContext).Title;
            layoutDocument.Content = view;
            

            view.Tag = tag;

            return layoutDocument;
        }

        /// <summary>
        /// Called when the docking manager has just closed a document.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AvalonDock.DocumentClosedEventArgs"/> instance containing the event data.</param>
        private static void OnDockingManagerDocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            /*var containerView = e.Document;
            var view = containerView.Content as IDocumentView;
            if (view != null)
            {
                view.CloseDocument();
            }*/

            //var region = RegionManager.Regions[(string)view.Tag];
            //region.Remove(sender);
        }
        #endregion
    }
}