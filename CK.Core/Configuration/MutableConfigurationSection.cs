using CK.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CK.Core
{
    /// <summary>
    /// Mutable <see cref="IConfigurationSection"/>: this acts as a simple configuration builder
    /// that can then be captured by a <see cref="ImmutableConfigurationSection"/>.
    /// <para>
    /// This supports direct <see cref="AddJson(string, IUtf8JsonReaderContext?, bool)"/> (that can be json
    /// with comments).
    /// </para>
    /// <para>
    /// Methods from <see cref="IConfigurationSection"/> are explicitly implemented, all their "mutable"
    /// equivalent exist: <see cref="GetMutableSection(string)"/>, <see cref="GetMutableChildren()"/>.
    /// </para>
    /// </summary>
    public sealed class MutableConfigurationSection : IConfigurationSection
    {
        readonly string _key;
        readonly string _path;
        string? _value;
        readonly List<MutableConfigurationSection> _children;
        MutableConfigurationSection? _withValue;

        /// <summary>
        /// Initializes a new <see cref="MutableConfigurationSection"/>.
        /// </summary>
        /// <param name="path">The section path. It must be <see cref="IsValidPath(ReadOnlySpan{char})"/>.</param>
        public MutableConfigurationSection( string path )
        {
            _path = CheckKeyArgument( path );
            _key = ConfigurationPath.GetSectionKey( path );
            _children = new List<MutableConfigurationSection>();
        }

        /// <summary>
        /// Initializes a new <see cref="MutableConfigurationSection"/> with a
        /// <see cref="IConfigurationSection.Path"/> that is "<paramref name="parentPath"/>:<paramref name="key"/>".
        /// </summary>
        /// <param name="parentPath">The parent section path. It must be null or empty or <see cref="IsValidPath(ReadOnlySpan{char})"/>.</param>
        /// <param name="key">The key name. It must be <see cref="IsValidKey(ReadOnlySpan{char})"/>.</param>
        public MutableConfigurationSection( string? parentPath, string key )
        {
            _key = CheckKeyArgument( key );
            if( string.IsNullOrEmpty( parentPath ) )
            {
                _path = _key;
            }
            else
            {
                _path = CheckPathArgument( parentPath ) + ':' + key;
            }
            _children = new List<MutableConfigurationSection>();
        }

        /// <summary>
        /// Initializes a new <see cref="ImmutableConfigurationSection"/>.
        /// </summary>
        /// <param name="section">The section to capture.</param>
        public MutableConfigurationSection( IConfigurationSection section )
            : this( section, null )
        {
        }

        MutableConfigurationSection( IConfigurationSection section, MutableConfigurationSection? withValue )
        {
            Throw.CheckNotNullArgument( section );
            Debug.Assert( ConfigurationPath.KeyDelimiter == ":" );
            _key = section.Key;
            _path = section.Path;
            _value = section.Value;
            _withValue = withValue ?? (_value != null ? this : null);
            _children = section.GetChildren().Select( c => new MutableConfigurationSection( c, _withValue ) ).ToList();
        }


        MutableConfigurationSection( MutableConfigurationSection parent, string key )
        {
            Throw.DebugAssert( !key.Contains( ':' ) && IsValidKey( key ) );
            _key = key;
            _path = parent._path + ':' + key;
            _withValue = parent._withValue;
            _children = new List<MutableConfigurationSection>();
        }

        /// <summary>
        /// Gets a configuration value.
        /// Setting a value is possible only if no subordinated children exists below the key. 
        /// </summary>
        /// <param name="path">The configuration key or a path to a subordinated key.</param>
        /// <returns>The value or null if not found.</returns>
        public string? this[string path]
        {
            get
            {
                var sPath = path.AsSpan();
                var parent = this;
                return Find( ref sPath, ref parent )?.Value;
            }
            set => GetMutableSection( path ).Value = value;
        }

        /// <inheritdoc />
        public string Key => _key;

        /// <inheritdoc />
        public string Path => _path;

        /// <summary>
        /// Gets the section value. Setting it clears the <see cref="GetMutableChildren()"/> collection.
        /// Setting a null value makes this section empty: <see cref="ConfigurationExtensions.Exists(IConfigurationSection)"/>
        /// becomes false.
        /// </summary>
        public string? Value
        {
            get => _value;
            set
            {
                if( _value != value )
                {
                    if( _value == null )
                    {
                        Debug.Assert( value != null );
                        if( _withValue != null && _withValue != this )
                        {
                            Throw.InvalidOperationException( $"Unable to set '{_path}' value to '{value}' since '{_withValue._path}' above has value '{_withValue._value}'." );
                        }
                        SetWithValue( this, value );
                    }
                    else if( value == null )
                    {
                        Debug.Assert( _value != null && _withValue == this );
                        ClearWithValue();
                    }
                    _value = value;
                }
            }
        }

        void ClearWithValue()
        {
            _withValue = null;
            foreach( var c in _children ) c.ClearWithValue();
        }

        void SetWithValue( MutableConfigurationSection section, string value )
        {
            _withValue = section;
            foreach( var c in _children )
            {
                if( c._value != null )
                {
                    Throw.InvalidOperationException( $"Unable to set '{section.Path}' value to '{value}' since at least '{c._path}' (with value '{c._value}') exists below." );
                }
                c.SetWithValue( section, value );
            }
        }

        bool InDepthExists()
        {
            if( _value != null ) return true;
            foreach( var c in _children )
                if( c.InDepthExists() ) return true;
            return false;
        }

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() => _children.Where( c => c.InDepthExists() );

        /// <summary>
        /// Gets the immediate descendant <see cref="MutableConfigurationSection"/> sub-sections: they can
        /// have no value and no children (<see cref="ConfigurationExtensions.Exists(IConfigurationSection)"/> can be false).
        /// </summary>
        /// <returns>The configuration sub-sections.</returns>
        public IReadOnlyList<MutableConfigurationSection> GetMutableChildren() => _children;

        IConfigurationSection IConfiguration.GetSection( string path ) => GetMutableSection( path );

        /// <summary>
        /// Finds or creates an existing subordinated section. The key can contain ":" delimiters: sub sections
        /// are found or created accordingly.
        /// <para>
        /// If the section is created, it has no value (<see cref="Value"/> is null) and no children
        /// (<see cref="ConfigurationExtensions.Exists(IConfigurationSection)"/> is false).
        /// </para>
        /// </summary>
        /// <param name="path">The section path relative to this <see cref="Path"/>.</param>
        /// <returns>The mutable section.</returns>
        public MutableConfigurationSection GetMutableSection( string path )
        {
            var sKey = path.AsSpan();
            var parent = this;
            var s = Find( ref sKey, ref parent );
            if( s != null ) return s;
            // Here, instead of reproducing the standard .Net implementation behavior,
            // we check the key syntax and ensure the path to target.
            CheckPathArgument( path );
            // We don't check here that a value exists here or above: getting a mutable
            // (empty) section is always possible.
            int idx;
            if( (idx = sKey.IndexOf( ':' )) < 0 )
            {
                Debug.Assert( (parent == this) == (sKey.Length == path.Length) );
                // Sets the adjusted key (to the new parent) if needed.
                if( parent != this )
                {
                    path = sKey.ToString();
                }
            }
            else
            {
                do
                {
                    path = sKey.Slice( 0, idx ).ToString();
                    s = new MutableConfigurationSection( parent, path );
                    parent._children.Add( s );
                    sKey = sKey.Slice( idx + 1 );
                    parent = s;
                }
                while( (idx = sKey.IndexOf( ':' )) != -1 );
                path = sKey.ToString();
            }
            s = new MutableConfigurationSection( parent, path );
            parent._children.Add( s );
            return s;
        }

        internal static string CheckKeyArgument( string key )
        {
            if( !IsValidKey( key ) )
            {
                if( key == null ) Throw.ArgumentNullException( "key" );
                Throw.ArgumentException( "key", $"Configuration key '{key}' is invalid." );
            }
            return key;
        }

        internal static string CheckPathArgument( string path )
        {
            if( !IsValidPath( path ) )
            {
                if( path == null ) Throw.ArgumentNullException( "path" );
                Throw.ArgumentException( "path", $"Configuration path '{path}' is invalid." );
            }
            return path;
        }

        /// <summary>
        /// Checks whether a <see cref="IConfigurationSection.Key"/> is valid.
        /// It must not be empty and not contain key delimiter ':' (<see cref="ConfigurationPath.KeyDelimiter"/>).
        /// </summary>
        /// <param name="sKey">The key to check.</param>
        /// <returns>Whether the key is valid.</returns>
        public static bool IsValidKey( ReadOnlySpan<char> sKey )
        {
            return sKey.Length > 0 && !sKey.Contains( ':' );
        }

        /// <summary>
        /// Checks whether a <see cref="IConfigurationSection.Path"/> is valid.
        /// It must not be empty and may contain non empty keys delimited by colon (':').
        /// </summary>
        /// <param name="sPath">The path to check.</param>
        /// <returns>Whether the path is valid.</returns>
        public static bool IsValidPath( ReadOnlySpan<char> sPath )
        {
            return sPath.Length > 0
                   && !sPath.Contains( "::".AsSpan(), StringComparison.Ordinal )
                   && sPath[0] != ':'
                   && sPath[sPath.Length - 1] != ':';
        }

        /// <summary>
        /// Adds a set of configuration values to this configuration.
        /// </summary>
        /// <param name="values">Configuration values to add.</param>
        public void Add( IEnumerable<KeyValuePair<string, string?>> values )
        {
            foreach( var kv in values)
            {
                GetMutableSection( kv.Key ).Value = kv.Value;
            }
        }

        /// <summary>
        /// Reads a JSON configuration string object and adds or updates all the corresponding sections and values.
        /// <para>
        /// Comments can exists and are ignored.
        /// </para>
        /// </summary>
        /// <param name="configuration">The Json configuration string.</param>
        /// <param name="checkPropertyNameUnicity">Optionally allow duplicate property names to appear: last occurrence wins.</param>
        /// <returns>This section.</returns>
        public MutableConfigurationSection AddJson( string configuration, bool checkPropertyNameUnicity = true )
        {
            var r = new Utf8JsonReader( Encoding.UTF8.GetBytes( configuration ), new JsonReaderOptions() { AllowTrailingCommas = true } );
            return AddJson( ref r, IUtf8JsonReaderContext.Empty, checkPropertyNameUnicity );
        }

        /// <summary>
        /// Reads a JSON object and adds or updates all the corresponding sections and values.
        /// The reader must positionned on <see cref="JsonTokenType.None"/> or <see cref="JsonTokenType.StartObject"/>.
        /// <para>
        /// Any <see cref="JsonTokenType.Comment"/> are skipped (if <see cref="JsonReaderOptions.CommentHandling"/> is <see cref="JsonCommentHandling.Allow"/>).
        /// </para>
        /// </summary>
        /// <param name="r">The Json reader.</param>
        /// <param name="context">Optional context. Defaults to <see cref="IUtf8JsonReaderContext.Empty"/>.</param>
        /// <param name="checkPropertyNameUnicity">Optionally allow duplicate property names to appear: last occurrence wins.</param>
        /// <returns>This section.</returns>
        public MutableConfigurationSection AddJson( ref Utf8JsonReader r, IUtf8JsonReaderContext? context = null, bool checkPropertyNameUnicity = true )
        {
            AddJson( ref r, context ?? IUtf8JsonReaderContext.Empty, this, checkPropertyNameUnicity );
            return this;
        }

        static void AddJson( ref Utf8JsonReader r, IUtf8JsonReaderContext context, MutableConfigurationSection target, bool checkPropertyNameUnicity )
        {
            if( r.TokenType == JsonTokenType.None && !r.Read() ) return;
            while( r.TokenType == JsonTokenType.Comment ) r.Read();
            Throw.CheckData( r.TokenType == JsonTokenType.StartObject );
            ReadObject( ref r, context, target, checkPropertyNameUnicity );

            static void ReadObject( ref Utf8JsonReader r, IUtf8JsonReaderContext context, MutableConfigurationSection target, bool checkPropertyNameUnicity )
            {
                Debug.Assert( r.TokenType == JsonTokenType.StartObject );
                r.ReadWithMoreData( context );
                r.SkipComments( context );
                var names = checkPropertyNameUnicity ? new HashSet<string>() : null;
                while( r.TokenType == JsonTokenType.PropertyName )
                {
                    var propertyName = r.GetString();
                    Throw.CheckData( propertyName != null );
                    if( names != null && !names.Add( propertyName ) )
                    {
                        Throw.InvalidDataException( $"Duplicate JSON property '{propertyName}'." );
                    }
                    var t = target.GetMutableSection( propertyName );
                    r.ReadWithMoreData( context );
                    r.SkipComments( context );
                    ReadValue( ref r, context, t, checkPropertyNameUnicity );
                    r.ReadWithMoreData( context );
                    r.SkipComments( context );
                }

                static void ReadValue( ref Utf8JsonReader r, IUtf8JsonReaderContext context, MutableConfigurationSection t, bool checkPropertyNameUnicity )
                {
                    switch( r.TokenType )
                    {
                        case JsonTokenType.String:
                            t.Value = r.GetString();
                            break;
                        case JsonTokenType.True:
                            t.Value = "True";
                            break;
                        case JsonTokenType.False:
                            t.Value = "False";
                            break;
                        case JsonTokenType.Number:
                            t.Value = r.HasValueSequence ? Encoding.UTF8.GetString( r.ValueSequence ) : Encoding.UTF8.GetString( r.ValueSpan );
                            break;
                        case JsonTokenType.StartObject:
                            ReadObject( ref r, context, t, checkPropertyNameUnicity );
                            break;
                        case JsonTokenType.StartArray:
                            r.ReadWithMoreData( context );
                            while( r.TokenType == JsonTokenType.Comment ) r.Read();
                            int index = 0;
                            while( r.TokenType != JsonTokenType.EndArray )
                            {
                                ReadValue( ref r, context, t.GetMutableSection( index.ToString() ), checkPropertyNameUnicity );
                                r.ReadWithMoreData( context );
                                r.SkipComments( context );
                                index++;
                            }
                            break;
                        case JsonTokenType.Null:
                            t.Value = null;
                            break;
                        default:
                            Throw.InvalidDataException( $"Unexpected token '{r.TokenType}'." );
                            break;
                    }
                }
            }
        }


        static MutableConfigurationSection? Find( ref ReadOnlySpan<char> sKey, ref MutableConfigurationSection parent )
        {
            for(; ; )
            {
                var idx = sKey.IndexOf( ':' );
                if( idx < 0 ) return FindCore( sKey, parent._children );
                var sub = FindCore( sKey.Slice( 0, idx ), parent._children );
                if( sub == null ) return null;
                sKey = sKey.Slice( idx + 1 );
                parent = sub;
            }

            static MutableConfigurationSection? FindCore( ReadOnlySpan<char> sKey, List<MutableConfigurationSection> children )
            {
                foreach( var child in children )
                {
                    if( sKey.Equals( child.Key, StringComparison.OrdinalIgnoreCase ) ) return child;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the never changing <see cref="Util.NoChangeToken"/>.
        /// </summary>
        /// <returns>A never changing token.</returns>
        public IChangeToken GetReloadToken() => Util.NoChangeToken;


        /// <summary>
        /// Overridden to display the path and the value or the count of children.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => $"{_path} = {(_value ?? (_children.Count != 0 ? $"{_children.Count} children" : "!Exists"))}";

    }
}
