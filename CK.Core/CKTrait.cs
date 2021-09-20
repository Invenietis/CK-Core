using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CK.Core;
using System.Threading;
using System.ComponentModel;

#nullable enable

namespace CK.Core
{
    /// <summary>
    /// A tag is an immutable object, associated to a unique string inside a <see cref="Context"/> (that is thread-safe), that can be
    /// atomic ("Alt", "Home", "Ctrl") or combined ("Alt|Ctrl", "Alt|Ctrl|Home") with a <see cref="CKTraitContext.Separator"/> character.
    /// <para>
    /// It supports implicit conversion to its string representation, however no conversion is possible the other way around (from mere string
    /// to trait) since a Context must be specified.
    /// </para>
    /// <para>
    /// The only way to obtain a CKTrait is to call <see cref="CKTraitContext.FindOrCreate(string)"/> (from a string) or to use one of the available combination
    /// methods (<see cref="Union"/>, <see cref="Except"/>, <see cref="SymmetricExcept"/> or <see cref="Intersect"/> ).
    /// </para>
    /// </summary>
    /// <remarks>
    /// A CKTrait is easily serializable as its <see cref="ToString"/> representation and restored with <see cref="CKTraitContext.FindOrCreate(string)"/>
    /// on the appropriate context.
    /// </remarks>
    public sealed class CKTrait : IComparable<CKTrait>, IEquatable<CKTrait>
    {
        readonly CKTraitContext _context;
        readonly string _tag;
        readonly IReadOnlyList<CKTrait> _tags;

        /// <summary>
        /// Initializes the new empty tag of a CKTraitContext.
        /// </summary>
        internal CKTrait( CKTraitContext ctx )
        {
            Debug.Assert( ctx.EmptyTrait == null, "There is only one empty tag per context." );
            _context = ctx;
            _tag = String.Empty;
            _tags = Array.Empty<CKTrait>();
        }

        /// <summary>
        /// Initializes a new atomic tag.
        /// </summary>
        internal CKTrait( CKTraitContext ctx, string atomicTag )
        {
            Debug.Assert( atomicTag.Contains( ctx.Separator ) == false );
            _context = ctx;
            _tag = atomicTag;
            _tags = new CKTrait[] { this };
        }

        /// <summary>
        /// Initializes a new combined tag.
        /// </summary>
        internal CKTrait( CKTraitContext ctx, string combinedTag, IReadOnlyList<CKTrait> tags )
        {
            Debug.Assert( combinedTag.IndexOf( ctx.Separator ) > 0 && tags.Count > 1, "There is more than one tag in a Combined Tag." );
            Debug.Assert( tags.All( m => m.IsAtomic ), "Provided tags are all atomic." );
            Debug.Assert( tags.GroupBy( m => m ).Where( g => g.Count() != 1 ).Count() == 0, "No duplicate in atomic in tags." );
            _context = ctx;
            _tag = combinedTag;
            _tags = tags;
        }

        /// <summary>
        /// Gets the <see cref="CKTraitContext"/> to which this tag belongs. 
        /// </summary>
        public CKTraitContext Context => _context;

        /// <summary>
        /// Gets the multi tags in an ordered manner separated by <see cref="CKTraitContext.Separator"/>.
        /// </summary>
        /// <returns>This multi tags as a string.</returns>
        public override string ToString() => _tag;

        /// <summary>
        /// Gets the atomic tags that this tag contains.
        /// This list does not contain the empty tag and is sorted according to the name of the atomic tags (lexical order): this is the 
        /// same as the <see cref="ToString"/> representation.
        /// Note that it is in reverse order regarding <see cref="CompareTo"/> ("A" that is stronger than "B" appears before "B").
        /// </summary>
        public IReadOnlyList<CKTrait> AtomicTraits => _tags;

        /// <summary>
        /// Gets a boolean indicating whether this tag is the empty tag (<see cref="AtomicTraits"/> is empty
        /// and <see cref="Fallbacks"/> contains only itself).
        /// </summary>
        public bool IsEmpty => _tag.Length == 0; 

        /// <summary>
        /// Gets a boolean indicating whether this tag contains zero 
        /// (the empty tag is considered as an atomic tag) or only one atomic tag.
        /// </summary>
        /// <remarks>
        /// For atomic tags (and the empty tag itself), <see cref="Fallbacks"/> contains only the <see cref="CKTraitContext.EmptyTrait"/>.
        /// </remarks>
        public bool IsAtomic => _tags.Count <= 1; 

