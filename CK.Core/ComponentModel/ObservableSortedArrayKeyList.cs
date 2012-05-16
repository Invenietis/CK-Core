using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="SortedArrayKeyList{T,TKey}"/> that implements <see cref="CollectionChanged"/> and <see cref="PropertyChanged"/> events
    /// in order to be an observable collection.
    /// </summary>
    public class ObservableSortedArrayKeyList<T,TKey> : SortedArrayKeyList<T,TKey>, IObservableReadOnlyList<T>
    {
        /// <summary>
        /// Initializes a new <see cref="ObservableSortedArrayKeyList{T,TKey}"/> with a default comparison function.
        /// </summary>
        /// <param name="keySelector">The function that select the key from an item.</param>
        /// <param name="allowDuplicates">True to allow duplicate items.</param>
        public ObservableSortedArrayKeyList( Func<T, TKey> keySelector, bool allowDuplicates = false )
            : base( keySelector, allowDuplicates )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ObservableSortedArrayKeyList{T,TKey}"/> with a specific comparison function.
        /// </summary>
        /// <param name="keySelector">The function that select the key from an item.</param>
        /// <param name="keyComparison">Comparison function for keys.</param>
        /// <param name="allowDuplicates">True to allow duplicate items.</param>
        public ObservableSortedArrayKeyList( Func<T, TKey> keySelector, Comparison<TKey> keyComparison, bool allowDuplicates = false )
            : base( keySelector, keyComparison, allowDuplicates )
        {
        }

        /// <summary>
        /// Standard <see cref="INotifyCollectionChanged"/> event.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Standard <see cref="INotifyPropertyChanged"/> event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            var h = CollectionChanged;
            if( h != null ) h( this, e );
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event (for "Count" and "Item[]" property).
        /// </summary>
        /// <param name="e">Event argument.</param>
        protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, e );
        }

        private void OnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
        }

        /// <summary>
        /// Overriden to trigger the necessary events.
        /// </summary>
        /// <param name="index">Index to insert.</param>
        /// <param name="value">Item to insert.</param>
        protected override void DoInsert( int index, T value )
        {
            base.DoInsert( index, value );
            OnPropertyChanged( "Count" );
            OnPropertyChanged( "Item[]" );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, value, index ) );
        }

        /// <summary>
        /// Overriden to trigger the necessary events.
        /// </summary>
        /// <param name="index">Index to remove.</param>
        protected override void DoRemoveAt( int index )
        {
            var item = this[index];
            base.DoRemoveAt( index );
            OnPropertyChanged( "Item[]" );
            OnPropertyChanged( "Count" );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item, index ) );
        }

        /// <summary>
        /// Overriden to trigger the necessary events.
        /// </summary>
        /// <param name="index">The position to set.</param>
        /// <param name="newValue">The new item to inject.</param>
        /// <returns>The previous item at the position.</returns>
        protected override T DoSet( int index, T newValue )
        {
            T oldValue = base.DoSet( index, newValue );
            OnPropertyChanged( "Item[]" );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, oldValue, newValue, index ) );
            return oldValue;
        }

        /// <summary>
        /// Overriden to trigger the necessary events.
        /// </summary>
        /// <param name="from">Old index of the item.</param>
        /// <param name="newIndex">New index.</param>
        /// <returns>The new index of the element.</returns>
        protected override int DoMove( int from, int newIndex )
        {
            newIndex = base.DoMove( from, newIndex );
            if( newIndex != from )
            {
                OnPropertyChanged( "Item[]" );
                OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Move, Store[newIndex], newIndex, from ) );
            }
            return newIndex;
        }

        /// <summary>
        /// Overriden to trigger the necessary events.
        /// </summary>
        protected override void DoClear()
        {
            base.DoClear();
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
    }
}