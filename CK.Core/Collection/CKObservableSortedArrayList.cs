#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKObservableSortedArrayList.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="CKSortedArrayList{T}"/> that is observable.
    /// </summary>
    public class CKObservableSortedArrayList<T> : CKSortedArrayList<T>, ICKObservableReadOnlyList<T>
    {
        /// <summary>
        /// Initializes a new <see cref="CKObservableSortedArrayList{T}"/> with a default comparer and no duplicates.
        /// </summary>
        public CKObservableSortedArrayList() 
            : base() 
        { 
        }

        /// <summary>
        /// Initializes a new <see cref="CKObservableSortedArrayList{T}"/> with a <see cref="IComparer{T}"/> 
        /// and that rejects or allows duplicates.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="allowDuplicates">True to allow duplicate items.</param>
        public CKObservableSortedArrayList( IComparer<T> comparer, bool allowDuplicates = false )
            : base( comparer, allowDuplicates )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKObservableSortedArrayList{T}"/> with a comparison function 
        /// and that rejects or allows duplicates.
        /// </summary>
        /// <param name="comparison">Comparison function to use.</param>
        /// <param name="allowDuplicates">True to allow duplicate items.</param>
        public CKObservableSortedArrayList( Comparison<T> comparison, bool allowDuplicates = false )
            : base( comparison, allowDuplicates )
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
        protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
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