using System;
using System.Collections.Specialized;
using System.ComponentModel;
using PresetMagician.Core.Collections;

namespace PresetMagician.Core.Data
{
    /// <summary>
    /// The notify list changed event args.
    /// </summary>
    public class WrappedCollectionItemPropertyChangedEventArgs : System.EventArgs
    {
        public string SourceProperty { get; private set; }
        public PropertyChangedEventArgs OriginalPropertyChangedEventArgs { get; private set; }

        public WrappedCollectionItemPropertyChangedEventArgs(string sourceProperty, PropertyChangedEventArgs originalPropertyChangedEventArgs)
        {
            SourceProperty = sourceProperty;
            OriginalPropertyChangedEventArgs = originalPropertyChangedEventArgs;
        }
    }
    
  
    
    public class WrappedCollectionChangedEventArgs : System.EventArgs
    {
        public string SourceProperty { get; private set; }
        public NotifyCollectionChangedEventArgs NotifyCollectionChangedEventArgs { get; private set; }

        public WrappedCollectionChangedEventArgs(string sourceProperty, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SourceProperty = sourceProperty;
            NotifyCollectionChangedEventArgs = notifyCollectionChangedEventArgs;
        }
    }
    
    public class UserEditableChangeNotificationWrapper
    {
        private readonly string _sourceProperty;
        private readonly IEditableCollection _sourceCollection;
        private readonly ModelBase _sourceObject;

        public UserEditableChangeNotificationWrapper(IEditableCollection value, string sourceProperty)
        {
            _sourceProperty = sourceProperty;
            
            value.CollectionChanged += ChangeNotificationWrapperOnCollectionChanged;
            value.PropertyChanged += ValueOnPropertyChanged;
            /*value.ItemPropertyChanged +=
                ChangeNotificationWrapperOnCollectionItemPropertyChanged;*/
            
            _sourceCollection = value;
           
        }

        private void ValueOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CollectionItemPropertyChanged?.Invoke(sender, new WrappedCollectionItemPropertyChangedEventArgs(_sourceProperty,e));
        }

        public UserEditableChangeNotificationWrapper(PresetMagician.Core.Data.ModelBase value, string sourceProperty)
        {
            _sourceProperty = sourceProperty;
            
            value.PropertyChanged +=
                ChangeNotificationWrapperOnCollectionItemPropertyChanged;
            
            _sourceObject = value;
        }

        public void Unsubscribe()
        {
            if (_sourceCollection != null)
            {
                _sourceCollection.CollectionChanged -= ChangeNotificationWrapperOnCollectionChanged;
                _sourceCollection.PropertyChanged -= ValueOnPropertyChanged;
                /*_sourceCollection.ItemPropertyChanged -=
                    ChangeNotificationWrapperOnCollectionItemPropertyChanged;*/
            }

            if (_sourceObject != null)
            {
                _sourceObject.PropertyChanged -= ChangeNotificationWrapperOnCollectionItemPropertyChanged;
            }
        }

        private void ChangeNotificationWrapperOnCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CollectionItemPropertyChanged?.Invoke(sender, new WrappedCollectionItemPropertyChangedEventArgs(_sourceProperty,e));
        }

        private void ChangeNotificationWrapperOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, new WrappedCollectionChangedEventArgs(_sourceProperty, e));
        }
        
        /// <summary>
        /// Occurs when the <see cref="PropertyChanged"/> event occurs in the collection when the target object is a collection.
        /// </summary>
        public event EventHandler<WrappedCollectionItemPropertyChangedEventArgs> CollectionItemPropertyChanged;

        /// <summary>
        /// Occurs when the <see cref="CollectionChanged"/> event occurs on the target object.
        /// </summary>
        public event EventHandler<WrappedCollectionChangedEventArgs> CollectionChanged;
    }
}