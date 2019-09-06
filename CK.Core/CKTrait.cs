using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CK.Core;
using System.Threading;
using System.ComponentModel;

namespace CK.Core
{
    /// <summary>
    /// A trait is an immutable object (thread-safe), associated to a unique string inside a <see cref="Context"/>, that can be atomic ("Alt", "Home", "Ctrl") or 
    /// combined ("Alt|Ctrl", "Alt|Ctrl|Home"). The only way to obtain a CKTrait is to call <see cref="CKTraitContext.FindOrCreate(string)"/> (from 
    /// a string) or to use one of the available combination methods (<see cref="Union"/>, <see cref="Except"/>, <see cref="SymmetricExcept"/> or <see cref="Intersect"/> ).
    /// </summary>
    /// <remarks>
    /// A CKTrait is not serializable: since it is relative to <see cref="CKTraitContext"/>, it must be recreated in the right context. A CKTraitContext is typically
    /// a static object that exists in the origin application domain. A CKTrait must be serialized as its <see cref="ToString"/> representation and it is up to the 
    /// code to call <see cref="CKTraitContext.FindOrCreate(string)"/> on the appropriate context when deserializing it.
    /// </remarks>
    public sealed class CKTrait : IComparable<CKTrait>, IEquatable<CKTrait>
    {
        readonly CKTraitContext _context;
        readonly string _trait;
        readonly IReadOnlyList<CKTrait> _traits;

        /// <summary>
        /// Initializes the new empty trait of a CKTraitContext.
        /// </summary>
        internal CKTrait( CKTraitContext ctx )
        {
            Debug.Assert( ctx.EmptyTrait == null, "There is only one empty trait per context." );
            _context = ctx;
            _trait = String.Empty;
            _traits = Util.Array.Empty<CKTrait>();
        }

        /// <summary>
        /// Initializes a new atomic trait.
        /// </summary>
        internal CKTrait( CKTraitContext ctx, string atomicTrait )
        {
            Debug.Assert( atomicTrait.Contains( ctx.Separator ) == false );
            _context = ctx;
            _trait = atomicTrait;
            _traits = new CKTrait[] { this };
        }

        /// <summary>
        /// Initializes a new combined trait.
        /// </summary>
        internal CKTrait( CKTraitContext ctx, string combinedTrait, IReadOnlyList<CKTrait> traits )
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
        public CKTraitContext Context => _context;

        /// <summary>
        /// Gets the multi traits in an ordered manner separated by +.
        /// </summary>
        /// <returns>This multi trait as a string.</returns>
        public override string ToString() => _trait;

        /// <summary>
        /// Gets the atomic traits that this trait contains.
        /// This list does not contain the empty trait and is sorted according to the name of the atomic traits (lexical order): this is the 
        /// same as the <see cref="ToString"/> representation.
        /// Note that it is in reverse order regarding <see cref="CompareTo"/> ("A" that is stronger than "B" appears before "B").
        /// </summary>
        public IReadOnlyList<CKTrait> AtomicTraits => _traits; 

        /// <summary>
        /// Gets a boolean indicating whether this trait is the empty trait (<see cref="AtomicTraits"/> is empty
        /// and <see cref="Fallbacks"/> contains only itself).
        /// </summary>
        public bool IsEmpty => _trait.Length == 0; 

        /// <summary>
        /// Gets a boolean indicating whether this trait contains zero 
        /// (the empty trait is considered as an atomic trait) or only one atomic trait.
        /// </summary>
        /// <remarks>
        /// For atomic traits (and the empty trait itself), <see cref="Fallbacks"/> contains only the <see cref="CKTraitContext.EmptyTrait"/>.
        /// </remarks>
        public bool IsAtomic => _traits.Count <= 1; 

