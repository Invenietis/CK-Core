using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CK.Core;

namespace CK.Keyboard
{
    public partial class KeyboardContext
    {
        static readonly Regex _canonize = new Regex( @"\s*(\+\s*)+", RegexOptions.Compiled | RegexOptions.CultureInvariant );

        KeyboardMode _empty;
        Dictionary<string, KeyboardMode> _modes;

        public IKeyboardMode EmptyMode { get { return _empty; } }

        public IKeyboardMode ObtainMode( string modes )
        {
            if ( modes == null || modes.Length == 0 ) return _empty;
            modes = modes.Normalize();
            KeyboardMode m;
            if ( !_modes.TryGetValue( modes, out m ) )
            {
                int modeCount;
                string[] splitModes = SplitMultiMode( modes, out modeCount );
                if ( modeCount <= 0 ) return _empty;
                if ( modeCount == 1 )
                {
                    modes = splitModes[0];
                    if ( !_modes.TryGetValue( modes, out m ) )
                    {
                        m = new KeyboardMode( this, modes );
                        _modes.Add( modes, m );
                    }
                    Debug.Assert( m.IsAtomic, "Special construction for atomic modes." );
                }
                else
                {
                    modes = String.Join( "+", splitModes, 0, modeCount );
                    if ( !_modes.TryGetValue( modes, out m ) )
                    {
                        IKeyboardMode[] atomics = new IKeyboardMode[modeCount];
                        for ( int i = 0; i < modeCount; ++i )
                        {
                            atomics[i] = ObtainMode( splitModes[i] );
                        }
                        m = new KeyboardMode( this, modes, new ReadOnlyListOnIList<IKeyboardMode>( atomics ) );
                        _modes.Add( modes, m );
                    }
                    Debug.Assert( !m.IsAtomic && m.AtomicModes.Count == modeCount, "Combined mode." );
                }
            }
            return m;
        }

        /// <summary>
        /// Obtains a mode from a list of atomic (already sorted) modes.
        /// Used by the Add, Toggle, Remove, Intersect methods.
        /// </summary>
        public IKeyboardMode ObtainMode( List<IKeyboardMode> atomicModes )
        {
            if ( atomicModes.Count == 0 ) return _empty;
            Debug.Assert( atomicModes[0].Context == this, "This is one of our modes." );
            Debug.Assert( atomicModes[0].AtomicModes.Count == 1, "This is an atomic mode and not the empty one." );
            if ( atomicModes.Count == 1 ) return atomicModes[0];
            StringBuilder b = new StringBuilder( atomicModes[0].ToString() );
            for ( int i = 1; i < atomicModes.Count; ++i )
            {
                Debug.Assert( atomicModes[i].Context == this, "This is one of our modes." );
                Debug.Assert( atomicModes[i].AtomicModes.Count == 1, "This is an atomic mode and not the empty one." );
                Debug.Assert( StringComparer.Ordinal.Compare( atomicModes[i - 1].ToString(), atomicModes[i].ToString() ) < 0,
                    "Modes are already sorted and NO DUPLICATES exist." );
                b.Append( '+' ).Append( atomicModes[i].ToString() );
            }
            string modes = b.ToString();
            KeyboardMode m;
            if ( !_modes.TryGetValue( modes, out m ) )
            {
                m = new KeyboardMode( this, modes, new ReadOnlyListOnIList<IKeyboardMode>( atomicModes.ToArray() ) );
                _modes.Add( modes, m );
            }
            return m;
        }

        /// <summary>
        /// Obtains a mode from a list of atomic (already sorted) modes.
        /// Used by fall back generation.
        /// </summary>
        public IKeyboardMode ObtainMode( IKeyboardMode[] atomicModes, int count )
        {
            Debug.Assert( count > 1, "Atomic modes are handled directly." );

            Debug.Assert( !Array.Exists( atomicModes, delegate( IKeyboardMode mA ) { return mA.Context != this || mA.AtomicModes.Count != 1; } ),
                "Modes are from this Context and they are atomic and not empty." );

            StringBuilder b = new StringBuilder( atomicModes[0].ToString() );
            for ( int i = 1; i < count; ++i )
            {
                Debug.Assert( StringComparer.Ordinal.Compare( atomicModes[i - 1].ToString(), atomicModes[i].ToString() ) < 0,
                    "Modes are already sorted and NO DUPLICATE exists." );
                b.Append( '+' ).Append( atomicModes[i].ToString() );
            }
            string modes = b.ToString();
            KeyboardMode m;
            if ( !_modes.TryGetValue( modes, out m ) )
            {
                if ( atomicModes.Length != count )
                {
                    IKeyboardMode[] subArray = new IKeyboardMode[count];
                    Array.Copy( atomicModes, subArray, count );
                    atomicModes = subArray;
                }
                else atomicModes = (IKeyboardMode[])atomicModes.Clone();
                m = new KeyboardMode( this, modes, new ReadOnlyListOnIList<IKeyboardMode>( atomicModes ) );
                _modes.Add( modes, m );
            }
            return m;
        }

        static string[] SplitMultiMode( string s, out int count )
        {
            s = _canonize.Replace( s.Trim(), "+" );
            string[] modes = s.Split( '+' );
            count = modes.Length;
            if ( count == 0 ) return Util.EmptyStringArray;
            int i = modes[0].Length == 0 ? 1 : 0;
            if ( modes[count - 1].Length == 0 ) count = count - 1 - i;
            else count = count - i;
            if ( count != modes.Length )
            {
                if ( count <= 0 ) return Util.EmptyStringArray;
                string[] m = new string[count];
                Array.Copy( modes, i, m, 0, count );
                modes = m;
            }
            // Sort if necessary (more than one atomic mode).
            if ( count > 1 )
            {
                Array.Sort( modes, StringComparer.Ordinal );
                // And removes duplicates. Since this occur very rarely
                // and that count is small we use a O(n) process that shifts
                // the modes array.
                i = count - 1;
                string last = modes[i];
                while ( --i >= 0 )
                {
                    Debug.Assert( last.Length > 0, "There is no empty strings." );
                    string cur = modes[i];
                    if ( StringComparer.Ordinal.Equals( cur, last ) )
                    {
                        int delta = (--count) - i - 1;
                        if ( delta > 0 )
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
