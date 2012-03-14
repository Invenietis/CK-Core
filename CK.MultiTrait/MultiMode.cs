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
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// A mode is an immutable object (thread-safe), associated to a unique string inside a <see cref="Context"/>, that can be atomic ("Alt", "Home", "Ctrl") or 
    /// combined ("Alt+Ctrl", "Alt+Ctrl+Home"). The only way to obtain a MultiMode is to call <see cref="MultiModeContext.ObtainMode"/> (from 
    /// a string) or to use one of the available combination methods (<see cref="Add"/>, <see cref="Remove"/>, <see cref="Toggle"/> or <see cref="Intersect"/> ).
    /// </summary>
    public sealed class MultiMode : IComparable<MultiMode>
    {
        readonly MultiModeContext _context;
        readonly string _mode;
        readonly IReadOnlyList<MultiMode> _modes;
        IReadOnlyList<MultiMode> _fallbacks;

        /// <summary>
        /// Initializes the new empty mode of a Context.
        /// </summary>
        internal MultiMode( MultiModeContext ctx )
        {
            Debug.Assert( ctx.EmptyMode == null, "There is only one empty mode per context." );
            _context = ctx;
            _mode = String.Empty;
            _modes = ReadOnlyListEmpty<MultiMode>.Empty;
            _fallbacks = new ReadOnlyListMono<MultiMode>( this );
        }

        /// <summary>
        /// Initializes a new atomic mode.
        /// </summary>
        internal MultiMode( MultiModeContext ctx, string atomicMode )
        {
            _context = ctx;
            _mode = atomicMode;
            _modes = new ReadOnlyListMono<MultiMode>( this );
            _fallbacks = ctx.EmptyMode.Fallbacks;
        }

        /// <summary>
        /// Initializes a new combined mode.
        /// </summary>
        internal MultiMode( MultiModeContext ctx, string combinedMode, IReadOnlyList<MultiMode> modes )
        {
            Debug.Assert( combinedMode.IndexOf( '+' ) > 0 && modes.Count > 1, "There is more than one mode in a Combined Mode." );
            Debug.Assert( modes.All( m => m.IsAtomic ), "Provided modes are all atomic." );
            Debug.Assert( modes.GroupBy( m => m ).Where( g => g.Count() != 1 ).Count() == 0, "No duplicate in atomic in modes." );
            _context = ctx;
            _mode = combinedMode;
            _modes = modes;
        }

        /// <summary>
        /// Gets the <see cref="MultiModeContext"/> to which this mode belongs. 
        /// </summary>
        public MultiModeContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets the multi modes in an ordered manner separated by +.
        /// </summary>
        /// <returns>This multi mode as a string.</returns>
        public override string ToString()
        {
            return _mode;
        }

        /// <summary>
        /// Gets the atomic modes that this mode contains.
        /// </summary>
        public IReadOnlyList<MultiMode> AtomicModes
        {
            get { return _modes; }
        }

        /// <summary>
        /// Gets a boolean indicating whether this mode is the empty mode (<see cref="AtomicModes"/> is empty
        /// and <see cref="Fallbacks"/> contains only itself).
        /// </summary>
        public bool IsEmpty
        {
            get { return _mode == String.Empty; }
        }

        /// <summary>
        /// Gets a boolean indicating whether this mode contains zero 
        /// (the empty mode is considered as an atomic mode) or only one atomic mode.
        /// </summary>
        /// <remarks>
        /// For atomic modes (and the empty mode itself), <see cref="Fallbacks"/> contains only the <see cref="MultiModeContext.EmptyMode"/>.
        /// </remarks>
        public bool IsAtomic
        {
            get { return _modes.Count < 2; }
        }

        /// <summary>
        /// Compares this mode with another one.
        /// </summary>
        /// <param name="other">The mode to compare to.</param>
        /// <returns>A negative, zero or positive value.</returns>
        public int CompareTo( MultiMode other )
        {
            if( ReferenceEquals( this, other ) ) return 0;
            int cmp = _modes.Count - other.AtomicModes.Count;
            if( cmp == 0 ) cmp = StringComparer.Ordinal.Compare( other.ToString(), _mode );
            return cmp;
        }

        /// <summary>
        /// Checks if each and every atomic modes of <paramref name="mode" /> exists in this mode.
        /// </summary>
        /// <param name="mode">The mode(s) to find.</param>
        /// <returns>True if all the specified modes appear in this mode.</returns>
        /// <remarks>
        /// Note that <see cref="MultiModeContext.EmptyMode"/> is contained (in the sense of this ContainsAll method) by definition in any mode 
        /// (including itself): this is the opposite of the <see cref="ContainsOne"/> method.
        /// </remarks>
        public bool ContainsAll( MultiMode mode )
        {
            bool foundAlien = false;
            Process( this, mode,
                null,
                delegate( MultiMode m ) { foundAlien = true; return false; },
                null );
            return !foundAlien;
        }

        /// <summary>
        /// Checks if one of the atomic modes of <paramref name="mode" /> exists in this mode.
        /// </summary>
        /// <param name="mode">The mode(s) to find.</param>
        /// <returns>Returns true if one of the specified modes appears in this mode.</returns>
        /// <remarks>
        /// When true, this ensures that <see cref="Intersect"/>( <paramref name="mode"/> ) != <see cref="IKeyboardContextMode.EmptyMode"/>. 
        /// The empty mode is not contained (in the sense of this ContainsOne method) in any mode (including itself). This is the opposite
        /// of the <see cref="ContainsAll"/> method.
        /// </remarks>
        public bool ContainsOne( MultiMode mode )
        {
            bool found = false;
            Process( this, mode,
                null,
                null,
                delegate( MultiMode m ) { found = true; return false; } );
            return found;
        }

        /// <summary>
        /// Obtains a <see cref="MultiMode"/> that contains the atomic modes from boyh this mode and <paramref name="mode"/>.
        /// </summary>
        /// <param name="mode">Mode(s) that must be kept.</param>
        /// <returns>The resulting mode.</returns>
        public MultiMode Intersect( MultiMode mode )
        {
            List<MultiMode> m = new List<MultiMode>();
            Process( this, mode, null, null, Adapter.AlwaysTrue<MultiMode>( m.Add ) );
            return _context.ObtainMode( m );
        }

        /// <summary>
        /// Obtains a <see cref="MultiMode"/> that combines this one and 
        /// the mode(s) specified by the parameter. 
        /// </summary>
        /// <param name="mode">Mode(s) to add.</param>
        /// <returns>The resulting mode.</returns>
        public MultiMode Add( MultiMode mode )
        {
            List<MultiMode> m = new List<MultiMode>();
            var add = Adapter.AlwaysTrue<MultiMode>( m.Add );
            Process( this, mode, add, add, add );
            return _context.ObtainMode( m );
        }

        /// <summary>
        /// Obtains a <see cref="MultiMode"/> from which mode(s) specified by the parameter are removed. 
        /// </summary>
        /// <param name="mode">Mode(s) to remove.</param>
        /// <returns>The resulting mode.</returns>
        public MultiMode Remove( MultiMode mode )
        {
            List<MultiMode> m = new List<MultiMode>();
            Process( this, mode, Adapter.AlwaysTrue<MultiMode>( m.Add ), null, null );
            return _context.ObtainMode( m );
        }

        /// <summary>
        /// Obtains a <see cref="MultiMode"/> where the atomic modes of <paramref name="mode" /> are removed (resp. added) depending 
        /// on whether they exist (resp. do not exist) in this mode. 
        /// </summary>
        /// <param name="mode">Mode(s) to toggle.</param>
        /// <returns>The resulting mode.</returns>
        public MultiMode Toggle( MultiMode mode )
        {
            List<MultiMode> m = new List<MultiMode>();
            var add = Adapter.AlwaysTrue<MultiMode>( m.Add );
            Process( this, mode, add, add, null );
            return _context.ObtainMode( m );
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
        static void Process( MultiMode left, MultiMode right, Predicate<MultiMode> onLeft, Predicate<MultiMode> onRight, Predicate<MultiMode> onBoth )
        {
            IReadOnlyList<MultiMode> l = left.AtomicModes;
            int cL = l.Count;
            int iL = 0;
            IReadOnlyList<MultiMode> r = right.AtomicModes;
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
                MultiMode eL = l[iL];
                MultiMode eR = r[iR];
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

        /// <summary>
        /// Gets the list of fallbacks to consider for this mode ordered from best to worst.
        /// The <see cref="MultiModeContext.EmptyMode"/> always ends this list.
        /// </summary>
        /// <remarks>
        /// For atomic modes (and the empty mode itself), <see cref="Fallbacks"/> contains only the <see cref="MultiModeContext.EmptyMode"/>.
        /// </remarks>
        public IReadOnlyList<MultiMode> Fallbacks
        {
            get
            {
                if( _fallbacks == null )
                {
                    MultiMode[] f = new MultiMode[(1 << _modes.Count) - 1];
                    ComputeFallbacks( f );
                    Interlocked.Exchange( ref _fallbacks, new ReadOnlyListOnIList<MultiMode>( f ) );
                }
                return _fallbacks;
            }
        }

        void ComputeFallbacks( MultiMode[] f )
        {
            int iAdd = 0;
            int currentLength = _modes.Count - 1;
            Debug.Assert( currentLength >= 1, "Atomic modes are up to date (by ctor)." );
            if( currentLength > 1 )
            {
                int nbModes = _modes.Count;
                bool[] kept = new bool[nbModes];
                MultiMode[] v = new MultiMode[currentLength];
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
                        f[iAdd++] = _context.ObtainMode( v, i );
                    }
                    while( Forward( kept, ref kMax ) );
                }
                while( --currentLength > 1 );
            }
            // Special processing for currentLength = 1 (optimization)
            foreach( MultiMode m in _modes ) f[iAdd++] = m;
            f[iAdd++] = _context.EmptyMode;
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