        /// <summary>
        /// Compares this trait with another one.
        /// The <see cref="Context"/> is the primary key (see <see cref="CKTraitContext.CompareTo"/>), then comes 
        /// the number of traits (more traits is greater) and then comes the string representation of the trait in 
        /// reverse lexical order (<see cref="StringComparer.Ordinal"/>): "A" is greater than "B".
        /// </summary>
        /// <param name="other">The trait to compare to.</param>
        /// <returns>A negative, zero or positive value.</returns>
        public int CompareTo( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( ReferenceEquals( this, other ) ) return 0;
            int cmp = _context.CompareTo( other._context );
            if( cmp == 0 )
            {
                cmp = _traits.Count - other.AtomicTraits.Count;
                if( cmp == 0 ) cmp = StringComparer.Ordinal.Compare( other._trait, _trait );
            }
            return cmp;
        }

        /// <summary>
        /// Checks equality of this trait with another one.
        /// </summary>
        /// <param name="other">The trait to compare to.</param>
        /// <returns>True on equality.</returns>
        public bool Equals( CKTrait other )
        {
            return ReferenceEquals( this, other );
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
            if( !other.Context.Equals( _context ) ) throw new InvalidOperationException( Impl.CoreResources.TraitsMustBelongToTheSameContext );
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
            if( !other.Context.Equals( _context ) ) throw new InvalidOperationException( Impl.CoreResources.TraitsMustBelongToTheSameContext );
            bool found = false;
            Process( this, other,
                null,
                null,
                delegate( CKTrait m ) { found = true; return false; } );
            return found;
        }

        class ListTrait : List<CKTrait>
        {
            public bool TrueAdd( CKTrait t )
            {
                Add( t );
                return true;
            }
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> that contains the atomic traits from both this trait and <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Trait that must be kept.</param>
        /// <returns>The resulting trait.</returns>
        public CKTrait Intersect( CKTrait other )
        {
            if( ReferenceEquals( other, this ) ) return this;
            if( other == null ) throw new ArgumentNullException( "other" );
            if( !other.Context.Equals( _context ) ) throw new InvalidOperationException( Impl.CoreResources.TraitsMustBelongToTheSameContext );
            ListTrait m = new ListTrait();
            Process( this, other, null, null, m.TrueAdd );
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
            if( ReferenceEquals( other, this ) ) return this;
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            if( !other.Context.Equals( _context ) ) throw new InvalidOperationException( Impl.CoreResources.TraitsMustBelongToTheSameContext );
            ListTrait m = new ListTrait();
            Func<CKTrait,bool> add = m.TrueAdd;
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
            if( ReferenceEquals( other, this ) ) return _context.EmptyTrait;
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            if( !other.Context.Equals( _context ) ) throw new InvalidOperationException( Impl.CoreResources.TraitsMustBelongToTheSameContext );
            ListTrait m = new ListTrait();
            Process( this, other, m.TrueAdd, null, null );
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
            if( ReferenceEquals( other, this ) ) return _context.EmptyTrait;
            if( other == null ) throw new ArgumentNullException( "other" );
            if( !other.Context.Equals( _context ) ) throw new InvalidOperationException( Impl.CoreResources.TraitsMustBelongToTheSameContext );
            ListTrait m = new ListTrait();
            Func<CKTrait,bool> add = m.TrueAdd;
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
        static void Process( CKTrait left, CKTrait right, Func<CKTrait,bool> onLeft, Func<CKTrait,bool> onRight, Func<CKTrait,bool> onBoth )
        {
            IReadOnlyList<CKTrait> l = left.AtomicTraits;
            int cL = l.Count;
            int iL = 0;
            IReadOnlyList<CKTrait> r = right.AtomicTraits;
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
        /// Gets the number of <see cref="Fallbacks"/>. It is 2^<see cref="AtomicTraits"/>.<see cref="IReadOnlyCollection{T}.Count"/> - 1 since this
        /// trait itself does not appear in the fallbacks, but it is always 1 for atomic and the empty trait (the empty trait always ends the list).
        /// </summary>
        public long FallbacksLongCount
        {
            get { return _traits.Count > 1 ? ( 1L << _traits.Count ) - 1L : 1L; }
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
            Debug.Assert( _currentLength >= 1, "Empty and atomic traits are handled explicitly (EnumWithEmpty)." );
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