        /// <summary>
        /// Compares this tag with another one.
        /// The <see cref="Context"/> is the primary key (see <see cref="CKTraitContext.CompareTo"/>), then comes 
        /// the number of tags (more tags is greater) and then comes the string representation of the tag in 
        /// reverse lexical order (<see cref="StringComparer.Ordinal"/>): "A" is greater than "B".
        /// </summary>
        /// <param name="other">The tag to compare to. Can be null: any trait is greater than null.</param>
        /// <returns>A negative, zero or positive value.</returns>
        public int CompareTo( CKTrait? other )
        {
            if( other == null ) return 1;
            if( ReferenceEquals( this, other ) ) return 0;
            int cmp = _context.CompareTo( other._context );
            if( cmp == 0 )
            {
                cmp = _tags.Count - other.AtomicTraits.Count;
                if( cmp == 0 ) cmp = StringComparer.Ordinal.Compare( other._tag, _tag );
            }
            return cmp;
        }

        /// <summary>
        /// Checks equality of this tag with another one.
        /// </summary>
        /// <param name="other">The tag to compare to.</param>
        /// <returns>True on equality.</returns>
        public bool Equals( CKTrait? other ) => ReferenceEquals( this, other );

        /// <summary>
        /// Checks if each and every atomic tags of <paramref name="other" /> exists in this tag.
        /// </summary>
        /// <param name="other">The tag(s) to find.</param>
        /// <returns>True if all the specified tags appear in this tag.</returns>
        /// <remarks>
        /// Note that <see cref="CKTraitContext.EmptyTrait"/> is contained (in the sense of this IsSupersetOf method) by definition in any tag 
        /// (including itself): this is the opposite of the <see cref="Overlaps"/> method.
        /// </remarks>
        public bool IsSupersetOf( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( Impl.CoreResources.TagsMustBelongToTheSameContext );
            if( _tags.Count < other._tags.Count ) return false;
            bool foundAlien = false;
            Process( this, other,
                     null,
                     delegate( CKTrait m ) { foundAlien = true; return false; },
                     null );
            return !foundAlien;
        }

        /// <summary>
        /// Checks if one of the atomic tags of <paramref name="other" /> exists in this tag.
        /// </summary>
        /// <param name="other">The tag to find.</param>
        /// <returns>Returns true if one of the specified tags appears in this tag.</returns>
        /// <remarks>
        /// When true, this ensures that <see cref="Intersect"/>( <paramref name="other"/> ) != <see cref="CKTraitContext.EmptyTrait"/>. 
        /// The empty tag is not contained (in the sense of this ContainsOne method) in any tag (including itself). This is the opposite
        /// of the <see cref="IsSupersetOf"/> method.
        /// </remarks>
        public bool Overlaps( CKTrait other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( Impl.CoreResources.TagsMustBelongToTheSameContext );
            bool found = false;
            Process( this, other,
                null,
                null,
                delegate( CKTrait m ) { found = true; return false; } );
            return found;
        }

