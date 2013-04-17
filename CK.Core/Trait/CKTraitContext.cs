#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\CKTraitContext.cs) is part of CiviKey. 
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
using System.Text;
using System.Text.RegularExpressions;
using CK.Core;
using System.Collections.Concurrent;
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// Thread-safe registration root for <see cref="CKTrait"/> objects.
    /// </summary>
    public sealed class CKTraitContext : IComparable<CKTraitContext>
    {
        static int _index;

        readonly CKTrait _empty;
        readonly IEnumerable<CKTrait> _enumerableWithEmpty;
        readonly Regex _canonize2;

        readonly ConcurrentDictionary<string, CKTrait> _traits;
        readonly object _creationLock;
        readonly string _uniqueName;
        readonly int _uniqueIndex;
        readonly string _separatorString;
        readonly char _separator;

        /// <summary>
        /// Initializes a new context for traits with a '|' as the separator.
        /// </summary>
        /// <param name="name">Name for the context. Must not be null nor whitespace.</param>
        public CKTraitContext( string name )
            : this( name, '|' )
        {
        }

        /// <summary>
        /// Initializes a new context for traits with the given separator.
        /// </summary>
        /// <param name="name">Name for the context. Must not be null nor whitespace.</param>
        /// <param name="separator">Separator if it must differ from '|'.</param>
        public CKTraitContext( string name, char separator )
        {
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( R.ArgumentMustNotBeNullOrWhiteSpace, "uniqueName" );
            _uniqueName = name.Normalize();
            _uniqueIndex = Interlocked.Increment( ref _index );
            _separator = separator;
            _separatorString = new String( separator, 1 );
            string pattern = "(\\s*" + Regex.Escape( _separatorString ) + "\\s*)+";
            _canonize2 = new Regex( pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant );
            _empty = new CKTrait( this );
            _traits = new ConcurrentDictionary<string, CKTrait>( StringComparer.Ordinal );
            _traits[ String.Empty ] = _empty;
            _enumerableWithEmpty = new CKReadOnlyListMono<CKTrait>( _empty );
            _creationLock = new Object();
        }

        /// <summary>
        /// Gets the separator to use to separate combined traits.
        /// </summary>
        public char Separator { get { return _separator; } }

        /// <summary>
        /// Gets the name of this context.
        /// </summary>
        public string Name { get { return _uniqueName; } }

        /// <summary>
        /// Compares this context to another one.
        /// The key is <see cref="Separator"/>, then <see cref="Name"/> and if they are equal, a unique number is 
        /// used to order the two contexts.
        /// </summary>
        /// <param name="other">Context to compare.</param>
        /// <returns>0 for the exact same object (ReferenceEquals), greater/lower than 0 otherwise.</returns>
        public int CompareTo( CKTraitContext other )
        {
            if( other == null ) throw new ArgumentNullException( "other" );
            if( ReferenceEquals( this, other ) ) return 0;
            int cmp = _separator - other._separator;
            if( cmp == 0 )
            {
                cmp = StringComparer.Ordinal.Compare( _uniqueName, other._uniqueName );
                if( cmp == 0 ) cmp = _uniqueIndex - other._uniqueIndex;
            }
            return cmp;
        }

        /// <summary>
        /// Gets the empty trait for this context. It corresponds to the empty string.
        /// </summary>
        public CKTrait EmptyTrait { get { return _empty; } }

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> (either combined or atomic).
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by +.</param>
        /// <returns>A trait.</returns>
        public CKTrait FindOrCreate( string traits )
        {
            return FindOrCreate( traits, true );
        }

        /// <summary>
        /// Finds a <see cref="CKTrait"/> (either combined or atomic) only if all 
        /// of its atomic traits already exists: if any of the atomic traits are not already 
        /// registered, null is returned.
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by +.</param>
        /// <returns>A trait or null if the trait does not exists.</returns>
        public CKTrait FindIfAllExist( string traits )
        {
            return FindOrCreate( traits, false );
        }

        /// <summary>
        /// Finds a <see cref="CKTrait"/> with only already existing atomic traits (null when not found).
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by +.</param>
        /// <param name="collector">Optional collector for unknown trait. As soon as the collector returns false, the process stops.</param>
        /// <returns>A trait that contains only already existing trait or null if none already exists.</returns>
        public CKTrait FindOnlyExisting( string traits, Func<string,bool> collector = null )
        {
            if( traits == null || traits.Length == 0 ) return null;
            traits = traits.Normalize();
            CKTrait m;
            if( !_traits.TryGetValue( traits, out m ) )
            {
                int traitCount;
                string[] splitTraits = SplitMultiTrait( traits, out traitCount );
                if( traitCount <= 0 ) return null;
                if( traitCount == 1 )
                {
                    m = FindOrCreateAtomicTrait( splitTraits[0], false );
                }
                else
                {
                    traits = String.Join( _separatorString, splitTraits, 0, traitCount );
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        List<CKTrait> atomics = new List<CKTrait>();
                        for( int i = 0; i < traitCount; ++i )
                        {
                            CKTrait trait = FindOrCreateAtomicTrait( splitTraits[i], false );
                            if( trait == null )
                            {
                                if( collector != null && !collector( splitTraits[i] ) ) break;
                            }
                            else atomics.Add( trait );
                        }
                        if( atomics.Count != 0 )
                        {
                            traits = String.Join( _separatorString, atomics );
                            if( !_traits.TryGetValue( traits, out m ) )
                            {
                                lock( _creationLock )
                                {
                                    if( !_traits.TryGetValue( traits, out m ) )
                                    {
                                        m = new CKTrait( this, traits, atomics.ToReadOnlyList() );
                                        _traits[traits] = m;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return m;
        }


        CKTrait FindOrCreate( string traits, bool create )
        {
            if( traits == null || traits.Length == 0 ) return _empty;
            traits = traits.Normalize();
            CKTrait m;
            if( !_traits.TryGetValue( traits, out m ) )
            {
                int traitCount;
                string[] splitTraits = SplitMultiTrait( traits, out traitCount );
                if( traitCount <= 0 ) return _empty;
                if( traitCount == 1 )
                {
                    m = FindOrCreateAtomicTrait( splitTraits[0], create );
                }
                else
                {
                    traits = String.Join( _separatorString, splitTraits, 0, traitCount );
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        CKTrait[] atomics = new CKTrait[traitCount];
                        for( int i = 0; i < traitCount; ++i )
                        {
                            CKTrait trait = FindOrCreateAtomicTrait( splitTraits[i], create );
                            if( (atomics[i] = trait) == null ) return null;
                        }
                        lock( _creationLock )
                        {
                            if( !_traits.TryGetValue( traits, out m ) )
                            {
                                m = new CKTrait( this, traits, new CKReadOnlyListOnIList<CKTrait>( atomics ) );
                                _traits[traits] = m;
                            }
                        }
                    }
                    Debug.Assert( !m.IsAtomic && m.AtomicTraits.Count == traitCount, "Combined trait." );
                }
            }
            return m;
        }

        CKTrait FindOrCreateAtomicTrait( string trait, bool create )
        {
            CKTrait m;
            if( !_traits.TryGetValue( trait, out m ) && create )
            {
                lock( _creationLock )
                {
                    if( !_traits.TryGetValue( trait, out m ) )
                    {
                        m = new CKTrait( this, trait );
                        _traits[trait] = m;
                    }
                }
                Debug.Assert( m.IsAtomic, "Special construction for atomic traits." );
            }
            return m;
        }

        /// <summary>
        /// Gets the fallback for empty and atomic traits.
        /// </summary>
        internal IEnumerable<CKTrait> EnumWithEmpty { get { return _enumerableWithEmpty; } }

        /// <summary>
        /// Obtains a trait from a list of atomic (already sorted) traits.
        /// Used by the Add, Toggle, Remove, Intersect methods.
        /// </summary>
        internal CKTrait FindOrCreate( List<CKTrait> atomicTraits )
        {
            if( atomicTraits.Count == 0 ) return _empty;
            Debug.Assert( atomicTraits[0].Context == this, "This is one of our traits." );
            Debug.Assert( atomicTraits[0].AtomicTraits.Count == 1, "This is an atomic trait and not the empty one." );
            if( atomicTraits.Count == 1 ) return atomicTraits[0];
            StringBuilder b = new StringBuilder( atomicTraits[0].ToString() );
            for( int i = 1; i < atomicTraits.Count; ++i )
            {
                Debug.Assert( atomicTraits[i].Context == this, "This is one of our traits." );
                Debug.Assert( atomicTraits[i].AtomicTraits.Count == 1, "This is an atomic trait and not the empty one." );
                Debug.Assert( StringComparer.Ordinal.Compare( atomicTraits[i - 1].ToString(), atomicTraits[i].ToString() ) < 0,
                    "Traits are already sorted and NO DUPLICATES exist." );
                b.Append( _separator ).Append( atomicTraits[i].ToString() );
            }
            string traits = b.ToString();
            CKTrait m;
            if( !_traits.TryGetValue( traits, out m ) )
            {
                lock( _creationLock )
                {
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        m = new CKTrait( this, traits, new CKReadOnlyListOnIList<CKTrait>( atomicTraits.ToArray() ) );
                        _traits[traits] = m;
                    }
                }
            }
            return m;
        }

        /// <summary>
        /// Obtains a trait from a list of atomic (already sorted) traits.
        /// Used by fall back generation.
        /// </summary>
        internal CKTrait FindOrCreate( CKTrait[] atomicTraits, int count )
        {
            Debug.Assert( count > 1, "Atomic traits are handled directly." );

            Debug.Assert( !Array.Exists( atomicTraits, mA => mA.Context != this || mA.AtomicTraits.Count != 1 ), "Traits are from this Context and they are atomic and not empty." );

            StringBuilder b = new StringBuilder( atomicTraits[0].ToString() );
            for( int i = 1; i < count; ++i )
            {
                Debug.Assert( StringComparer.Ordinal.Compare( atomicTraits[i - 1].ToString(), atomicTraits[i].ToString() ) < 0, "Traits are already sorted and NO DUPLICATE exists." );
                b.Append( '+' ).Append( atomicTraits[i].ToString() );
            }
            string traits = b.ToString();
            CKTrait m;
            if( !_traits.TryGetValue( traits, out m ) )
            {
                // We must clone the array since fallback generation reuses it.
                if( atomicTraits.Length != count )
                {
                    CKTrait[] subArray = new CKTrait[count];
                    Array.Copy( atomicTraits, subArray, count );
                    atomicTraits = subArray;
                }
                else atomicTraits = (CKTrait[])atomicTraits.Clone();
                lock( _creationLock )
                {
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        m = new CKTrait( this, traits, new CKReadOnlyListOnIList<CKTrait>( atomicTraits ) );
                        _traits[traits] = m;
                    }
                }
            }
            return m;
        }

        string[] SplitMultiTrait( string s, out int count )
        {
            string[] traits = _canonize2.Split( s.Trim() );
            count = traits.Length;
            Debug.Assert( count != 0, "Split always create a cell." );
            int i = traits[0].Length == 0 ? 1 : 0;
            // Special handling for first and last slots if ther are empty.
            if( traits[count - 1].Length == 0 ) count = count - 1 - i;
            else count = count - i;
            if( count != traits.Length )
            {
                if( count <= 0 ) return Util.EmptyStringArray;
                string[] m = new string[count];
                Array.Copy( traits, i, m, 0, count );
                traits = m;
            }
            // Sort if necessary (more than one atomic trait).
            if( count > 1 )
            {
                Array.Sort( traits, StringComparer.Ordinal );
                // And removes duplicates. Since this occur very rarely
                // and that count is small we use a O(n) process that shifts
                // the traits array.
                i = count - 1;
                string last = traits[i];
                while( --i >= 0 )
                {
                    Debug.Assert( last.Length > 0, "There is no empty strings." );
                    string cur = traits[i];
                    if( StringComparer.Ordinal.Equals( cur, last ) )
                    {
                        int delta = (--count) - i - 1;
                        if( delta > 0 )
                        {
                            Array.Copy( traits, i + 2, traits, i + 1, delta );
                        }
                    }
                    last = cur;
                }
            }
            return traits;
        }

    }
}
