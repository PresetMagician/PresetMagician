using System;
using System.Collections.Specialized;
using System.ComponentModel;
using SharedModels.Collections;

namespace SharedModels.Data
{
    /// <summary>
    /// The notify list changed event args.
    /// </summary>
    public class WrappedCollectionItemPropertyChangedEventArgs : EventArgs
    {
        public string SourceProperty { get; private set; }
        public PropertyChangedEventArgs OriginalPropertyChangedEventArgs { get; private set; }

        public WrappedCollectionItemPropertyChangedEventArgs(string sourceProperty, PropertyChangedEventArgs originalPropertyChangedEventArgs)
        {
            SourceProperty = sourceProperty;
            OriginalPropertyChangedEventArgs = originalPropertyChangedEventArgs;
        }
    }
    
    public class WrappedCollectionChangedEventArgs : EventArgs
    {
        public string SourceProperty { get; private set; }
        public NotifyCollectionChangedEventArgs NotifyCollectionChangedEventArgs { get; private set; }

        public WrappedCollectionChangedEventArgs(string sourceProperty, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SourceProperty = sourceProperty;
            NotifyCollectionChangedEventArgs = notifyCollectionChangedEventArgs;
        }
    }

    public class CollectionChangeNotificationWrapper
    {
        private readonly string _sourceProperty;
        private readonly ITrackableCollection _sourceObject;

        public CollectionChangeNotificationWrapper(ITrackableCollection value, string sourceProperty)
        {
            _sourceProperty = sourceProperty;
            
            value.CollectionChanged += ChangeNotificationWrapperOnCollectionChanged;
            value.ItemPropertyChanged +=
                ChangeNotificationWrapperOnCollectionItemPropertyChanged;
            
            _sourceObject = value;
           
        }

        public void Unsubscribe()
        {
            _sourceObject.CollectionChanged -= ChangeNotificationWrapperOnCollectionChanged;
            _sourceObject.ItemPropertyChanged -=
                ChangeNotificationWrapperOnCollectionItemPropertyChanged;
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