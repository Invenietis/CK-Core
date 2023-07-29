using CommunityToolkit.HighPerformance;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Captures an interpolated string result along with its placeholders and
    /// provides the composite format string (https://learn.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting)
    /// that can be used as a template for other placeholder values.
    /// <para>
    /// This is implicitly castable as a string: <see cref="Text"/> is returned.
    /// </para>
    /// <para>
    /// Note: We don't use <see cref="Range"/> here because there's no use of any "FromEnd". a simple
    /// value tuple <c>(int Start, int Length)</c> is easier and faster.
    /// </para>
    /// </summary>
    [SerializationVersion( 0 )]
    public sealed class FormattedString : ICKSimpleBinarySerializable, ICKVersionedBinarySerializable
    {
        static ReadOnlySpan<char> _braces => "{}";

        readonly string _text;
        readonly (int Start, int Length)[] _slots;
        readonly CultureInfo _culture;

        /// <summary>
        /// Gets a <see cref="CultureInfo.InvariantCulture"/>, empty message (empty object pattern).
        /// </summary>
        public static readonly FormattedString InvariantEmpty = new FormattedString( CultureInfo.InvariantCulture, string.Empty );

        /// <summary>
        /// Initializes a <see cref="FormattedString"/> with a plain string (no <see cref="Placeholders"/>)
        /// that is bound to the <see cref="CultureInfo.CurrentCulture"/> (that is a thread static property).
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        public FormattedString( string plainText )
            : this( CultureInfo.CurrentCulture, plainText )
        {
        }

        /// <summary>
        /// Initializes a <see cref="FormattedString"/> with a plain string (no <see cref="Placeholders"/>).
        /// </summary>
        /// <param name="culture">The culture of this formatted string.</param>
        /// <param name="plainText">The plain text.</param>
        public FormattedString( CultureInfo culture, string plainText )
        {
            Throw.CheckNotNullArgument( plainText );
            _text = plainText;
            _slots = Array.Empty<(int, int)>();
            _culture = culture;
            Debug.Assert( CheckPlaceholders( _slots, _text.Length ) );
        }

        /// <summary>
        /// Initializes a <see cref="FormattedString"/> with <see cref="Placeholders"/> using
        /// the thread static <see cref="CultureInfo.CurrentCulture"/> to format the placeholder contents.
        /// </summary>
        /// <param name="text">The interpolated text.</param>
        public FormattedString( [InterpolatedStringHandlerArgument] FormattedStringHandler text )
        {
            _culture = CultureInfo.CurrentCulture;
            (_text, _slots) = text.GetResult();
            Debug.Assert( CheckPlaceholders( _slots, _text.Length ) );
        }

        /// <summary>
        /// Initializes a <see cref="FormattedString"/> with <see cref="Placeholders"/> using
        /// the provided <paramref name="culture"/>.
        /// </summary>
        /// <param name="culture">The culture used to format placeholders' content.</param>
        /// <param name="text">The interpolated text.</param>
        public FormattedString( CultureInfo culture, [InterpolatedStringHandlerArgument( nameof( culture ) )] FormattedStringHandler text )
        {
            (_text,_slots) = text.GetResult();
            _culture = culture;
            Debug.Assert( CheckPlaceholders( _slots, _text.Length ) );
        }

        FormattedString( string text, (int Start, int Length)[] placeholders, CultureInfo culture )
        {
            _text = text;
            _slots = placeholders;
            _culture = culture;
        }

        /// <summary>
        /// Creates a <see cref="FormattedString"/>. This is intended to restore an instance from its component:
        /// this can typically be used by serializers/deserializers.
        /// <para>
        /// All parameters are checked (placeholders cannot overlap or cover more than the text).
        /// </para>
        /// </summary>
        /// <param name="text">The <see cref="Text"/>.</param>
        /// <param name="placeholders">The <see cref="Placeholders"/>.</param>
        /// <param name="culture">The <see cref="Culture"/>.</param>
        /// <returns>A new formatted string.</returns>
        public static FormattedString Create( string text, (int Start, int Length)[] placeholders, CultureInfo culture )
        {
            Throw.CheckNotNullArgument( text );
            Throw.CheckNotNullArgument( placeholders );
            Throw.CheckNotNullArgument( culture );
            Throw.CheckArgument( CheckPlaceholders( placeholders, text.Length ) );
            return new FormattedString( text, placeholders, culture );
        }

        /// <summary>
        /// Intended for wrappers that capture the interpolated string handler.
        /// </summary>
        /// <param name="handler">The interpolated string handler.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>A new formatted string.</returns>
        public static FormattedString Create( ref FormattedStringHandler handler, CultureInfo culture )
        {
            Throw.CheckNotNullArgument( culture );
            var (t, p) = handler.GetResult();
            return new FormattedString( t, p, culture );
        }

        static bool CheckPlaceholders( (int Start, int Length)[] placeholders, int lenText )
        {
            int last = 0;
            foreach( var p in placeholders )
            {
                if( p.Start < 0 || p.Length < 0 || last > p.Start ) return false;
                last = p.Start + p.Length;
            }
            return last <= lenText;
        }

        /// <summary>
        /// Gets this formatted string content.
        /// </summary>
        public string Text => _text;

        /// <summary>
        /// Gets whether this message is empty: <see cref="Text"/> is empty.
        /// <para>
        /// Note that there may be one or more <see cref="Placeholders"/>... but they are all
        /// empty.
        /// </para>
        /// </summary>
        public bool IsEmpty => _text.Length == 0;

        /// <summary>
        /// Gets the placeholders' occurrence in this <see cref="Text"/>.
        /// </summary>
        public IReadOnlyList<(int Start, int Length)> Placeholders => _slots;

        /// <summary>
        /// Gets the placeholders' content.
        /// </summary>
        /// <returns>A formatted content (with <see cref="Culture"/>) for each placeholders.</returns>
        public IEnumerable<ReadOnlyMemory<char>> GetPlaceholderContents()
        {
            foreach( var (Start, Length) in _slots )
            {
                yield return _text.AsMemory( Start, Length );
            }
        }

        /// <summary>
        /// Gets the culture that has been used to format the placeholder's content.
        /// <para>
        /// When deserializing, this culture is set to the <see cref="CultureInfo.InvariantCulture"/> if
        /// the culture cannot be restored properly.
        /// </para>
        /// </summary>
        public CultureInfo Culture => _culture;

        /// <summary>
        /// Implicit cast into string.
        /// </summary>
        /// <param name="f">This formatted string.</param>
        public static implicit operator string( FormattedString f ) => f._text;

        /// <summary>
        /// Returns a <see cref="string.Format(IFormatProvider?, string, object?[])"/> composite format string
        /// with positional placeholders {0}, {1} etc. for each placeholder.
        /// <para>
        /// The purpose of this format string is not to rewrite this message with other contents, it is to ease globalization
        /// process by providing the message's format in order to translate it into different languages.
        /// </para>
        /// </summary>
        /// <returns>The composite format string.</returns>
        public string GetFormatString()
        {
            if( _slots.Length == 0 ) return _text.Replace( "{", "{{" ).Replace( "}", "}}" );
            Debug.Assert( _slots.Length < 100 );
            // Worst case is full of { and } (that must be doubled) and all slots are empty
            // (that must be filled with {xx}: it is useless to handle the 10 first {x} placeholders).
            // Note: It is enough to blindly double { and } (https://github.com/dotnet/docs/issues/36416).
            var fmtA = ArrayPool<char>.Shared.Rent( _text.Length * 2 + _slots.Length * 4 );
            var fmt = fmtA.AsSpan();
            int fHead = 0;
            var text = _text.AsSpan();
            int tStart = 0;
            int cH = '0';
            int cL = '0';
            foreach( var slot in _slots )
            {
                int lenBefore = slot.Start - tStart;
                var before = text.Slice( tStart, lenBefore );
                tStart = slot.Start + slot.Length;
                CopyWithDoubledBraces( before, fmt, ref fHead );
                fmt[fHead++] = '{';
                if( cH > '0' ) fmt[fHead++] = (char)cH;
                fmt[fHead++] = (char)cL;
                if( ++cL == ':' )
                {
                    cL = '0';
                    ++cH;
                }
                fmt[fHead++] = '}';
            }
            CopyWithDoubledBraces( text.Slice( tStart ), fmt, ref fHead );
            var s = new string( fmt.Slice( 0, fHead ) );
            ArrayPool<char>.Shared.Return( fmtA );
            return s;

            static void CopyWithDoubledBraces( ReadOnlySpan<char> before, Span<char> fmt, ref int fHead )
            {
                int iB = before.IndexOfAny( _braces );
                while( iB >= 0 )
                {
                    var b = before[iB];
                    before.Slice( 0, ++iB ).CopyTo( fmt.Slice( fHead ) );
                    fHead += iB;
                    fmt[fHead++] = b;
                    before = before.Slice( iB );
                    iB = before.IndexOfAny( _braces );
                }
                before.CopyTo( fmt.Slice( fHead ) );
                fHead += before.Length;
            }
        }

        /// <summary>
        /// Overridden to return this <see cref="Text"/>.
        /// </summary>
        /// <returns>This text.</returns>
        public override string ToString() => _text;

        #region Serialization
        /// <summary>
        /// Simple deserialization constructor.
        /// </summary>
        /// <param name="r">The reader.</param>
        public FormattedString( ICKBinaryReader r )
            : this( r, r.ReadNonNegativeSmallInt32() )
        {
        }

        /// <inheritdoc />
        public void Write( ICKBinaryWriter w )
        {
            Debug.Assert( SerializationVersionAttribute.GetRequiredVersion( GetType() ) == 0 );
            w.WriteNonNegativeSmallInt32( 0 );
            WriteData( w );
        }

        /// <summary>
        /// Versioned deserialization constructor.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="version">The saved version number.</param>
        public FormattedString( ICKBinaryReader r, int version )
        {
            Throw.CheckData( version == 0 );
            _text = r.ReadString();
            int count = r.ReadNonNegativeSmallInt32();
            Throw.CheckData( count < 100 );
            _slots = new (int Start, int Length)[count];
            for( int i = 0; i < count; ++i )
            {
                ref var s = ref _slots[i];
                s.Start = r.ReadNonNegativeSmallInt32();
                s.Length = r.ReadNonNegativeSmallInt32();
            }
            var n = r.ReadString();
            // First idea was to throw if the culture cannot be found but it seems
            // a better idea to never throw at this level...
            // Since we have no IActivityMonitor here, we use the .Net Trace framework: the
            // GrandOutput will eventually receive it.
            // If there's only the invariant culture, we also avoid the exception.
            if( !Util.IsGlobalizationInvariantMode )
            {
                try
                {
                    // don't use predefinedOnly: true overload here.
                    // If it happens that a culture is not predefined (Nls for windows, Icu on linux)
                    // this has less chance to throw.
                    _culture = CultureInfo.GetCultureInfo( n );
                }
                catch( CultureNotFoundException )
                {
                    _culture = CultureInfo.InvariantCulture;
                    Trace.TraceError( $"CultureInfo named '{n}' cannot be resolved. Using InvariantCulture for FormattedString '{_text}'." );
                }
            }
            else
            {
                _culture = CultureInfo.InvariantCulture;
            }
        }


        /// <inheritdoc />
        public void WriteData( ICKBinaryWriter w )
        {
            // Don't bother optimizing the InvariantEmpty as it should not be used
            // frequently and if it is, only the caller can serialize a marker and
            // deserialize the singleton.
            w.Write( _text );
            w.WriteNonNegativeSmallInt32( _slots.Length );
            foreach( var (start, length) in _slots )
            {
                w.WriteNonNegativeSmallInt32( start );
                w.WriteNonNegativeSmallInt32( length );
            }
            w.Write( _culture.Name );
        }
        #endregion

    }
}
