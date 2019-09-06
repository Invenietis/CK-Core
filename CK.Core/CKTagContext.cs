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
    /// Thread-safe registration root for <see cref="CKTag"/> objects.
    /// Each context has a <see cref="Name"/> that uniquely, and definitely, identifies it. Any number of named contexts
    /// can be created in an Application Domain: as long as they define the same <see cref="Separator"/>, they all are actually
    /// the exact same internal context. (If Separator differ during a redefinition, an <see cref="InvalidOperationException"/> is thrown.)
    /// </summary>
    public readonly struct CKTagContext : IComparable<CKTagContext>, IEquatable<CKTagContext>
    {
        /// <summary>
        /// Private implementation: this is the actual class, exposed CKTagContext is just a typed reference.
        /// </summary>
        sealed class Impl : IComparable<Impl>
        {
            readonly Regex _canonize2;
            readonly ConcurrentDictionary<string, CKTag> _tags;
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
                EmptyTag = new CKTag( new CKTagContext( this ) );
                _tags = new ConcurrentDictionary<string, CKTag>( StringComparer.Ordinal );
                _tags[String.Empty] = EmptyTag;
                EnumWithEmpty = new CKTag[] { EmptyTag };
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

            public readonly CKTag EmptyTag;

            public readonly IEnumerable<CKTag> EnumWithEmpty;

            public CKTag FindOnlyExisting( string tags, Func<string, bool> collector = null )
            {
                if( tags == null || tags.Length == 0 ) return null;
                tags = tags.Normalize( NormalizationForm.FormC );
                CKTag m;
                if( !_tags.TryGetValue( tags, out m ) )
                {
                    int tagCount;
                    string[] splitTags = SplitMultiTag( tags, out tagCount );
                    if( tagCount <= 0 ) return null;
                    if( tagCount == 1 )
                    {
                        m = FindOrCreateAtomicTag( splitTags[0], false );
                    }
                    else
                    {
                        tags = String.Join( _separatorString, splitTags, 0, tagCount );
                        if( !_tags.TryGetValue( tags, out m ) )
                        {
                            List<CKTag> atomics = new List<CKTag>();
                            for( int i = 0; i < tagCount; ++i )
                            {
                                CKTag tag = FindOrCreateAtomicTag( splitTags[i], false );
                                if( tag == null )
                                {
                                    if( collector != null && !collector( splitTags[i] ) ) break;
                                }
                                else atomics.Add( tag );
                            }
                            if( atomics.Count != 0 )
                            {
                                tags = String.Join( _separatorString, atomics );
                                if( !_tags.TryGetValue( tags, out m ) )
                                {
                                    lock( _creationLock )
                                    {
                                        if( !_tags.TryGetValue( tags, out m ) )
                                        {
                                            m = new CKTag( new CKTagContext( this ), tags, atomics.ToArray() );
                                            _tags[tags] = m;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return m;
            }

            public CKTag FindOrCreate( string tags, bool create )
            {
                if( tags == null || tags.Length == 0 ) return EmptyTag;
                tags = tags.Normalize();
                if( tags.IndexOfAny( new[] { '\n', '\r' } ) >= 0 ) throw new ArgumentException( Core.Impl.CoreResources.TagsMustNotBeMultiLineString );
                CKTag m;
                if( !_tags.TryGetValue( tags, out m ) )
                {
                    int tagCount;
                    string[] splitTags = SplitMultiTag( tags, out tagCount );
                    if( tagCount <= 0 ) return EmptyTag;
                    if( tagCount == 1 )
                    {
                        m = FindOrCreateAtomicTag( splitTags[0], create );
                    }
                    else
                    {
                        tags = String.Join( _separatorString, splitTags, 0, tagCount );
                        if( !_tags.TryGetValue( tags, out m ) )
                        {
                            CKTag[] atomics = new CKTag[tagCount];
                            for( int i = 0; i < tagCount; ++i )
                            {
                                CKTag tag = FindOrCreateAtomicTag( splitTags[i], create );
                                if( (atomics[i] = tag) == null ) return null;
                            }
                            lock( _creationLock )
                            {
                                if( !_tags.TryGetValue( tags, out m ) )
                                {
                                    m = new CKTag( new CKTagContext( this ), tags, atomics );
                                    _tags[tags] = m;
                                }
                            }
                        }
                        Debug.Assert( !m.IsAtomic && m.AtomicTags.Count == tagCount, "Combined tag." );
                    }
                }
                return m;
            }

            CKTag FindOrCreateAtomicTag( string tag, bool create )
            {
                CKTag m;
                if( !_tags.TryGetValue( tag, out m ) && create )
                {
                    lock( _creationLock )
                    {
                        if( !_tags.TryGetValue( tag, out m ) )
                        {
                            m = new CKTag( new CKTagContext( this ), tag );
                            _tags[tag] = m;
                        }
                    }
                    Debug.Assert( m.IsAtomic, "Special construction for atomic tags." );
                }
                return m;
            }

            public CKTag FindOrCreate( List<CKTag> atomicTags )
            {
                if( atomicTags.Count == 0 ) return EmptyTag;
                Debug.Assert( atomicTags[0].Context._c == this, "This is one of our tags." );
                Debug.Assert( atomicTags[0].AtomicTags.Count == 1, "This is an atomic tag and not the empty one." );
                if( atomicTags.Count == 1 ) return atomicTags[0];
                StringBuilder b = new StringBuilder( atomicTags[0].ToString() );
                for( int i = 1; i < atomicTags.Count; ++i )
                {
                    Debug.Assert( atomicTags[i].Context._c == this, "This is one of our tags." );
                    Debug.Assert( atomicTags[i].AtomicTags.Count == 1, "This is an atomic tag and not the empty one." );
                    Debug.Assert( StringComparer.Ordinal.Compare( atomicTags[i - 1].ToString(), atomicTags[i].ToString() ) < 0,
                        "Tags are already sorted and NO DUPLICATES exist." );
                    b.Append( Separator ).Append( atomicTags[i].ToString() );
                }
                string tags = b.ToString();
                CKTag m;
                if( !_tags.TryGetValue( tags, out m ) )
                {
                    lock( _creationLock )
                    {
                        if( !_tags.TryGetValue( tags, out m ) )
                        {
                            m = new CKTag( new CKTagContext( this ), tags, atomicTags.ToArray() );
                            _tags[tags] = m;
                        }
                    }
                }
                return m;
            }

            public CKTag FindOrCreate( CKTag[] atomicTags, int count )
            {
                Debug.Assert( count > 1, "Atomic tags are handled directly." );

                Debug.Assert( !Array.Exists( atomicTags, mA => mA.Context._c != this || mA.AtomicTags.Count != 1 ), "Tags are from this Context and they are atomic and not empty." );

                StringBuilder b = new StringBuilder( atomicTags[0].ToString() );
                for( int i = 1; i < count; ++i )
                {
                    Debug.Assert( StringComparer.Ordinal.Compare( atomicTags[i - 1].ToString(), atomicTags[i].ToString() ) < 0, "Tags are already sorted and NO DUPLICATE exists." );
                    b.Append( Separator ).Append( atomicTags[i].ToString() );
                }
                string tags = b.ToString();
                CKTag m;
                if( !_tags.TryGetValue( tags, out m ) )
                {
                    // We must clone the array since fall backs generation reuses it.
                    if( atomicTags.Length != count )
                    {
                        CKTag[] subArray = new CKTag[count];
                        Array.Copy( atomicTags, subArray, count );
                        atomicTags = subArray;
                    }
                    else atomicTags = (CKTag[])atomicTags.Clone();
                    lock( _creationLock )
                    {
                        if( !_tags.TryGetValue( tags, out m ) )
                        {
                            m = new CKTag( new CKTagContext( this ), tags, atomicTags );
                            _tags[tags] = m;
                        }
                    }
                }
                return m;
            }

            string[] SplitMultiTag( string s, out int count )
            {
                string[] tags = _canonize2.Split( s.Trim() );
                count = tags.Length;
                Debug.Assert( count != 0, "Split always create a cell." );
                int i = tags[0].Length == 0 ? 1 : 0;
                // Special handling for first and last slots if they are empty.
                if( tags[count - 1].Length == 0 ) count = count - 1 - i;
                else count = count - i;
                if( count != tags.Length )
                {
                    if( count <= 0 ) return Util.Array.Empty<string>();
                    string[] m = new string[count];
                    Array.Copy( tags, i, m, 0, count );
                    tags = m;
                }
                // Sort if necessary (more than one atomic tag).
                if( count > 1 )
                {
                    Array.Sort( tags, StringComparer.Ordinal );
                    // And removes duplicates. Since this occur very rarely
                    // and that count is small we use a O(n) process that shifts
                    // the tags array.
                    i = count - 1;
                    string last = tags[i];
                    while( --i >= 0 )
                    {
                        Debug.Assert( last.Length > 0, "There is no empty strings." );
                        string cur = tags[i];
                        if( StringComparer.Ordinal.Equals( cur, last ) )
                        {
                            int delta = (--count) - i - 1;
                            if( delta > 0 )
                            {
                                Array.Copy( tags, i + 2, tags, i + 1, delta );
                            }
                        }
                        last = cur;
                    }
                }
                return tags;
            }

            public override string ToString() => $"CKTagContext {Name} '{Separator}'";
        }

        /// <summary>
        /// This is basic. And we don't really need more: CKTagContext are typically created once, statically.
        /// The only "really dynamic" sollicitation of this list will be during deserialization.
        /// </summary>
        static readonly List<Impl> _allContexts;
        static readonly object _basicLock;

        static CKTagContext()
        {
            _allContexts = new List<Impl>();
            _basicLock = new object();
        }

        readonly Impl _c;

        CKTagContext( Impl impl ) => _c = impl;

        /// <summary>
        /// Initializes a new context for tags with the given separator.
        /// If a context with the ame name but a different separator has already been created, this
        /// raises a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="name">Name for the context that identifies it. Must not be null nor whitespace.</param>
        /// <param name="separator">Separator (if it must differ from '|').</param>
        public CKTagContext( string name, char separator = '|' )
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
                throw new InvalidOperationException( $"CKTagContext named '{name}' is already defined with the separator '{exists.Separator}', it cannot be redefined with the separator '{separator}'." );
            }
            return c;
        }

        /// <summary>
        /// Reads a <see cref="CKTagContext"/> that has been previously written by <see cref="Write"/>.
        /// </summary>
        /// <param name="r">The binary reader to use.</param>
        public CKTagContext( ICKBinaryReader r )
        {
            _c = Bind( r.ReadSharedString(), r.ReadChar() );
        }

        /// <summary>
        /// Writes the <see cref="Name"/> and <see cref="Separator"/> so that <see cref="CKTagContext(ICKBinaryReader)"/> can
        /// rebuild or rebind to the context.
        /// </summary>
        /// <param name="w">The binary writer to use.</param>
        public void Write( ICKBinaryWriter w )
        {
            w.WriteSharedString( _c.Name );
            w.Write( _c.Separator );
        }

        /// <summary>
        /// Gets the separator to use to separate combined tags. It is | by default.
        /// </summary>
        public char Separator => _c.Separator;

        /// <summary>
        /// Gets the name of this context.
        /// </summary>
        public string Name => _c.Name;

        /// <summary>
        /// Gets the empty tag for this context. It corresponds to the empty string.
        /// </summary>
        public CKTag EmptyTag => _c.EmptyTag;

        /// <summary>
        /// Obtains a <see cref="CKTag"/> (either combined or atomic).
        /// </summary>
        /// <param name="tags">Atomic tag or tags separated by <see cref="Separator"/>.</param>
        /// <returns>A tag.</returns>
        public CKTag FindOrCreate( string tags ) => _c.FindOrCreate( tags, true );

        /// <summary>
        /// Finds a <see cref="CKTag"/> (either combined or atomic) only if all 
        /// of its atomic tags already exists: if any of the atomic tags are not already 
        /// registered, null is returned.
        /// </summary>
        /// <param name="tags">Atomic tag or tags separated by <see cref="Separator"/>.</param>
        /// <returns>A tag or null if the tag does not exists.</returns>
        public CKTag FindIfAllExist( string tags ) => _c.FindOrCreate( tags, false );

        /// <summary>
        /// Finds a <see cref="CKTag"/> with only already existing atomic tags (null when not found).
        /// </summary>
        /// <param name="tags">Atomic tag or tags separated by <see cref="Separator"/>.</param>
        /// <param name="collector">Optional collector for unknown tag. As soon as the collector returns false, the process stops.</param>
        /// <returns>A tag that contains only already existing tag or null if none already exists.</returns>
        public CKTag FindOnlyExisting( string tags, Func<string, bool> collector = null ) => _c.FindOnlyExisting( tags, collector );

        /// <summary>
        /// Gets the fallback for empty and atomic tags.
        /// </summary>
        internal IEnumerable<CKTag> EnumWithEmpty => _c.EnumWithEmpty;

        /// <summary>
        /// Obtains a tag from a list of atomic (already sorted) tags.
        /// Used by the Add, Toggle, Remove, Intersect methods.
        /// </summary>
        internal CKTag FindOrCreate( List<CKTag> atomicTags ) => _c.FindOrCreate( atomicTags );

        /// <summary>
        /// Obtains a tag from a list of atomic (already sorted) tags.
        /// Used by fall back generation.
        /// </summary>
        internal CKTag FindOrCreate( CKTag[] atomicTags, int count ) => _c.FindOrCreate( atomicTags, count );

        /// <summary>
        /// Compares this context to another one. The keys are <see cref="Separator"/>, then <see cref="Name"/>.
        /// </summary>
        /// <param name="other">Context to compare.</param>
        /// <returns>0 for the exact same context, greater/lower than 0 otherwise.</returns>
        public int CompareTo( CKTagContext other ) => _c == other._c ? 0 : _c.CompareTo( other._c );

        /// <summary>
        /// Context equality is based on <see cref="Name"/> and <see cref="Separator"/>.
        /// </summary>
        /// <param name="other">The other context.</param>
        /// <returns>True if the 2 contexts are the same, false otherwise.</returns>
        public bool Equals( CKTagContext other ) => _c == other._c;

        /// <summary>
        /// Overridden to call <see cref="Equals(CKTagContext)"/>.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the 2 contexts are the same, false otherwise.</returns>
        public override bool Equals( object obj ) => obj is CKTagContext c ? Equals( c ) : false;

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
        public static bool operator ==( in CKTagContext c1, in CKTagContext c2 ) => c1._c == c2._c;

        /// <summary>
        /// Mimics reference inequality: this context lokks like a reference.
        /// </summary>
        /// <param name="c1">The first context.</param>
        /// <param name="c2">The second context.</param>
        /// <returns>True if the the 2 contexts are different, false when they ar the same.</returns>
        public static bool operator !=( in CKTagContext c1, in CKTagContext c2 ) => c1._c != c2._c;

    }
}
