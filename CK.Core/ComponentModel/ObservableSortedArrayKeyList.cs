using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System;

namespace CK.Core
{
    /// <summary>
    /// </summary>
    public class ObservableSortedArrayKeyList<T,TKey> : SortedArrayKeyList<T,TKey>, IObservableReadOnlyList<T>
    {
        public ObservableSortedArrayKeyList( Func<T, TKey> keySelector, bool allowDuplicates = false )
            : base( keySelector, allowDuplicates )
        {
        }

        public ObservableSortedArrayKeyList( Func<T, TKey> keySelector, Comparison<TKey> keyComparison, bool allowDuplicates = false )
            : base( keySelector, keyComparison, allowDuplicates )
        {
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            var h = CollectionChanged;
            if( h != null ) h( this, e );
        }

        protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, e );
        }

        private void OnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
        }

        protected override void DoInsert( int index, T value )
        {
            base.DoInsert( index, value );
            OnPropertyChanged( "Count" );
            OnPropertyChanged( "Item[]" );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, value, index ) );
        }

        protected override void DoRemoveAt( int index )
        {
            var item = this[index];
            base.DoRemoveAt( index );
            OnPropertyChanged( "Item[]" );
            OnPropertyChanged( "Count" );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item, index ) );
        }

        protected override T DoSet( int index, T newValue )
        {
            T oldValue = base.DoSet( index, newValue );
            OnPropertyChanged( "Item[]" );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, oldValue, newValue, index ) );
            return oldValue;
        }

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

        protected override void DoClear()
        {
            base.DoClear();
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
    }
}