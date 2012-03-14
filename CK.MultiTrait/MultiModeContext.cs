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
    /// Thread-safe registration root for <see cref="MultiMode"/> objects.
    /// </summary>
    public sealed class MultiModeContext
    {
        static readonly Regex _canonize2 = new Regex( @"(\s*\+\s*)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant );

        readonly MultiMode _empty;
        readonly ConcurrentDictionary<string, MultiMode> _modes;
        readonly object _creationLock;

        public MultiModeContext()
        {
            _empty = new MultiMode( this );
            _modes = new ConcurrentDictionary<string, MultiMode>( StringComparer.Ordinal );
            _modes[ String.Empty ] = _empty;
            _creationLock = new Object();
        }

        /// <summary>
        /// Gets the empty mode for this context. It corresponds to the empty string.
        /// </summary>
        public MultiMode EmptyMode { get { return _empty; } }

        /// <summary>
        /// Obtains a <see cref="MultiMode"/> (either combined or atomic).
        /// </summary>
        /// <param name="modes">Atomic mode or modes separated by +.</param>
        /// <returns>A mode.</returns>
        public MultiMode ObtainMode( string modes )
        {
            return FindOrCreate( modes, true );
        }

        /// <summary>
        /// Finds a <see cref="MultiMode"/> (either combined or atomic) only if all 
        /// of its atomic modes already exists: if any of the atomic modes are not already 
        /// registered, null is returned.
        /// </summary>
        /// <param name="modes">Atomic mode or modes separated by +.</param>
        /// <returns>A mode or null.</returns>
        public MultiMode FindAllModes( string modes )
        {
            return FindOrCreate( modes, false );
        }

        MultiMode FindOrCreate( string modes, bool create )
        {
            if( modes == null || modes.Length == 0 ) return _empty;
            modes = modes.Normalize();
            MultiMode m;
            if( !_modes.TryGetValue( modes, out m ) )
            {
                int modeCount;
                string[] splitModes = SplitMultiMode( modes, out modeCount );
                if( modeCount <= 0 ) return _empty;
                if( modeCount == 1 )
                {
                    m = FindOrCreateAtomicMode( splitModes[0], create );
                }
                else
                {
                    modes = String.Join( "+", splitModes, 0, modeCount );
                    if( !_modes.TryGetValue( modes, out m ) )
                    {
                        MultiMode[] atomics = new MultiMode[modeCount];
                        for( int i = 0; i < modeCount; ++i )
                        {
                            MultiMode mode = FindOrCreateAtomicMode( splitModes[i], create );
                            if( (atomics[i] = mode) == null ) return null;
                        }
                        lock( _creationLock )
                        {
                            if( !_modes.TryGetValue( modes, out m ) )
                            {
                                m = new MultiMode( this, modes, new ReadOnlyListOnIList<MultiMode>( atomics ) );
                                _modes[modes] = m;
                            }
                        }
                    }
                    Debug.Assert( !m.IsAtomic && m.AtomicModes.Count == modeCount, "Combined mode." );
                }
            }
            return m;
        }

        private MultiMode FindOrCreateAtomicMode( string mode, bool create )
        {
            MultiMode m;
            if( !_modes.TryGetValue( mode, out m ) && create )
            {
                lock( _creationLock )
                {
                    if( !_modes.TryGetValue( mode, out m ) )
                    {
                        m = new MultiMode( this, mode );
                        _modes[mode] = m;
                    }
                }
                Debug.Assert( m.IsAtomic, "Special construction for atomic modes." );
            }
            return m;
        }
        
        /// <summary>
        /// Obtains a mode from a list of atomic (already sorted) modes.
        /// Used by the Add, Toggle, Remove, Intersect methods.
        /// </summary>
        internal MultiMode ObtainMode( List<MultiMode> atomicModes )
        {
            if( atomicModes.Count == 0 ) return _empty;
            Debug.Assert( atomicModes[0].Context == this, "This is one of our modes." );
            Debug.Assert( atomicModes[0].AtomicModes.Count == 1, "This is an atomic mode and not the empty one." );
            if( atomicModes.Count == 1 ) return atomicModes[0];
            StringBuilder b = new StringBuilder( atomicModes[0].ToString() );
            for( int i = 1; i < atomicModes.Count; ++i )
            {
                Debug.Assert( atomicModes[i].Context == this, "This is one of our modes." );
                Debug.Assert( atomicModes[i].AtomicModes.Count == 1, "This is an atomic mode and not the empty one." );
                Debug.Assert( StringComparer.Ordinal.Compare( atomicModes[i - 1].ToString(), atomicModes[i].ToString() ) < 0,
                    "Modes are already sorted and NO DUPLICATES exist." );
                b.Append( '+' ).Append( atomicModes[i].ToString() );
            }
            string modes = b.ToString();
            MultiMode m;
            if( !_modes.TryGetValue( modes, out m ) )
            {
                lock( _creationLock )
                {
                    if( !_modes.TryGetValue( modes, out m ) )
                    {
                        m = new MultiMode( this, modes, new ReadOnlyListOnIList<MultiMode>( atomicModes.ToArray() ) );
                        _modes[modes] = m;
                    }
                }
            }
            return m;
        }

        /// <summary>
        /// Obtains a mode from a list of atomic (already sorted) modes.
        /// Used by fall back generation.
        /// </summary>
        internal MultiMode ObtainMode( MultiMode[] atomicModes, int count )
        {
            Debug.Assert( count > 1, "Atomic modes are handled directly." );

            Debug.Assert( !Array.Exists( atomicModes, delegate( MultiMode mA ) { return mA.Context != this || mA.AtomicModes.Count != 1; } ),
                "Modes are from this Context and they are atomic and not empty." );

            StringBuilder b = new StringBuilder( atomicModes[0].ToString() );
            for( int i = 1; i < count; ++i )
            {
                Debug.Assert( StringComparer.Ordinal.Compare( atomicModes[i - 1].ToString(), atomicModes[i].ToString() ) < 0, "Modes are already sorted and NO DUPLICATE exists." );
                b.Append( '+' ).Append( atomicModes[i].ToString() );
            }
            string modes = b.ToString();
            MultiMode m;
            if( !_modes.TryGetValue( modes, out m ) )
            {
                if( atomicModes.Length != count )
                {
                    MultiMode[] subArray = new MultiMode[count];
                    Array.Copy( atomicModes, subArray, count );
                    atomicModes = subArray;
                }
                else atomicModes = (MultiMode[])atomicModes.Clone();
                lock( _creationLock )
                {
                    if( !_modes.TryGetValue( modes, out m ) )
                    {
                        m = new MultiMode( this, modes, new ReadOnlyListOnIList<MultiMode>( atomicModes ) );
                        _modes[modes] = m;
                    }
                }
            }
            return m;
        }

        static string[] SplitMultiMode( string s, out int count )
        {
            string[] modes = _canonize2.Split( s.Trim() );
            count = modes.Length;
            if( count == 0 ) return Util.EmptyStringArray;
            int i = modes[0].Length == 0 ? 1 : 0;
            // Special handling for first and last slots if ther are empty.
            if( modes[count - 1].Length == 0 ) count = count - 1 - i;
            else count = count - i;
            if( count != modes.Length )
            {
                if( count <= 0 ) return Util.EmptyStringArray;
                string[] m = new string[count];
                Array.Copy( modes, i, m, 0, count );
                modes = m;
            }
            // Sort if necessary (more than one atomic mode).
            if( count > 1 )
            {
                Array.Sort( modes, StringComparer.Ordinal );
                // And removes duplicates. Since this occur very rarely
                // and that count is small we use a O(n) process that shifts
                // the modes array.
                i = count - 1;
                string last = modes[i];
                while( --i >= 0 )
                {
                    Debug.Assert( last.Length > 0, "There is no empty strings." );
                    string cur = modes[i];
                    if( StringComparer.Ordinal.Equals( cur, last ) )
                    {
                        int delta = (--count) - i - 1;
                        if( delta > 0 )
                        {
                            Array.Copy( modes, i + 2, modes, i + 1, delta );
                        }
                    }
                    last = cur;
                }
            }
            return modes;
        }
    }
}
