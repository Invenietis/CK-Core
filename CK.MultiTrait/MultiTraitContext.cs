#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.MultiTrait\MultiTraitContext.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Thread-safe registration root for <see cref="MultiTrait"/> objects.
    /// </summary>
    public sealed class MultiTraitContext
    {
        static readonly Regex _canonize2 = new Regex( @"(\s*\+\s*)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant );

        readonly MultiTrait _empty;
        readonly ConcurrentDictionary<string, MultiTrait> _traits;
        readonly object _creationLock;

        public MultiTraitContext()
        {
            _empty = new MultiTrait( this );
            _traits = new ConcurrentDictionary<string, MultiTrait>( StringComparer.Ordinal );
            _traits[ String.Empty ] = _empty;
            _creationLock = new Object();
        }

        /// <summary>
        /// Gets the empty trait for this context. It corresponds to the empty string.
        /// </summary>
        public MultiTrait EmptyTrait { get { return _empty; } }

        /// <summary>
        /// Obtains a <see cref="MultiTrait"/> (either combined or atomic).
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by +.</param>
        /// <returns>A trait.</returns>
        public MultiTrait FindOrCreate( string traits )
        {
            return FindOrCreate( traits, true );
        }

        /// <summary>
        /// Finds a <see cref="MultiTrait"/> (either combined or atomic) only if all 
        /// of its atomic traits already exists: if any of the atomic traits are not already 
        /// registered, null is returned.
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by +.</param>
        /// <returns>A trait or null if the MultiTrait does not exists.</returns>
        public MultiTrait FindIfAllExist( string traits )
        {
            return FindOrCreate( traits, false );
        }

        /// <summary>
        /// Finds a <see cref="MultiTrait"/> with only already existing atomic traits.
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by +.</param>
        /// <param name="collector">Optional collector for unknown trait. As soon as the collector returns false, the process stops.</param>
        /// <returns>A trait that contains only already existing traits or null if none already exists.</returns>
        public MultiTrait FindOnlyExisting( string traits, Func<string,bool> collector = null )
        {
            if( traits == null || traits.Length == 0 ) return null;
            traits = traits.Normalize();
            MultiTrait m;
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
                    traits = String.Join( "+", splitTraits, 0, traitCount );
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        List<MultiTrait> atomics = new List<MultiTrait>();
                        for( int i = 0; i < traitCount; ++i )
                        {
                            MultiTrait trait = FindOrCreateAtomicTrait( splitTraits[i], false );
                            if( trait == null )
                            {
                                if( collector != null && !collector( splitTraits[i] ) ) break;
                            }
                            else atomics.Add( trait );
                        }
                        if( atomics.Count != 0 )
                        {
                            traits = String.Join( "+", atomics );
                            if( !_traits.TryGetValue( traits, out m ) )
                            {
                                lock( _creationLock )
                                {
                                    if( !_traits.TryGetValue( traits, out m ) )
                                    {
                                        m = new MultiTrait( this, traits, atomics.ToReadOnlyList() );
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


        MultiTrait FindOrCreate( string traits, bool create )
        {
            if( traits == null || traits.Length == 0 ) return _empty;
            traits = traits.Normalize();
            MultiTrait m;
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
                    traits = String.Join( "+", splitTraits, 0, traitCount );
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        MultiTrait[] atomics = new MultiTrait[traitCount];
                        for( int i = 0; i < traitCount; ++i )
                        {
                            MultiTrait trait = FindOrCreateAtomicTrait( splitTraits[i], create );
                            if( (atomics[i] = trait) == null ) return null;
                        }
                        lock( _creationLock )
                        {
                            if( !_traits.TryGetValue( traits, out m ) )
                            {
                                m = new MultiTrait( this, traits, new ReadOnlyListOnIList<MultiTrait>( atomics ) );
                                _traits[traits] = m;
                            }
                        }
                    }
                    Debug.Assert( !m.IsAtomic && m.AtomicTraits.Count == traitCount, "Combined trait." );
                }
            }
            return m;
        }

        private MultiTrait FindOrCreateAtomicTrait( string trait, bool create )
        {
            MultiTrait m;
            if( !_traits.TryGetValue( trait, out m ) && create )
            {
                lock( _creationLock )
                {
                    if( !_traits.TryGetValue( trait, out m ) )
                    {
                        m = new MultiTrait( this, trait );
                        _traits[trait] = m;
                    }
                }
                Debug.Assert( m.IsAtomic, "Special construction for atomic traits." );
            }
            return m;
        }
        
        /// <summary>
        /// Obtains a trait from a list of atomic (already sorted) traits.
        /// Used by the Add, Toggle, Remove, Intersect methods.
        /// </summary>
        internal MultiTrait FindOrCreate( List<MultiTrait> atomicTraits )
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
                b.Append( '+' ).Append( atomicTraits[i].ToString() );
            }
            string traits = b.ToString();
            MultiTrait m;
            if( !_traits.TryGetValue( traits, out m ) )
            {
                lock( _creationLock )
                {
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        m = new MultiTrait( this, traits, new ReadOnlyListOnIList<MultiTrait>( atomicTraits.ToArray() ) );
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
        internal MultiTrait FindOrCreate( MultiTrait[] atomicTraits, int count )
        {
            Debug.Assert( count > 1, "Atomic traits are handled directly." );

            Debug.Assert( !Array.Exists( atomicTraits, delegate( MultiTrait mA ) { return mA.Context != this || mA.AtomicTraits.Count != 1; } ),
                "Traits are from this Context and they are atomic and not empty." );

            StringBuilder b = new StringBuilder( atomicTraits[0].ToString() );
            for( int i = 1; i < count; ++i )
            {
                Debug.Assert( StringComparer.Ordinal.Compare( atomicTraits[i - 1].ToString(), atomicTraits[i].ToString() ) < 0, "Traits are already sorted and NO DUPLICATE exists." );
                b.Append( '+' ).Append( atomicTraits[i].ToString() );
            }
            string traits = b.ToString();
            MultiTrait m;
            if( !_traits.TryGetValue( traits, out m ) )
            {
                // We must clone the array since fallback generation reuses it.
                if( atomicTraits.Length != count )
                {
                    MultiTrait[] subArray = new MultiTrait[count];
                    Array.Copy( atomicTraits, subArray, count );
                    atomicTraits = subArray;
                }
                else atomicTraits = (MultiTrait[])atomicTraits.Clone();
                lock( _creationLock )
                {
                    if( !_traits.TryGetValue( traits, out m ) )
                    {
                        m = new MultiTrait( this, traits, new ReadOnlyListOnIList<MultiTrait>( atomicTraits ) );
                        _traits[traits] = m;
                    }
                }
            }
            return m;
        }

        static string[] SplitMultiTrait( string s, out int count )
        {
            string[] traits = _canonize2.Split( s.Trim() );
            count = traits.Length;
            if( count == 0 ) return Util.EmptyStringArray;
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
