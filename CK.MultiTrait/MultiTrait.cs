#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.MultiTrait\MultiTrait.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CK.Core;
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// A trait is an immutable object (thread-safe), associated to a unique string inside a <see cref="Context"/>, that can be atomic ("Alt", "Home", "Ctrl") or 
    /// combined ("Alt+Ctrl", "Alt+Ctrl+Home"). The only way to obtain a MultiTrait is to call <see cref="MultiTraitContext.FindOrCreate"/> (from 
    /// a string) or to use one of the available combination methods (<see cref="Add"/>, <see cref="Remove"/>, <see cref="Toggle"/> or <see cref="Intersect"/> ).
    /// </summary>
    public sealed class MultiTrait : IComparable<MultiTrait>
    {
        readonly MultiTraitContext _context;
        readonly string _trait;
        readonly IReadOnlyList<MultiTrait> _traits;
        IReadOnlyList<MultiTrait> _fallbacks;

        /// <summary>
        /// Initializes the new empty trait of a Context.
        /// </summary>
        internal MultiTrait( MultiTraitContext ctx )
        {
            Debug.Assert( ctx.EmptyTrait == null, "There is only one empty trait per context." );
            _context = ctx;
            _trait = String.Empty;
            _traits = ReadOnlyListEmpty<MultiTrait>.Empty;
            _fallbacks = new ReadOnlyListMono<MultiTrait>( this );
        }

        /// <summary>
        /// Initializes a new atomic trait.
        /// </summary>
        internal MultiTrait( MultiTraitContext ctx, string atomicTrait )
        {
            Debug.Assert( atomicTrait.Contains( '+' ) == false );
            _context = ctx;
            _trait = atomicTrait;
            _traits = new ReadOnlyListMono<MultiTrait>( this );
            _fallbacks = ctx.EmptyTrait.Fallbacks;
        }

        /// <summary>
        /// Initializes a new combined trait.
        /// </summary>
        internal MultiTrait( MultiTraitContext ctx, string combinedTrait, IReadOnlyList<MultiTrait> traits )
        {
            Debug.Assert( combinedTrait.IndexOf( '+' ) > 0 && traits.Count > 1, "There is more than one trait in a Combined Trait." );
            Debug.Assert( traits.All( m => m.IsAtomic ), "Provided traits are all atomic." );
            Debug.Assert( traits.GroupBy( m => m ).Where( g => g.Count() != 1 ).Count() == 0, "No duplicate in atomic in traits." );
            _context = ctx;
            _trait = combinedTrait;
            _traits = traits;
        }

        /// <summary>
        /// Gets the <see cref="MultiTraitContext"/> to which this trait belongs. 
        /// </summary>
        public MultiTraitContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets the multi traits in an ordered manner separated by +.
        /// </summary>
        /// <returns>This multi trait as a string.</returns>
        public override string ToString()
        {
            return _trait;
        }

        /// <summary>
        /// Gets the atomic traits that this trait contains.
        /// </summary>
        public IReadOnlyList<MultiTrait> AtomicTraits
        {
            get { return _traits; }
        }

        /// <summary>
        /// Gets a boolean indicating whether this trait is the empty trait (<see cref="AtomicTraits"/> is empty
        /// and <see cref="Fallbacks"/> contains only itself).
        /// </summary>
        public bool IsEmpty
        {
            get { return _trait == String.Empty; }
        }

        /// <summary>
        /// Gets a boolean indicating whether this trait contains zero 
        /// (the empty trait is considered as an atomic trait) or only one atomic trait.
        /// </summary>
        /// <remarks>
        /// For atomic traits (and the empty trait itself), <see cref="Fallbacks"/> contains only the <see cref="MultiTraitContext.EmptyTrait"/>.
        /// </remarks>
        public bool IsAtomic
        {
            get { return _traits.Count < 2; }
        }

        /// <summary>
        /// Finds the element whose traits obtained with a <paramref name="selector"/>
        /// is the closest one from this.
        /// </summary>
        /// <typeparam name="T">Type of element.</typeparam>
        /// <param name="elements">Set of elements.</param>
        /// <param name="selector">Function that extracts a trait from an element.</param>
        /// <returns>
        /// The closest element or null if no best trait can be found. If one of the trait is <see cref="String.Empty"/>, 
        /// the corresponding element is guaranted to be returned.
        /// </returns>
        public T Closest<T>( IEnumerable<T> elements, Func<T, string> selector )
        {
            T best = default( T );
            int dist = Int32.MaxValue;
            foreach( var e in elements )
            {
                string trait = selector( e );
                if( trait != null )
                {
                    if( trait.Length == 0 )
                    {
                        if( dist == Int32.MaxValue )
                        {
                            best = e;
                            dist = Int32.MaxValue - 1;
                        }
                    }
                    else
                    {
                        MultiTrait mE = _context.FindOnlyExisting( selector( e ) );
                        if( mE != null )
                        {
                            if( ReferenceEquals( this, mE ) )
                            {
                                best = e;
                                break;
                            }
                            int eDist = Fallbacks.IndexOf( mE );
                            if( eDist >= 0 && eDist < dist )
                            {
                                best = e;
                                dist = eDist;
                            }
                        }
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// Compares this trait with another one.
        /// </summary>
        /// <param name="other">The trait to compare to.</param>
        /// <returns>A negative, zero or positive value.</returns>
        public int CompareTo( MultiTrait other )
        {
            if( ReferenceEquals( this, other ) ) return 0;
            int cmp = _traits.Count - other.AtomicTraits.Count;
            if( cmp == 0 ) cmp = StringComparer.Ordinal.Compare( other.ToString(), _trait );
            return cmp;
        }

        /// <summary>
        /// Checks if each and every atomic traits of <paramref name="trait" /> exists in this trait.
        /// </summary>
        /// <param name="trait">The trait(s) to find.</param>
        /// <returns>True if all the specified traits appear in this trait.</returns>
        /// <remarks>
        /// Note that <see cref="MultiTraitContext.EmptyTrait"/> is contained (in the sense of this ContainsAll method) by definition in any trait 
        /// (including itself): this is the opposite of the <see cref="ContainsOne"/> method.
        /// </remarks>
        public bool ContainsAll( MultiTrait trait )
        {
            bool foundAlien = false;
            Process( this, trait,
                null,
                delegate( MultiTrait m ) { foundAlien = true; return false; },
                null );
            return !foundAlien;
        }

        /// <summary>
        /// Checks if one of the atomic traits of <paramref name="trait" /> exists in this trait.
        /// </summary>
        /// <param name="trait">The trait(s) to find.</param>
        /// <returns>Returns true if one of the specified traits appears in this trait.</returns>
        /// <remarks>
        /// When true, this ensures that <see cref="Intersect"/>( <paramref name="trait"/> ) != <see cref="IKeyboardContextTrait.EmptyTrait"/>. 
        /// The empty trait is not contained (in the sense of this ContainsOne method) in any trait (including itself). This is the opposite
        /// of the <see cref="ContainsAll"/> method.
        /// </remarks>
        public bool ContainsOne( MultiTrait trait )
        {
            bool found = false;
            Process( this, trait,
                null,
                null,
                delegate( MultiTrait m ) { found = true; return false; } );
            return found;
        }

        /// <summary>
        /// Obtains a <see cref="MultiTrait"/> that contains the atomic traits from boyh this trait and <paramref name="trait"/>.
        /// </summary>
        /// <param name="trait">Trait(s) that must be kept.</param>
        /// <returns>The resulting trait.</returns>
        public MultiTrait Intersect( MultiTrait trait )
        {
            List<MultiTrait> m = new List<MultiTrait>();
            Process( this, trait, null, null, Adapter.AlwaysTrue<MultiTrait>( m.Add ) );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Obtains a <see cref="MultiTrait"/> that combines this one and 
        /// the trait(s) specified by the parameter. 
        /// </summary>
        /// <param name="trait">Trait(s) to add.</param>
        /// <returns>The resulting trait.</returns>
        public MultiTrait Add( MultiTrait trait )
        {
            List<MultiTrait> m = new List<MultiTrait>();
            var add = Adapter.AlwaysTrue<MultiTrait>( m.Add );
            Process( this, trait, add, add, add );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Obtains a <see cref="MultiTrait"/> from which trait(s) specified by the parameter are removed. 
        /// </summary>
        /// <param name="trait">Trait(s) to remove.</param>
        /// <returns>The resulting trait.</returns>
        public MultiTrait Remove( MultiTrait trait )
        {
            List<MultiTrait> m = new List<MultiTrait>();
            Process( this, trait, Adapter.AlwaysTrue<MultiTrait>( m.Add ), null, null );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Obtains a <see cref="MultiTrait"/> where the atomic traits of <paramref name="trait" /> are removed (resp. added) depending 
        /// on whether they exist (resp. do not exist) in this trait. 
        /// </summary>
        /// <param name="trait">Trait(s) to toggle.</param>
        /// <returns>The resulting trait.</returns>
        public MultiTrait Toggle( MultiTrait trait )
        {
            List<MultiTrait> m = new List<MultiTrait>();
            var add = Adapter.AlwaysTrue<MultiTrait>( m.Add );
            Process( this, trait, add, add, null );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Common process function where 3 predicates drive the result: each atomic trait is submitted to one of the 3 predicates
        /// depending on whether it is only in the left, only in the right or appears in both traits.
        /// When returning false, a predicate stops the process.
        /// </summary>
        /// <remarks>
        /// When this predicate is 'adding the trait to a list', we can draw the following table where '1' means the predicate exists and '0' means
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
        static void Process( MultiTrait left, MultiTrait right, Predicate<MultiTrait> onLeft, Predicate<MultiTrait> onRight, Predicate<MultiTrait> onBoth )
        {
            IReadOnlyList<MultiTrait> l = left.AtomicTraits;
            int cL = l.Count;
            int iL = 0;
            IReadOnlyList<MultiTrait> r = right.AtomicTraits;
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
                MultiTrait eL = l[iL];
                MultiTrait eR = r[iR];
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
        /// Gets the list of fallbacks to consider for this trait ordered from best to worst.
        /// The <see cref="MultiTraitContext.EmptyTrait"/> always ends this list.
        /// </summary>
        /// <remarks>
        /// For atomic traits (and the empty trait itself), <see cref="Fallbacks"/> contains only the <see cref="MultiTraitContext.EmptyTrait"/>.
        /// </remarks>
        public IReadOnlyList<MultiTrait> Fallbacks
        {
            get
            {
                if( _fallbacks == null )
                {
                    MultiTrait[] f = new MultiTrait[(1 << _traits.Count) - 1];
                    ComputeFallbacks( f );
                    Interlocked.Exchange( ref _fallbacks, new ReadOnlyListOnIList<MultiTrait>( f ) );
                }
                return _fallbacks;
            }
        }

        void ComputeFallbacks( MultiTrait[] f )
        {
            int iAdd = 0;
            int currentLength = _traits.Count - 1;
            Debug.Assert( currentLength >= 1, "Atomic traits are up to date (by ctor)." );
            if( currentLength > 1 )
            {
                int nbTraits = _traits.Count;
                bool[] kept = new bool[nbTraits];
                MultiTrait[] v = new MultiTrait[currentLength];
                do
                {
                    int i = nbTraits;
                    while( --i >= currentLength ) kept[i] = false;
                    int kMax = i;
                    while( i >= 0 ) kept[i--] = true;
                    do
                    {
                        i = 0;
                        for( int j = 0; j < nbTraits; ++j )
                        {
                            if( kept[j] ) v[i++] = _traits[j];
                        }
                        Debug.Assert( i == currentLength, "We kept the right number of traits." );
                        f[iAdd++] = _context.FindOrCreate( v, i );
                    }
                    while( Forward( kept, ref kMax ) );
                }
                while( --currentLength > 1 );
            }
            // Special processing for currentLength = 1 (optimization)
            foreach( MultiTrait m in _traits ) f[iAdd++] = m;
            f[iAdd++] = _context.EmptyTrait;
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
