using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CK.Core
{
    /// <summary>
    /// Thread-safe registration root for <see cref="CKTrait"/> objects.
    /// Each context has a <see cref="Name"/> that uniquely, and definitely, identifies it. Any number of named contexts
    /// can be created in an Application Domain: as long as they define the same <see cref="Separator"/>, they all are actually
    /// the exact same internal context. (If Separator differ during a redefinition, an <see cref="InvalidOperationException"/> is thrown.)
    /// </summary>
    public readonly struct CKTraitContext : IComparable<CKTraitContext>, IEquatable<CKTraitContext>
    {
        /// <summary>
        /// Private implementation: this is the actual class: exposed CKTraitContext is just a typed reference.
        /// </summary>
        sealed class Impl : IComparable<Impl>
        {
            readonly Regex _canonize2;
            readonly ConcurrentDictionary<string, CKTrait> _traits;
            readonly object _creationLock;
            readonly string _separatorString;

            public Impl( string name, char separator = '|' )
            {
                if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( Core.Impl.CoreResources.ArgumentMustNotBeNullOrWhiteSpace, "uniqueName" );
                Name = name.Normalize();
                Separator = separator;
                _separatorString = new String( separator, 1 );
                string pattern = "(\\s*" + Regex.Escape( _separatorString ) + "\\s*)+";
                _canonize2 = new Regex( pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant );
                EmptyTrait = new CKTrait( new CKTraitContext( this ) );
                _traits = new ConcurrentDictionary<string, CKTrait>( StringComparer.Ordinal );
                _traits[String.Empty] = EmptyTrait;
                EnumWithEmpty = new CKTrait[] { EmptyTrait };
                _creationLock = new Object();
            }

            public readonly char Separator;

            public readonly string Name;

            public int CompareTo( Impl other )
            {
                Debug.Assert( other != null && !ReferenceEquals( this, other ) );
                int cmp = Separator - other.Separator;
                if( cmp == 0 )
                {
                    cmp = StringComparer.Ordinal.Compare( Name, other.Name );
                    Debug.Assert( cmp != 0 );
                }
                return cmp;
            }

            public readonly CKTrait EmptyTrait;

            public readonly IEnumerable<CKTrait> EnumWithEmpty;

            public CKTrait FindOnlyExisting( string traits, Func<string, bool> collector = null )
            {
                if( traits == null || traits.Length == 0 ) return null;
                traits = traits.Normalize( NormalizationForm.FormC );
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
                                            m = new CKTrait( new CKTraitContext( this ), traits, atomics.ToArray() );
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

            public CKTrait FindOrCreate( string traits, bool create )
            {
                if( traits == null || traits.Length == 0 ) return EmptyTrait;
                traits = traits.Normalize();
                if( traits.IndexOfAny( new[] { '\n', '\r' } ) >= 0 ) throw new ArgumentException( Core.Impl.CoreResources.TraitsMustNotBeMultiLineString );
                CKTrait m;
                if( !_traits.TryGetValue( traits, out m ) )
                {
                    int traitCount;
                    string[] splitTraits = SplitMultiTrait( traits, out traitCount );
                    if( traitCount <= 0 ) return EmptyTrait;
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
                                    m = new CKTrait( new CKTraitContext( this ), traits, atomics );
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
                            m = new CKTrait( new CKTraitContext( this ), trait );
                            _traits[trait] = m;
                        }
                    }
                    Debug.Assert( m.IsAtomic, "Special construction for atomic traits." );
                }
                return m;
            }

            public CKTrait FindOrCreate( List<CKTrait> atomicTraits )
            {
                if( atomicTraits.Count == 0 ) return EmptyTrait;
                Debug.Assert( atomicTraits[0].Context._c == this, "This is one of our traits." );
                Debug.Assert( atomicTraits[0].AtomicTraits.Count == 1, "This is an atomic trait and not the empty one." );
                if( atomicTraits.Count == 1 ) return atomicTraits[0];
                StringBuilder b = new StringBuilder( atomicTraits[0].ToString() );
                for( int i = 1; i < atomicTraits.Count; ++i )
                {
                    Debug.Assert( atomicTraits[i].Context._c == this, "This is one of our traits." );
                    Debug.Assert( atomicTraits[i].AtomicTraits.Count == 1, "This is an atomic trait and not the empty one." );
                    Debug.Assert( StringComparer.Ordinal.Compare( atomicTraits[i - 1].ToString(), atomicTraits[i].ToString() ) < 0,
                        "Traits are already sorted and NO DUPLICATES exist." );
                    b.Append( Separator ).Append( atomicTraits[i].ToString() );
                }
                string traits = b.ToString();
                CKTrait m;
                if( !_traits.TryGetValue( traits, out m ) )
                {
                    lock( _creationLock )
                    {
                        if( !_traits.TryGetValue( traits, out m ) )
                        {
                            m = new CKTrait( new CKTraitContext( this ), traits, atomicTraits.ToArray() );
                            _traits[traits] = m;
                        }
                    }
                }
                return m;
            }

            public CKTrait FindOrCreate( CKTrait[] atomicTraits, int count )
            {
                Debug.Assert( count > 1, "Atomic traits are handled directly." );

                Debug.Assert( !Array.Exists( atomicTraits, mA => mA.Context._c != this || mA.AtomicTraits.Count != 1 ), "Traits are from this Context and they are atomic and not empty." );

                StringBuilder b = new StringBuilder( atomicTraits[0].ToString() );
                for( int i = 1; i < count; ++i )
                {
                    Debug.Assert( StringComparer.Ordinal.Compare( atomicTraits[i - 1].ToString(), atomicTraits[i].ToString() ) < 0, "Traits are already sorted and NO DUPLICATE exists." );
                    b.Append( Separator ).Append( atomicTraits[i].ToString() );
                }
                string traits = b.ToString();
                CKTrait m;
                if( !_traits.TryGetValue( traits, out m ) )
                {
                    // We must clone the array since fall backs generation reuses it.
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
                            m = new CKTrait( new CKTraitContext( this ), traits, atomicTraits );
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
                // Special handling for first and last slots if they are empty.
                if( traits[count - 1].Length == 0 ) count = count - 1 - i;
                else count = count - i;
                if( count != traits.Length )
                {
                    if( count <= 0 ) return Util.Array.Empty<string>();
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

            public override string ToString() => $"CKTraitContext {Name} '{Separator}'";
        }

        /// <summary>
        /// This is basic. And we don't really need more: CKTraitContext are typically created once, statically.
        /// The only "really dynamic" sollicitation of this list will be during deserialization.
        /// </summary>
        static readonly List<Impl> _allContexts;
        static readonly object _basicLock;

        static CKTraitContext()
        {
            _allContexts = new List<Impl>();
            _basicLock = new object();
        }

        readonly Impl _c;

        CKTraitContext( Impl impl ) => _c = impl;

        /// <summary>
        /// Initializes a new context for traits with the given separator.
        /// If a context with the ame name but a different separator has already been created, this
        /// raises a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="name">Name for the context that identifies it. Must not be null nor whitespace.</param>
        /// <param name="separator">Separator (if it must differ from '|').</param>
        public CKTraitContext( string name, char separator = '|' )
        {
            _c = Bind( name, separator );
        }

        static Impl Bind( string name, char separator )
        {
            Impl c, exists;
            lock( _basicLock )
            {
                c = exists = _allContexts.FirstOrDefault( x => x.Name == name );
                if( exists == null ) _allContexts.Add( c = new Impl( name, separator ) );
            }
            if( exists != null && exists.Separator != separator )
            {
                throw new InvalidOperationException( $"CKTraitContext named '{name}' is already defined with the separator '{exists.Separator}', it cannot be redefined with the separator '{separator}'." );
            }
            return c;
        }

        /// <summary>
        /// Reads a <see cref="CKTraitContext"/> that has been previously written by <see cref="Write"/>.
        /// </summary>
        /// <param name="r">The binary reader to use.</param>
        public CKTraitContext( ICKBinaryReader r )
        {
            _c = Bind( r.ReadSharedString(), r.ReadChar() );
        }

        /// <summary>
        /// Writes the <see cref="Name"/> and <see cref="Separator"/> so that <see cref="CKTraitContext(ICKBinaryReader)"/> can
        /// rebuild or rebind to the context.
        /// </summary>
        /// <param name="w">The binary writer to use.</param>
        public void Write( ICKBinaryWriter w )
        {
            w.WriteSharedString( _c.Name );
            w.Write( _c.Separator );
        }

        /// <summary>
        /// Gets the separator to use to separate combined traits. It is | by default.
        /// </summary>
        public char Separator => _c.Separator;

        /// <summary>
        /// Gets the name of this context.
        /// </summary>
        public string Name => _c.Name;

        /// <summary>
        /// Gets the empty trait for this context. It corresponds to the empty string.
        /// </summary>
        public CKTrait EmptyTrait => _c.EmptyTrait;

        /// <summary>
        /// Obtains a <see cref="CKTrait"/> (either combined or atomic).
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by <see cref="Separator"/>.</param>
        /// <returns>A trait.</returns>
        public CKTrait FindOrCreate( string traits ) => _c.FindOrCreate( traits, true );

        /// <summary>
        /// Finds a <see cref="CKTrait"/> (either combined or atomic) only if all 
        /// of its atomic traits already exists: if any of the atomic traits are not already 
        /// registered, null is returned.
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by <see cref="Separator"/>.</param>
        /// <returns>A trait or null if the trait does not exists.</returns>
        public CKTrait FindIfAllExist( string traits ) => _c.FindOrCreate( traits, false );

        /// <summary>
        /// Finds a <see cref="CKTrait"/> with only already existing atomic traits (null when not found).
        /// </summary>
        /// <param name="traits">Atomic trait or traits separated by <see cref="Separator"/>.</param>
        /// <param name="collector">Optional collector for unknown trait. As soon as the collector returns false, the process stops.</param>
        /// <returns>A trait that contains only already existing trait or null if none already exists.</returns>
        public CKTrait FindOnlyExisting( string traits, Func<string, bool> collector = null ) => _c.FindOnlyExisting( traits, collector );

        /// <summary>
        /// Gets the fallback for empty and atomic traits.
        /// </summary>
        internal IEnumerable<CKTrait> EnumWithEmpty => _c.EnumWithEmpty;

        /// <summary>
        /// Obtains a trait from a list of atomic (already sorted) traits.
        /// Used by the Add, Toggle, Remove, Intersect methods.
        /// </summary>
        internal CKTrait FindOrCreate( List<CKTrait> atomicTraits ) => _c.FindOrCreate( atomicTraits );

        /// <summary>
        /// Obtains a trait from a list of atomic (already sorted) traits.
        /// Used by fall back generation.
        /// </summary>
        internal CKTrait FindOrCreate( CKTrait[] atomicTraits, int count ) => _c.FindOrCreate( atomicTraits, count );

        /// <summary>
        /// Compares this context to another one. The keys are <see cref="Separator"/>, then <see cref="Name"/>.
        /// </summary>
        /// <param name="other">Context to compare.</param>
        /// <returns>0 for the exact same context, greater/lower than 0 otherwise.</returns>
        public int CompareTo( CKTraitContext other ) => _c == other._c ? 0 : _c.CompareTo( other._c );

        /// <summary>
        /// Context equality is based on <see cref="Name"/> and <see cref="Separator"/>.
        /// </summary>
        /// <param name="other">The other context.</param>
        /// <returns>True if the 2 contexts are the same, false otherwise.</returns>
        public bool Equals( CKTraitContext other ) => _c == other._c;

        /// <summary>
        /// Overridden to call <see cref="Equals(CKTraitContext)"/>.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the 2 contexts are the same, false otherwise.</returns>
        public override bool Equals( object obj ) => obj is CKTraitContext c ? Equals( c ) : false;

        /// <summary>
        /// Gets the correct hash (<see cref="Separator"/> and <see cref="Name"/> equality).
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode() => _c.GetHashCode();

        /// <summary>
        /// Overridden to return the <see cref="Name"/> and <see cref="Separator"/>.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => _c.ToString();

        /// <summary>
        /// Mimics reference equality: this context lokks like a reference.
        /// </summary>
        /// <param name="c1">The first context.</param>
        /// <param name="c2">The second context.</param>
        /// <returns>True if the the 2 contexts are the same, false otherwise.</returns>
        public static bool operator ==( in CKTraitContext c1, in CKTraitContext c2 ) => c1._c == c2._c;

        /// <summary>
        /// Mimics reference inequality: this context lokks like a reference.
        /// </summary>
        /// <param name="c1">The first context.</param>
        /// <param name="c2">The second context.</param>
        /// <returns>True if the the 2 contexts are different, false when they ar the same.</returns>
        public static bool operator !=( in CKTraitContext c1, in CKTraitContext c2 ) => c1._c != c2._c;

    }
}