        class ListTag : List<CKTrait>
        {
            public bool TrueAdd( CKTrait t )
            {
                Add( t );
                return true;
            }
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> that contains the atomic tags from both this tag and <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Tag that must be kept.</param>
        /// <returns>The resulting tag.</returns>
        public CKTrait Intersect( CKTrait other )
        {
            if( ReferenceEquals( other, this ) ) return this;
            if( other == null ) throw new ArgumentNullException( "other" );
            if( other.Context != _context ) throw new InvalidOperationException( Impl.CoreResources.TagsMustBelongToTheSameContext );
            ListTag m = new ListTag();
            Process( this, other, null, null, m.TrueAdd );
            return _context.FindOrCreateFromAtomicSortedList( m );
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> that combines this one and 
        /// the tzg(s) specified by the parameter. 
        /// </summary>
        /// <param name="other">Tag to add.</param>
        /// <returns>The resulting tag.</returns>
        public CKTrait Union( CKTrait other )
        {
            if( ReferenceEquals( other, this ) ) return this;
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            if( other.Context != _context ) throw new InvalidOperationException( Impl.CoreResources.TagsMustBelongToTheSameContext );
            ListTag m = new ListTag();
            Func<CKTrait,bool> add = m.TrueAdd;
            Process( this, other, add, add, add );
            return _context.FindOrCreateFromAtomicSortedList( m );
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> from which tag(s) specified by the parameter are removed.
        /// </summary>
        /// <param name="other">Tag to remove.</param>
        /// <returns>The resulting tag.</returns>
        public CKTrait Except( CKTrait other )
        {
            if( ReferenceEquals( other, this ) ) return _context.EmptyTrait;
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            if( other.Context != _context ) throw new InvalidOperationException( Impl.CoreResources.TagsMustBelongToTheSameContext );
            ListTag m = new ListTag();
            Process( this, other, m.TrueAdd, null, null );
            return _context.FindOrCreateFromAtomicSortedList( m );
        }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> where the atomic tags of <paramref name="other" /> are removed (resp. added) depending 
        /// on whether they exist (resp. do not exist) in this tag. This is like an Exclusive Or (XOR), this implements a "toggle"
        /// operation.
        /// </summary>
        /// <param name="other">Tag to toggle.</param>
        /// <returns>The resulting tag.</returns>
        public CKTrait SymmetricExcept( CKTrait other )
        {
            if( ReferenceEquals( other, this ) ) return _context.EmptyTrait;
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            if( other.Context != _context ) throw new InvalidOperationException( Impl.CoreResources.TagsMustBelongToTheSameContext );
            ListTag m = new ListTag();
            Func<CKTrait,bool> add = m.TrueAdd;
            Process( this, other, add, add, null );
            return _context.FindOrCreateFromAtomicSortedList( m );
        }

        /// <summary>
        /// Applies the given <see cref="SetOperation"/>.
        /// </summary>
        /// <param name="other">Tag to combine.</param>
        /// <param name="operation">Set operation.</param>
        /// <returns>Resulting tag.</returns>
        public CKTrait Apply( CKTrait other, SetOperation operation )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            switch( operation )
            {
                case SetOperation.Union: return Union( other );
                case SetOperation.Except: return Except( other );
                case SetOperation.Intersect: return Intersect( other );
                case SetOperation.SymetricExcept: return SymmetricExcept( other );
                case SetOperation.None: return this;
            }
            Debug.Assert( operation == SetOperation.Replace, "All operations are covered." );
            return other;
        }

        /// <summary>
        /// Implicit conversion to sting: simply returns the normalized <see cref="ToString"/> form.
        /// </summary>
        /// <param name="t">The trait to convert.</param>
        public static implicit operator String( CKTrait t ) => t._tag;

        /// <summary>
        /// Lesser than comparison.
        /// </summary>
        /// <param name="t1">The first trait to compare.</param>
        /// <param name="t2">The second trait to compare.</param>
        /// <returns>Whether t1 is smaller than t2.</returns>
        public static bool operator <( CKTrait? t1, CKTrait? t2 ) => t1 == null ? t2 != null : t1.CompareTo( t2 ) < 0;

        /// <summary>
        /// Greater than comparison.
        /// </summary>
        /// <param name="t1">The trait to convert.</param>
        /// <param name="t2">The trait to convert.</param>
        /// <returns>Whether t1 is greater than t2.</returns>
        public static bool operator >( CKTrait? t1, CKTrait? t2 ) => t2 == null ? t1 != null : t2.CompareTo( t1 ) <= 0;

        /// <summary>
        /// Lesser or equal comparison.
        /// </summary>
        /// <param name="t1">The first trait to compare.</param>
        /// <param name="t2">The second trait to compare.</param>
        /// <returns>Whether t1 is smaller or equal to t2.</returns>
        public static bool operator <=( CKTrait? t1, CKTrait? t2 ) => t1 == null || t1.CompareTo( t2 ) <= 0;

        /// <summary>
        /// Greater or equal comparison.
        /// </summary>
        /// <param name="t1">The first trait to compare.</param>
        /// <param name="t2">The second trait to compare.</param>
        /// <returns>Whether t1 is greater or equal to t2.</returns>
        public static bool operator >=( CKTrait? t1, CKTrait? t2 ) => t2 == null || t2.CompareTo( t1 ) < 0;

        /// <summary>
        /// Calls <see cref="Union(CKTrait)"/> (<see cref="Context"/> must be the same).
        /// This is the same operator as the | operator.
        /// </summary>
        /// <param name="t1">The first tag to combine.</param>
        /// <param name="t2">The second tag to combine.</param>
        /// <returns>The combined trait.</returns>
        public static CKTrait? operator +( CKTrait? t1, CKTrait? t2 ) => t1 == null ? t2 : (t2 == null ? t1 : t1.Union( t2 ));

        /// <summary>
        /// Calls <see cref="Union(CKTrait)"/> (<see cref="Context"/> must be the same).
        /// This is the same operator as the + operator.
        /// </summary>
        /// <param name="t1">The first tag to combine.</param>
        /// <param name="t2">The second tag to combine.</param>
        /// <returns>The combined trait.</returns>
        public static CKTrait? operator |( CKTrait? t1, CKTrait? t2 ) => t1 == null ? t2 : (t2 == null ? t1 : t1.Union( t2 ));

        /// <summary>
        /// Calls <see cref="Except"/> (<see cref="Context"/> must be the same).
        /// </summary>
        /// <param name="t1">The first tag to combine.</param>
        /// <param name="t2">The second tag to combine.</param>
        /// <returns>The combined trait.</returns>
        public static CKTrait? operator -( CKTrait? t1, CKTrait? t2 ) => t1 == null ? t2 : (t2 == null ? t1 : t1.Except( t2 ));

        /// <summary>
        /// Calls <see cref="Intersect"/> (<see cref="Context"/> must be the same).
        /// </summary>
        /// <param name="t1">The first tag to combine.</param>
        /// <param name="t2">The second tag to combine.</param>
        /// <returns>The combined trait.</returns>
        public static CKTrait? operator &( CKTrait? t1, CKTrait? t2 ) => t1 == null ? t2 : (t2 == null ? t1 : t1.Intersect( t2 ));

        /// <summary>
        /// Calls <see cref="SymmetricExcept"/> (<see cref="Context"/> must be the same).
        /// </summary>
        /// <param name="t1">The first tag to combine.</param>
        /// <param name="t2">The second tag to combine.</param>
        /// <returns>The combined trait.</returns>
        public static CKTrait? operator ^( CKTrait? t1, CKTrait? t2 ) => t1 == null ? t2 : (t2 == null ? t1 : t1.SymmetricExcept( t2 ));

        /// <summary>
        /// Common process function where 3 predicates drive the result: each atomic tag is submitted to one of the 3 predicates
        /// depending on whether it is only in the left, only in the right or appears in both tags.
        /// When returning false, a predicate stops the process.
        /// </summary>
        /// <remarks>
        /// When this predicate is 'adding the tag to a list', we can draw the following table where '1' means the predicate exists and '0' means
        /// no predicate (or the 'always true' one):
        /// 
        ///             0, 0, 0 =  -- 'Empty'
        /// Intersect   0, 0, 1 = Intersect (keep commons) => /Toggle (SymmetricExcept)
        ///             0, 1, 0 =  -- 'Cleanup' (keep theirs only) => /Remove 
        ///             0, 1, 1 =  -- 'Other' (keep theirs and commons, reject mine) => /This
        /// Except      1, 0, 0 = Remove (keep mine only) => /Cleanup
        ///             1, 0, 1 =  -- 'This' (keep mine and commons and reject theirs) => /Other
        /// Toggle      1, 1, 0 = Toggle (SymmetricExcept) (keep mine, theirs, but reject commons) => /Intersect
        /// Union       1, 1, 1 = Add
        /// 
        /// This shows that our 4 methods Intersect, Remove, Toggle and Add cover the interesting cases - others are either symetric or useless.
        /// </remarks>
        static void Process( CKTrait left, CKTrait right, Func<CKTrait,bool>? onLeft, Func<CKTrait,bool>? onRight, Func<CKTrait,bool>? onBoth )
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
        /// tag itself does not appear in the fallbacks, but it is always 1 for atomic and the empty tag (the empty tag always ends the list).
        /// </summary>
        public int FallbacksCount => _tags.Count > 1 ? ( 1 << _tags.Count ) - 1 : 1;

        /// <summary>
        /// Gets the number of <see cref="Fallbacks"/>. It is 2^<see cref="AtomicTraits"/>.<see cref="IReadOnlyCollection{T}.Count"/> - 1 since this
        /// tag itself does not appear in the fallbacks, but it is always 1 for atomic and the empty tag (the empty tag always ends the list).
        /// </summary>
        public long FallbacksLongCount => _tags.Count > 1 ? ( 1L << _tags.Count ) - 1L : 1L; 

        /// <summary>
        /// Gets an enumeration of fallbacks to consider for this tag ordered from best to worst.
        /// This tag does not start the list but the <see cref="CKTraitContext.EmptyTrait"/> always ends this list.
        /// </summary>
        /// <remarks>
        /// For atomic tags (and the empty tag itself), <see cref="Fallbacks"/> contains only the <see cref="CKTraitContext.EmptyTrait"/>.
        /// </remarks>
        public IEnumerable<CKTrait> Fallbacks
        {
            get
            {
                if( _tags.Count <= 1 ) return _context.EnumWithEmpty;
                return ComputeFallbacks();
            }
        }

        IEnumerable<CKTrait> ComputeFallbacks()
        {
            int _currentLength = _tags.Count - 1;
            Debug.Assert( _currentLength >= 1, "Empty and atomic tags are handled explicitly (EnumWithEmpty)." );
            if( _currentLength > 1 )
            {
                int nbTag = _tags.Count;
                bool[] kept = new bool[nbTag];
                CKTrait[] v = new CKTrait[_currentLength];
                do
                {
                    int i = nbTag;
                    while( --i >= _currentLength ) kept[i] = false;
                    int kMax = i;
                    while( i >= 0 ) kept[i--] = true;
                    do
                    {
                        i = 0;
                        for( int j = 0; j < nbTag; ++j )
                        {
                            if( kept[j] ) v[i++] = _tags[j];
                        }
                        Debug.Assert( i == _currentLength, "We kept the right number of tags." );
                        yield return _context.FindOrCreate( v, i );
                    }
                    while( Forward( kept, ref kMax ) );
                }
                while( --_currentLength > 1 );
            }
            // Special processing for currentLength = 1 (optimization)
            foreach( CKTrait m in _tags ) yield return m;
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
