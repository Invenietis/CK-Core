#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\ModeObject\ModeObject.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    interface IModeDependantObjectImpl<T> where T : IModeDependantObjectImpl<T>
    {
        IKeyboardMode Mode { get; set; }
        T Prev { get; set; }
    }

    /// <summary>
    /// This class embbeds a linked chain, it may be seen as this :
    /// the default mode is the very first item. then, modes are sorted in a descending way : the "greater" the mode, the closer it will be to the default mode.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TI"></typeparam>
    abstract class ModeObjectRoot<T,TI> : IReadOnlyCollection<TI>
        where T : class, IModeDependantObjectImpl<T>, TI
    {
        T _first;
        T _last;
        T _current;
        int _count;
        IReadOnlyCollection<T> _enumObjects;

        protected T First { get { return _first; } }
        protected T Last { get { return _last; } }
        public T Current { get { return _current; } }
        protected IReadOnlyCollection<T> Objects { get { return _enumObjects; } }


        protected void Initialize( IKeyboardContextMode contextMode )
        {
            _count = 1;
            _enumObjects = new EO( this );
            _current = _first = _last = DoCreate( contextMode.EmptyMode );
        }

        public TI this[ IKeyboardMode mode ]
        {
            get { return (TI)Find( mode ); } 
        }

        public T Find( IKeyboardMode mode ) 
        {
            T c = _last;
            do
            {
                if( c.Mode == mode ) return c;
                c = c.Prev;
            } 
            while( c != null );
            return c;
        }

        public TI FindBest( IKeyboardMode mode )
        {
            return FindBest( _last, mode );
        }

        internal T FindBest( T last, IKeyboardMode mode )
        {
            Debug.Assert( last == _last, "We currently do not skip any object." );
            for(;;)
            {
                if( last == _first || mode.ContainsAll( last.Mode ) ) return last;
                last = last.Prev;
            }
        }

        public T FindOrCreate( IKeyboardMode mode )
        {
            Debug.Assert( _first != null, "Default (first) object always exists." );
            // Finds the position and exits if an existing object is found.
            T c = _last, cNext = null;
            do
            {
                int cmp = c.Mode.CompareTo( mode ); //returns > 0 if mode is before c in the linked chain
                if( cmp == 0 ) return c;
                if( cmp < 0 ) break;
                cNext = c;
                c = c.Prev;
            }
            while( c != null );
            
            if( !GetAvailableMode().ContainsAll( mode ) )
                throw new CKException( R.ModeObjectCreateUnavailableMode, typeof( TI ).Name, mode.Remove( GetAvailableMode() ) );
            
            T o = DoCreate( mode );
            Debug.Assert( o.Mode == mode );
            Debug.Assert( o.Prev == null );
            InsertBefore( o, cNext );
            ++_count;
            OnCreated( o );
            CheckBestCurrent( o );
            return o;
        }

        private void InsertBefore( T toInsert, T e )
        {
            Debug.Assert( toInsert.Prev == null );
            if( e == null )
            {
                toInsert.Prev = _last;
                _last = toInsert;
            }
            else
            {
                Debug.Assert( e != _first, "We can not insert before the default (first) object." );
                toInsert.Prev = e.Prev;
                e.Prev = toInsert;
            }
        }

        private void CheckBestCurrent( T o )
        {
            if( IsBestCurrentMode( o.Mode ) )
            {
                _current = o;
                OnCurrentChanged();
            }
        }

        /// <summary>
        /// If the proposed mode is greater than the current object's mode,
        /// and contains only atomic modes of the current keyboard mode: the proposed mode will be a better current.
        /// </summary>
        /// <param name="mode">Proposed mode.</param>
        /// <returns>True if the <paramref name="mode"/> should replace the actual current object.</returns>
        bool IsBestCurrentMode( IKeyboardMode mode )
        {
            return mode.CompareTo( _current.Mode ) > 0 && GetCurrentMode().ContainsAll( mode );
        }

        public void Destroy( T o )
        {
            Debug.Assert( Contains( o ), "It is one of our objects." );
            Debug.Assert( !o.Mode.IsEmpty, "It is not the default one." );

            if( _current == o )
            {
                _current = o.Prev;
                Debug.Assert( _current != null, "Since o is not the default (first) one." );
                OnCurrentChanged();
            }
            RemoveFromList( o, GetNext( o ) );
            --_count;
            Debug.Assert( _count > 0 );
            OnDestroyed( o );
        }

        private T GetNext( T o )
        {
            T cNext = null, c = _last;
            do
            {
                if( c == o ) break;
                cNext = c;
                c = c.Prev;
            }
            while( c != null );
            return cNext;
        }

        private void RemoveFromList( T o, T next )
        {
            Debug.Assert( o != null && GetNext( o ) == next );
            if( next == null )
            {
                Debug.Assert( _last == o );
                _last = o.Prev;
            }
            else next.Prev = o.Prev;
            o.Prev = null;
        }

        private bool DoChangeObjectMode( T o, IKeyboardMode newMode )
        {
            // First, we need to insert the newMode at the right place, to keep the list sorted.
            T c = _last, next = null;
            do
            {
                int cmp = c.Mode.CompareTo( newMode );
                if( cmp == 0 ) return false; // The newMode already exists.
                if( cmp < 0 ) break; // The current mode is "smaller" than the new mode, so the new mode must be inserted after "c", but before "next".
                next = c;
                c = c.Prev;
            }
            while( c != null );

            RemoveFromList( o, GetNext( o ) ); // Removes "o" from the list so that we can update its Mode property and moves it to the previously computed position.
            o.Mode = newMode;
            InsertBefore( o, next );
            return true;
        }

        internal void SwapModes( T first, T second )
        {
            //We know that the two ILayoutKeyModes exist and have a valid Mode linked to them.
            Debug.Assert( first != second, "Swapping an object with itself" );
            Debug.Assert( first != null && second != null, "Swapping with a null" );

            //Make sure that first is "bigger" than second
            int cmp = second.Mode.CompareTo( first.Mode );
            if( cmp < 0 ) // if first is after second in the linked chain
            {
                T temp = first;
                first = second;
                second = temp;
            }
            Debug.Assert( second.Mode.CompareTo( first.Mode ) > 0 ); //now, we know that first is before second in the linked chain

            IKeyboardMode originalFirstMode = first.Mode;
            IKeyboardMode originalSecondMode = second.Mode;

            if( second.Prev == first ) //if the two keyModes are next to each other in the linked chain 
            {
                second.Prev = first.Prev;
                T secondNext = GetNext( second );
                if(secondNext != null) secondNext.Prev = first;
                first.Prev = second;
            }
            else //else, there is at least one keyMode between the two that we want to swap.
            {                
                T firstPrev = first.Prev;
                T firstNext = GetNext( first );
                T secondPrev = second.Prev;
                T secondNext = GetNext( second );                

                second.Prev = firstPrev;                
                if( secondNext != null ) secondNext.Prev = first;                
                if( firstNext != null ) firstNext.Prev = second;
                first.Prev = secondPrev;
            }

            //Swap Modes
            first.Mode = originalSecondMode;
            second.Mode = originalFirstMode; 

            if( first == _first ) //if first was the first of the chain
                _first = second;

            if( second == _last ) //if second was the last of the chain
                _last = first;

            Debug.Assert( _first.Mode.ToString() == String.Empty );

            OnModeChanged( first, originalFirstMode );
            OnModeChanged( second, originalSecondMode );
        }

        internal bool ChangeObjectMode( T o, IKeyboardMode newMode )
        {
            Debug.Assert( !o.Mode.IsEmpty );
            Debug.Assert( Contains( o ), "It is one of our object." );
            Debug.Assert( o.Mode != newMode, "We do not call this method for nothing." );            

            if( !GetAvailableMode().ContainsAll( newMode ) )
                throw new CKException( R.ModeObjectChangeUnavailableMode, typeof( TI ).Name, newMode.Remove( GetAvailableMode() ) );

            IKeyboardMode prevMode = o.Mode;

            if( DoChangeObjectMode( o, newMode ) )
            {
                if( _current != o )
                {
                    // If we are not changing the mode of the current object,
                    // we can simply check if the new mode would lead to a 
                    // better current (just like when creating a new object).
                    CheckBestCurrent( o );
                }
                else
                {
                    // We changed the mode of the current object.
                    // We simply recompute the best object among existing ones.
                    _current = FindBest( _last, newMode );
                    if( o != _current ) OnCurrentChanged();
                }
                OnModeChanged( o, prevMode );
            }
            return true;
        }

        internal void OnAvailableModeRemoved( IReadOnlyList<IKeyboardMode> modes )
        {
            Debug.Assert( modes.Count > 0, "Not called if useless." );

            // The idea here is to remove any objects that are bound to disappearing mode
            // but to keep the maximum of information: the object (without the disappearing modes)
            // is preserved if (and only if) an object with the final modes does not already exist.
            
            // We process the disappearing modes from the "strongest" mode ('Ctrl') to the 
            // weakest ('Shift'): by doing this, we follow (and maintain) the fall back rules.
            var toProcess = new List<T>();
            
            // We keep the set of objects for which we changed the mode.
            Dictionary<T,IKeyboardMode> changedModes = new Dictionary<T,IKeyboardMode>();
            foreach( IKeyboardMode m in modes )
            {
                Debug.Assert( m.IsAtomic, "The parameter modes list contains only atomic modes." );
                Debug.Assert( GetCurrentMode().ContainsOne( m ) == false, 
                    "We first changed the CurrentMode: the CurrentMode does not contain any of the removed modes." );
                
                // We store in toProcess all the actual keys that have the atomic mode m in their mode.
                toProcess.Clear();
                T c = _last;
                while( c != _first )
                {
                    if( c.Mode.ContainsOne( m ) )
                    {
                        Debug.Assert( c != _current,
                            "We first changed the CurrentMode: an object that use this atomic mode can not be current." );
                        toProcess.Add( c );
                    }
                    c = c.Prev;
                }
                foreach( T o in toProcess )
                {
                    IKeyboardMode prevMode = o.Mode;
                    IKeyboardMode newMode = prevMode.Remove( m );
                    if( newMode.IsEmpty || !DoChangeObjectMode( o, newMode ) )
                    {
                        // If the new mode is already associated to an object,
                        // we destroy this object.
                        Destroy( o );
                        changedModes.Remove( o );
                    }
                    else 
                    {
                        if( !changedModes.ContainsKey( o ) ) changedModes.Add( o, prevMode );
                    }
                }
            }
            if( changedModes.Count > 0 )
            {
                T newCurrent = FindBest( _last, GetCurrentMode() );
                if( _current != newCurrent )
                {
                    _current = newCurrent;
                    OnCurrentChanged();
                }
                foreach( var e in changedModes )
                {
                    OnModeChanged( e.Key, e.Value );
                }
            }
        }

        internal void OnCurrentModeChanged()
        {
            IKeyboardMode currentMode = GetCurrentMode();
            if( _current.Mode != currentMode ) 
            {
                T newCurrent = FindBest( _last, currentMode );
                if( _current != newCurrent )
                {
                    _current = newCurrent;
                    OnCurrentChanged();
                }
            }
        }

        

        public abstract bool Contains( object item );

        protected abstract T DoCreate( IKeyboardMode mode );

        protected abstract void OnCreated( T o );

        protected abstract void OnDestroyed( T o );

        protected abstract IKeyboardMode GetCurrentMode();

        protected abstract IKeyboardMode GetAvailableMode();

        protected abstract void OnCurrentChanged();

        protected abstract void OnModeChanged( T o, IKeyboardMode prevMode );

        #region Collections Objects & IReadOnlyCollection<TI> implementations

        /// <summary>
        /// Enumerator implementation with an exposed TOut type.
        /// </summary>
        sealed class E<TModeObjectRoot, TIn, TOut> : IEnumerator<TOut> 
            where TModeObjectRoot : class, IModeDependantObjectImpl<TModeObjectRoot>, TIn, TOut
        {
            ModeObjectRoot<TModeObjectRoot,TIn> _list;
            TModeObjectRoot _current;

            public E( ModeObjectRoot<TModeObjectRoot,TIn> l )
            {
                _list = l;
            }

            public TOut Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if( _current == null )
                {
                    if( _list == null ) return false;
                    _current = _list.Last;
                }
                else 
                {
                    _current = _current.Prev;
                    if( _current == null )
                    {
                        _list = null;
                        return false;
                    }
                }
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Enumerable implementation.
        /// </summary>
        sealed class EO : IReadOnlyCollection<T>
        {
            ModeObjectRoot<T,TI> _list;

            public EO( ModeObjectRoot<T, TI> l )
            {
                _list = l;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new E<T, TI, T>( _list );
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool Contains( object item )
            {
                return _list.Contains( item );
            }

            public int Count
            {
                get { return _list._count; }
            }
        }

        #region IReadOnlyCollection<TI>

        public int Count
        {
            get { return _count; }
        }

        public IEnumerator<TI> GetEnumerator()
        {
            return new E<T,TI,TI>( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion

        #endregion



    }

}
