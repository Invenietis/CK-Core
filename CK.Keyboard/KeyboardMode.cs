#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Context\KeyboardMode.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using CK.Core;
using CK.Keyboard.Model;

namespace CK.Keyboard
{
    /// <summary>
    /// Implements <see cref="IKeyboardMode"/>.
    /// </summary>
    sealed class KeyboardMode : IKeyboardMode
    {
        IKeyboardContext _context;
        string _mode;
        IReadOnlyList<IKeyboardMode> _modes;
        IReadOnlyList<IKeyboardMode> _fallbacks;

        /// <summary>
        /// Initializes the new empty mode of a Context.
        /// </summary>
        internal KeyboardMode( IKeyboardContext ctx )
        {
            Debug.Assert( ctx.EmptyMode == null, "There is only one empty mode per context." );
            _context = ctx;
            _mode = String.Empty;
            _modes = ReadOnlyListEmpty<IKeyboardMode>.Empty;
            _fallbacks = new ReadOnlyListMono<IKeyboardMode>( this );
        }

        /// <summary>
        /// Initializes a new atomic mode.
        /// </summary>
        internal KeyboardMode( IKeyboardContext ctx, string atomicMode )
        {
            _context = ctx;
            _mode = atomicMode;
            _modes = new ReadOnlyListMono<IKeyboardMode>( this );
            _fallbacks = ctx.EmptyMode.Fallbacks;
        }

        /// <summary>
        /// Initializes a new combined mode.
        /// </summary>
        internal KeyboardMode( IKeyboardContext ctx, string combinedMode, IReadOnlyList<IKeyboardMode> modes )
        {
            Debug.Assert( combinedMode.IndexOf( '+' ) > 0 && modes.Count > 1, "There is more than one mode in a Combined Mode." );
            Debug.Assert( modes.All( m => m.IsAtomic ), "Provided modes are all atomic." );
            Debug.Assert( modes.GroupBy( m => m ).Where( g => g.Count() != 1 ).Count() == 0, "No duplicate in atomic in modes." );
            _context = ctx;
            _mode = combinedMode;
            _modes = modes;
        }

        IKeyboardContextMode IKeyboardMode.Context
        {
            get { return _context; }
        }

        internal IKeyboardContext Context
        {
            get { return _context; }
        }
        
        public override string ToString()
        {
            return _mode;
        }
        
        public IReadOnlyList<IKeyboardMode> AtomicModes 
        {
            get { return _modes; } 
        }

        public bool IsEmpty
        {
            get { return _mode == String.Empty; }
        }

        public bool IsAtomic
        {
            get { return _modes.Count < 2; }
        }

        public int CompareTo( IKeyboardMode other )
        {
            if( _modes == other ) return 0;
            int cmp = _modes.Count - other.AtomicModes.Count;
            if( cmp == 0 ) cmp = StringComparer.Ordinal.Compare( other.ToString(), _mode );
            return cmp;
        }

        bool IKeyboardMode.ContainsAll( IKeyboardMode mode )
        {
            bool foundAlien = false;
            Process( this, mode,
                null,
                delegate( IKeyboardMode m ) { foundAlien = true; return false; },
                null );
            return !foundAlien;
        }

        bool IKeyboardMode.ContainsOne( IKeyboardMode mode )
        {
            bool found = false;
            Process( this, mode, 
                null, 
                null,
                delegate( IKeyboardMode m ) { found = true; return false; } );
            return found;
        }

        IKeyboardMode IKeyboardMode.Intersect( IKeyboardMode mode )
        {
            List<IKeyboardMode> m = new List<IKeyboardMode>();
            Process( this, mode, null, null, Adapter.AlwaysTrue<IKeyboardMode>( m.Add ) );
            return Context.ObtainMode( m );
        }
        
        IKeyboardMode IKeyboardMode.Add( IKeyboardMode mode )
        {
            List<IKeyboardMode> m = new List<IKeyboardMode>();
            var add = Adapter.AlwaysTrue<IKeyboardMode>( m.Add );
            Process( this, mode, add, add, add );
            return Context.ObtainMode( m );
        }

       IKeyboardMode IKeyboardMode.Remove( IKeyboardMode mode )
        {
            List<IKeyboardMode> m = new List<IKeyboardMode>();
            Process( this, mode, Adapter.AlwaysTrue<IKeyboardMode>( m.Add ), null, null );
            return Context.ObtainMode( m );
        }

        IKeyboardMode IKeyboardMode.Toggle( IKeyboardMode mode )
        {
            List<IKeyboardMode> m = new List<IKeyboardMode>();
            var add = Adapter.AlwaysTrue<IKeyboardMode>( m.Add );
            Process( this, mode, add, add, null );
            return Context.ObtainMode( m );
        }

