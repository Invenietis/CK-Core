#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\CKTrait.cs) is part of CiviKey. 
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
    /// combined ("Alt|Ctrl", "Alt|Ctrl|Home"). The only way to obtain a CKTrait is to call <see cref="CKTraitContext.FindOrCreate(string)"/> (from 
    /// a string) or to use one of the available combination methods (<see cref="Union"/>, <see cref="Except"/>, <see cref="SymmetricExcept"/> or <see cref="Intersect"/> ).
    /// </summary>
    public sealed class CKTrait : IComparable<CKTrait>
    {
        readonly CKTraitContext _context;
        readonly string _trait;
        readonly ICKReadOnlyList<CKTrait> _traits;

        /// <summary>
        /// Initializes the new empty trait of a Context.
        /// </summary>
        internal CKTrait( CKTraitContext ctx )
        {
            Debug.Assert( ctx.EmptyTrait == null, "There is only one empty trait per context." );
            _context = ctx;
            _trait = String.Empty;
            _traits = CKReadOnlyListEmpty<CKTrait>.Empty;
        }

        /// <summary>
        /// Initializes a new atomic trait.
        /// </summary>
        internal CKTrait( CKTraitContext ctx, string atomicTrait )
        {
            Debug.Assert( atomicTrait.Contains( ctx.Separator ) == false );
            _context = ctx;
            _trait = atomicTrait;
            _traits = new CKReadOnlyListMono<CKTrait>( this );
        }

        /// <summary>
        /// Initializes a new combined trait.
        /// </summary>
        internal CKTrait( CKTraitContext ctx, string combinedTrait, ICKReadOnlyList<CKTrait> traits )
        {
            Debug.Assert( combinedTrait.IndexOf( ctx.Separator ) > 0 && traits.Count > 1, "There is more than one trait in a Combined Trait." );
            Debug.Assert( traits.All( m => m.IsAtomic ), "Provided traits are all atomic." );
            Debug.Assert( traits.GroupBy( m => m ).Where( g => g.Count() != 1 ).Count() == 0, "No duplicate in atomic in traits." );
            _context = ctx;
            _trait = combinedTrait;
            _traits = traits;
        }

        /// <summary>
        /// Gets the <see cref="CKTraitContext"/> to which this trait belongs. 
        /// </summary>
        public CKTraitContext Context
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
        public ICKReadOnlyList<CKTrait> AtomicTraits
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
        /// For atomic traits (and the empty trait itself), <see cref="Fallbacks"/> contains only the <see cref="CKTraitContext.EmptyTrait"/>.
        /// </remarks>
        public bool IsAtomic
        {
            get { return _traits.Count < 2; }
        }

        /// <summary>
        /// Compares this trait with another one.
        /// </summary>
        /// <param name="other">The trait to compare to.</param>
        /// <returns>A negative, zero or positive value.</returns>
        public int CompareTo( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( ReferenceEquals( this, other ) ) return 0;
            if( _context != other._context ) return _context.CompareTo( other._context );
            int cmp = _traits.Count - other.AtomicTraits.Count;
            if( cmp == 0 ) cmp = StringComparer.Ordinal.Compare( other._trait, _trait );
            return cmp;
        }

        /// <summary>
        /// Checks if each and every atomic traits of <paramref name="other" /> exists in this trait.
        /// </summary>
        /// <param name="other">The trait(s) to find.</param>
        /// <returns>True if all the specified traits appear in this trait.</returns>
        /// <remarks>
        /// Note that <see cref="CKTraitContext.EmptyTrait"/> is contained (in the sense of this IsSupersetOf method) by definition in any trait 
        /// (including itself): this is the opposite of the <see cref="Overlaps"/> method.
        /// </remarks>
        public bool IsSupersetOf( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( R.TraitsMustBelongToTheSameContext );
            if( _traits.Count < other._traits.Count ) return false;
            bool foundAlien = false;
            Process( this, other,
                null,
                delegate( CKTrait m ) { foundAlien = true; return false; },
                null );
            return !foundAlien;
        }

        /// <summary>
        /// Checks if one of the atomic traits of <paramref name="other" /> exists in this trait.
        /// </summary>
        /// <param name="other">The trait to find.</param>
        /// <returns>Returns true if one of the specified traits appears in this trait.</returns>
        /// <remarks>
        /// When true, this ensures that <see cref="Intersect"/>( <paramref name="other"/> ) != <see cref="CKTraitContext.EmptyTrait"/>. 
        /// The empty trait is not contained (in the sense of this ContainsOne method) in any trait (including itself). This is the opposite
        /// of the <see cref="IsSupersetOf"/> method.
        /// </remarks>
        public bool Overlaps( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( R.TraitsMustBelongToTheSameContext );
            bool found = false;
            Process( this, other,
                null,
                null,
                delegate( CKTrait m ) { found = true; return false; } );
            return found;
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> that contains the atomic traits from both this trait and <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Trait that must be kept.</param>
        /// <returns>The resulting trait.</returns>
        public CKTrait Intersect( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( R.TraitsMustBelongToTheSameContext );
            List<CKTrait> m = new List<CKTrait>();
            Process( this, other, null, null, Util.AlwaysTrue<CKTrait>( m.Add ) );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> that combines this one and 
        /// the trait(s) specified by the parameter. 
        /// </summary>
        /// <param name="other">Trait to add.</param>
        /// <returns>The resulting trait.</returns>
        public CKTrait Union( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( R.TraitsMustBelongToTheSameContext );
            List<CKTrait> m = new List<CKTrait>();
            var add = Util.AlwaysTrue<CKTrait>( m.Add );
            Process( this, other, add, add, add );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> from which trait(s) specified by the parameter are removed.
        /// </summary>
        /// <param name="other">Trait to remove.</param>
        /// <returns>The resulting trait.</returns>
        public CKTrait Except( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( R.TraitsMustBelongToTheSameContext );
            List<CKTrait> m = new List<CKTrait>();
            Process( this, other, Util.AlwaysTrue<CKTrait>( m.Add ), null, null );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> where the atomic traits of <paramref name="other" /> are removed (resp. added) depending 
        /// on whether they exist (resp. do not exist) in this trait. This is like an Exclusive Or (XOR).
        /// </summary>
        /// <param name="other">Trait to toggle.</param>
        /// <returns>The resulting trait.</returns>
        public CKTrait SymmetricExcept( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( R.TraitsMustBelongToTheSameContext );
            List<CKTrait> m = new List<CKTrait>();
            var add = Util.AlwaysTrue<CKTrait>( m.Add );
            Process( this, other, add, add, null );
            return _context.FindOrCreate( m );
        }

        /// <summary>
        /// Applies the given <see cref="SetOperation"/>.
        /// </summary>
        /// <param name="other">Trait to combine.</param>
        /// <param name="operation">Set operation.</param>
        /// <returns>Resulting trait.</returns>
        public CKTrait Apply( CKTrait other, SetOperation operation )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( operation == SetOperation.Union ) return Union( other );
            else if( operation == SetOperation.Except ) return Except( other );
            else if( operation == SetOperation.Intersect ) return Intersect( other );
            else if( operation == SetOperation.SymetricExcept ) return SymmetricExcept( other );
            else if( operation == SetOperation.None ) return this;
            Debug.Assert( operation == SetOperation.Replace, "All operations are covered." );
            return other;
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
        ///             0, 0, 0 =  -- 'Empty'
        /// Intersect   0, 0, 1 = Intersect (keep commons) => /Toggle
        ///             0, 1, 0 =  -- 'Cleanup' (keep theirs only) => /Remove 
        ///             0, 1, 1 =  -- 'Other' (keep theirs and commons, reject mine) => /This
        /// Except      1, 0, 0 = Remove (keep mine only) => /Cleanup
        ///             1, 0, 1 =  -- 'This' (keep mine and commons and reject theirs) => /Other
        /// Toggle      1, 1, 0 = Toggle (keep mine, theirs, but reject commons) => /Intersect
        /// Union       1, 1, 1 = Add
        /// 
        /// This shows that our 4 methods Intersect, Remove, Toggle and Add cover the interesting cases - others are either symetric or useless.
        /// </remarks>
        static void Process( CKTrait left, CKTrait right, Predicate<CKTrait> onLeft, Predicate<CKTrait> onRight, Predicate<CKTrait> onBoth )
        {
            ICKReadOnlyList<CKTrait> l = left.AtomicTraits;
            int cL = l.Count;
            int iL = 0;
            ICKReadOnlyList<CKTrait> r = right.AtomicTraits;
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
                CKTrait eL = l[iL];
                CKTrait eR = r[iR];
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
                    Debug.Assert( cmp != 0, "Since they are not the same." );
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
        /// Gets the number of <see cref="Fallbacks"/>. It is 2^<see cref="AtomicTraits"/>.<see cref="IReadOnlyCollection{T}.Count"/> - 1 since this
        /// trait itself does not appear in the fallbacks, but it is always 1 for atomic and the empty trait (the empty trait always ends the list).
        /// </summary>
        public int FallbacksCount
        {
            get { return _traits.Count > 1 ? ( 1 << _traits.Count ) - 1 : 1; }
        }

        /// <summary>
        /// Gets an enumeration of fallbacks to consider for this trait ordered from best to worst.
        /// This trait does not start the list but the <see cref="CKTraitContext.EmptyTrait"/> always ends this list.
        /// </summary>
        /// <remarks>
        /// For atomic traits (and the empty trait itself), <see cref="Fallbacks"/> contains only the <see cref="CKTraitContext.EmptyTrait"/>.
        /// </remarks>
        public IEnumerable<CKTrait> Fallbacks
        {
            get
            {
                if( _traits.Count <= 1 ) return _context.EnumWithEmpty;
                return ComputeFallbacks();
            }
        }

        IEnumerable<CKTrait> ComputeFallbacks()
        {
            int _currentLength = _traits.Count - 1;
            Debug.Assert( _currentLength >= 1, "Empty and atomic traits are handled explicitely (EnumWithEmpty)." );
            if( _currentLength > 1 )
            {
                int nbTraits = _traits.Count;
                bool[] kept = new bool[nbTraits];
                CKTrait[] v = new CKTrait[_currentLength];
                do
                {
                    int i = nbTraits;
                    while( --i >= _currentLength ) kept[i] = false;
                    int kMax = i;
                    while( i >= 0 ) kept[i--] = true;
                    do
                    {
                        i = 0;
                        for( int j = 0; j < nbTraits; ++j )
                        {
                            if( kept[j] ) v[i++] = _traits[j];
                        }
                        Debug.Assert( i == _currentLength, "We kept the right number of traits." );
                        yield return _context.FindOrCreate( v, i );
                    }
                    while( Forward( kept, ref kMax ) );
                }
                while( --_currentLength > 1 );
            }
            // Special processing for currentLength = 1 (optimization)
            foreach( CKTrait m in _traits ) yield return m;
            yield return _context.EmptyTrait;
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
