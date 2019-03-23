using System.Collections;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Catel.IoC;
using DataGridExtensions;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.Controls
{
    /// <summary>
    /// Interaction logic for MultipleChoiceFilter.xaml
    /// </summary>
    public partial class TypesMultipleChoiceFilter
    {
        private ListBox _listBox;
        public GlobalTypeCollection GlobalTypes { get; }

        public TypesMultipleChoiceFilter()
        {
            GlobalTypes = ServiceLocator.Default.ResolveType<GlobalService>().GlobalTypes;

            InitializeComponent();
        }


        public TypesMultipleChoiceContentFilter Filter
        {
            get { return (TypesMultipleChoiceContentFilter) GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(TypesMultipleChoiceContentFilter),
                typeof(TypesMultipleChoiceFilter),
                new FrameworkPropertyMetadata(new TypesMultipleChoiceContentFilter(null),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (sender, e) => ((TypesMultipleChoiceFilter) sender).Filter_Changed()));


      
        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = Template.FindName("ListBox", this) as ListBox;

            var filter = Filter;

            if (filter?.ExcludedItems == null)
            {
                _listBox?.SelectAll();
            }

            var items = _listBox?.Items as INotifyCollectionChanged;
            if (items == null)
                return;

            items.CollectionChanged += ListBox_ItemsCollectionChanged;
        }

        private void ListBox_ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var filter = Filter;

            if (filter?.ExcludedItems == null)
            {
                _listBox.SelectAll();
            }
            else
            {
                foreach (var item in _listBox.Items.Cast<Type>().Except(filter.ExcludedItems))
                {
                    _listBox.SelectedItems.Add(item);
                }
            }
        }

        private void Filter_Changed()
        {
            var filter = Filter;

            if (filter?.ExcludedItems == null)
            {
                _listBox?.SelectAll();
                return;
            }

            if (_listBox?.SelectedItems.Count != 0)
                return;

            foreach (var item in _listBox.Items.Cast<Type>().Except(filter.ExcludedItems))
            {
                _listBox.SelectedItems.Add(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var excludedItems = Filter?.ExcludedItems ?? new List<Type>();

            var selectedItems = _listBox.SelectedItems.Cast<Type>().ToArray();
            var unselectedItems = _listBox.Items.Cast<Type>().Except(selectedItems).ToArray();

            excludedItems = excludedItems.Except(selectedItems).Concat(unselectedItems).Distinct().ToArray();

            Filter = new TypesMultipleChoiceContentFilter(excludedItems);
        }
    }

    public class TypesMultipleChoiceContentFilter : IContentFilter
    {
        public TypesMultipleChoiceContentFilter(IEnumerable<Type> excludedItems)
        {
            ExcludedItems = excludedItems?.ToArray();
        }

        public IList<Type> ExcludedItems { get; }

        public bool IsMatch(object value)
        {
            var typeCollection = value as TypeCollection;

            return true;
            //return ExcludedItems?.Contains(value as string) != true;
        }
    }
}