        /// <summary>
        /// Common process function where 3 predicates drive the result: each atomic mode is submitted to one of the 3 predicates
        /// depending on whether it is only in the left, only in the right or appears in both modes.
        /// When returning false, a predicate stops the process.
        /// </summary>
        /// <remarks>
        /// When this predicate is 'adding the mode to a list', we can draw the following table where '1' means the predicate exists and '0' means
        /// no predicate (or the 'always true' one):
        /// 
        /// 0, 0, 0 =  -- 'Empty'
        /// 0, 0, 1 = Intersect (keep commons) => /Toggle
        /// 0, 1, 0 =  -- 'Cleanup' (keep theirs only) => /Remove 
        /// 0, 1, 1 =  -- 'Other' (keep theirs and commons, reject mine) => /This
        /// 1, 0, 0 = Remove (keep mine only) => /Cleanup
        /// 1, 0, 1 =  -- 'This' (keep mine and commons and reject theirs) => /Other
        /// 1, 1, 0 = Toggle (keep mine, theirs, but reject commons) => /Intersect
        /// 1, 1, 1 = Add
        /// 
        /// This shows that our 4 methods Intersect, Remove, Toggle and Add cover the interesting cases - others are either symetric or useless.
        /// </remarks>
        static void Process( IKeyboardMode left, IKeyboardMode right, Predicate<IKeyboardMode> onLeft, Predicate<IKeyboardMode> onRight,  Predicate<IKeyboardMode> onBoth )
        {
            IReadOnlyList<IKeyboardMode> l = left.AtomicModes;
            int cL = l.Count;
            int iL = 0;
            IReadOnlyList<IKeyboardMode> r = right.AtomicModes;
            int cR = r.Count;
            int iR = 0;
            for( ; ; )
            {
                if( cL == 0 )
                {
                    while( cR-- > 0 )
                    {
                        if( onRight == null || !onRight( r[iR++] ) ) break;
                    }
                    return;
                }
                if( cR == 0 )
                {
                    while( cL-- > 0 )
                    {
                        if( onLeft == null || !onLeft( l[iL++] ) ) break;
                    }
                    return;
                }
                Debug.Assert( iL >= 0 && iL < l.Count && iR >= 0 && iR < r.Count, "End of lists is handled above." );
                IKeyboardMode eL = l[iL];
                IKeyboardMode eR = r[iR];
                if( eL == eR )
                {
                    if( onBoth != null && !onBoth( eL ) ) break;
                    iL++;
                    cL--;
                    iR++;
                    cR--;
                }
                else
                {
                    int cmp = eL.CompareTo( eR );
                    Debug.Assert( eL.CompareTo( eR ) != 0, "Since they are not the same." );
                    if( cmp > 0 )
                    {
                        if( onLeft != null && !onLeft( eL ) ) break;
                        iL++;
                        cL--;
                    }
                    else
                    {
                        if( onRight != null && !onRight( eR ) ) break;
                        iR++;
                        cR--;
                    }
                }
            }
        }

        public IReadOnlyList<IKeyboardMode> Fallbacks
        {
            get
            {
                if( _fallbacks == null )
                {
                    IKeyboardMode[] f = new IKeyboardMode[ (1 << _modes.Count) - 1 ];
                    ComputeFallbacks( f );
                    _fallbacks = new ReadOnlyListOnIList<IKeyboardMode>( f );
                }
                return _fallbacks;
            }
        }

        public void ComputeFallbacks( IKeyboardMode[] f )
        {
            int iAdd = 0;
            int currentLength = _modes.Count - 1;
            Debug.Assert( currentLength >= 1, "Atomic modes are up to date (by ctor)." );
            if( currentLength > 1 )
            {
                int nbModes = _modes.Count;
                bool[] kept = new bool[nbModes];
                IKeyboardMode[] v = new IKeyboardMode[currentLength];
                do
                {
                    int i = nbModes;
                    while( --i >= currentLength ) kept[i] = false;
                    int kMax = i;
                    while( i >= 0 ) kept[i--] = true;
                    do
                    {
                        i = 0;
                        for( int j = 0; j < nbModes; ++j )
                        {
                            if( kept[j] ) v[i++] = _modes[j];
                        }
                        Debug.Assert( i == currentLength, "We kept the right number of modes." );
                        f[iAdd++] = Context.ObtainMode( v, i );
                    }
                    while( Forward( kept, ref kMax ) );
                }
                while( --currentLength > 1 );
            }
            // Special processing for currentLength = 1 (optimization)
            foreach( IKeyboardMode m in _modes ) f[iAdd++] = m;
            f[iAdd++] = Context.EmptyMode;
            Debug.Assert( iAdd == f.Length, "We completely filled the array." );
        }

        static bool Forward( bool[] kept, ref int kMax )
        {
            Debug.Assert( Array.FindLastIndex( kept, delegate( bool b ) { return b; } ) == kMax, "kMax maintains the last 'true' position." );
            kept[kMax] = false;
            if( ++kMax < kept.Length ) kept[kMax] = true;
            else
            {
                int maxIdx = kept.Length - 1;
                // Skips ending 'true' slots.
                int k = maxIdx - 1;
                while( k >= 0 && kept[k] ) --k;
                if( k < 0 ) return false;
                // Find the next 'true' (skips 'false' slots).
                int head = k;
                while( head >= 0 && !kept[head] ) --head;
                if( head < 0 ) return false;
                // Number of 'true' slots after the head.
                int nb = kept.Length - k;
                kept[head++] = false;
                while( --nb >= 0 ) kept[head++] = true;
                // Resets ending slots to 'false'.
                kMax = head - 1;
                while( head < maxIdx ) kept[head++] = false;
            }
            return true;
        }


    }
}
