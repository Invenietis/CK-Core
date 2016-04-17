using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// This class supports "Match and Forward" pattern.
    /// On a failed match, the <see cref="SetError"/> method sets the <see cref="ErrorMessage"/>.
    /// On a successful match, the <see cref="StartIndex"/> is updated by a call to <see cref="Forward"/> so that 
    /// the <see cref="Head"/> is positioned after the match (and any existing error is cleared).
    /// There are 2 main kind of methods: TryMatchXXX that when the match fails returns false but do not call 
    /// <see cref="SetError"/>and MatchXXX that do set an error on failure.
    /// This class does not actually hide/encapsulate a lot of things: it is designed to be extended through 
    /// extension methods.
    /// </summary>
    public sealed class StringMatcher
    {
        readonly string _text;
        int _length;
        int _startIndex;
        string _errorDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMatcher"/> class.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <param name="startIndex">Index where the match must start in <paramref name="text"/>.</param>
        public StringMatcher( string text, int startIndex = 0 )
            : this( text, startIndex, text.Length - startIndex )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMatcher"/> class on a substring.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <param name="startIndex">
        /// Index where the match must start in <paramref name="text"/>.
        /// </param>
        /// <param name="length">
        /// Number of characters to consider in the string.
        /// If <paramref name="startIndex"/> + length is greater than the length of the string, an <see cref="ArgumentException"/> is thrown.
        /// </param>
        public StringMatcher( string text, int startIndex, int length )
        {
            if( text == null ) throw new ArgumentNullException( nameof( text ) );
            if( startIndex < 0 || startIndex > text.Length ) throw new ArgumentOutOfRangeException( nameof( startIndex ) );
            if( startIndex + length > text.Length ) throw new ArgumentException( nameof( length ) );
            _text = text;
            _startIndex = startIndex;
            _length = length;
        }

        /// <summary>
        /// Gets the whole text.
        /// </summary>
        /// <value>The text.</value>
        public string Text => _text; 

        /// <summary>
        /// Gets the current start index: this is incremented by <see cref="Forward(int)"/>
        /// or <see cref="UncheckedMove(int)"/>.
        /// </summary>
        /// <value>The current start index.</value>
        public int StartIndex => _startIndex;

        /// <summary>
        /// Gets the current head: this is the character in <see cref="Text"/> at index <see cref="StartIndex"/>.
        /// </summary>
        /// <value>The head.</value>
        public char Head => _text[_startIndex];

        /// <summary>
        /// Gets the current length available.
        /// </summary>
        /// <value>The length.</value>
        public int Length => _length; 

        /// <summary>
        /// Gets whether this matcher is at the end of the text to match.
        /// </summary>
        /// <value><c>true</c> on end; otherwise, <c>false</c>.</value>
        public bool IsEnd => _length <= 0;

        /// <summary>
        /// Gets whether an error has been set.
        /// You can call <see cref="SetSuccess"/> to clear the error.
        /// </summary>
        /// <value><c>true</c> on error; otherwise, <c>false</c>.</value>
        public bool IsError => _errorDescription != null; 

        /// <summary>
        /// Gets the error message if any.
        /// You can call <see cref="SetSuccess"/> to clear the error.
        /// </summary>
        /// <value>The error message. Null when no error.</value>
        public string ErrorMessage => _errorDescription; 

        /// <summary>
        /// Sets an error and always returns false. The message starts with the caller's method name.
        /// </summary>
        /// <param name="expectedMessage">
        /// Optional object. Its <see cref="object.ToString()"/> will be used to generate an "expected '...'" message.
        /// </param>
        /// <param name="callerName">Name of the caller (automatically injected by the compiler).</param>
        /// <returns>Always false to use it as the return statement in a match method.</returns>
        public bool SetError( object expectedMessage = null, [CallerMemberName]string callerName = null )
        {
            _errorDescription = FormatMessage( expectedMessage, callerName );
            return false;
        }

        private static string FormatMessage( object expectedMessage, string callerName )
        {
            string d = callerName;
            string tail = expectedMessage != null ? expectedMessage.ToString() : null;
            if( !string.IsNullOrEmpty( tail ) )
            {
                d += ": expected '" + tail + "'.";
            }
            return d;
        }

        /// <summary>
        /// Clears any error and returns true. 
        /// <returns>Always true to use it as the return statement in a match method.</returns>
        public bool SetSuccess()
        {
            _errorDescription = null;
            return true;
        }

        /// <summary>
        /// Moves back the head at a previously index and sets an error. 
        /// The message starts with the caller's method name.
        /// </summary>
        /// <param name="savedStartIndex">Index to reset.</param>
        /// <param name="expectedMessage">
        /// Optional object. Its <see cref="object.ToString()"/> will be used to generate an "expected '...'" message.
        /// </param>
        /// <param name="callerName">Name of the caller (automatically injected by the compiler).</param>
        /// <returns>Always false to use it as the return statement in a match method.</returns>
        public bool BackwardSetError( int savedStartIndex, object expectedMessage = null, [CallerMemberName]string callerName = null )
        {
            int delta = _startIndex - savedStartIndex;
            if( savedStartIndex < 0 || delta < 0 ) throw new ArgumentException( nameof( savedStartIndex ) );
            _length += delta;
            _startIndex = savedStartIndex;
            if( _errorDescription != null )
            {
                _errorDescription = FormatMessage( expectedMessage, callerName ) + Environment.NewLine + "<-- " + _errorDescription;
            }
            else _errorDescription = FormatMessage( expectedMessage, callerName );
            return SetError( expectedMessage, callerName );
        }

        /// <summary>
        /// Moves the head without any check and returns always true: typically called by 
        /// successful TryMatchXXX methods.
        /// Can be used to move the head at any position in the <see cref="Text"/> (or outside it since NO checks are made).
        /// </summary>
        /// <param name="delta">Number of characters.</param>
        /// <returns>Always <c>true</c>.</returns>
        public bool UncheckedMove( int delta )
        {
            _startIndex += delta;
            _length -= delta;
            return true;
        }

        /// <summary>
        /// Increments the <see cref="StartIndex"/> (and decrements <see cref="Length"/>) with the 
        /// specified character count and clears any existing error.
        /// </summary>
        /// <param name="charCount">The successfully matched character count. 
        /// Must be positive and should not move head past the end of the substring.</param>
        /// <returns>Always true to use it as the return statement in a match method.</returns>
        public bool Forward( int charCount )
        {
            if( charCount < 0 ) throw new ArgumentException( nameof( charCount ) );
            int newLen = _length - charCount;
            if( newLen < 0 ) throw new InvalidOperationException( Impl.CoreResources.StringMatcherForwardPastEnd );
            _startIndex += charCount;
            _length = newLen;
            _errorDescription = null;
            return true;
        }

        /// <summary>
        /// Matches an exact single character. 
        /// If match fails, <see cref="SetError"/> is called.
        /// </summary>
        /// <param name="c">The character that must match.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool MatchChar( char c ) => TryMatchChar( c ) ? SetSuccess() : SetError( c );

        /// <summary>
        /// Attempts to match an exact single character. 
        /// </summary>
        /// <param name="c">The character that must match.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool TryMatchChar( char c ) => !IsEnd && Head == c ? UncheckedMove( 1 ) : false;

        /// <summary>
        /// Matches a text without setting an error if match fails.
        /// </summary>
        /// <param name="text">The string that must match. Can not be null nor empty.</param>
        /// <param name="comparisonType">Specifies the culture, case, and sort rules.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool TryMatchText( string text, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase )
        {
            if( string.IsNullOrEmpty( text ) ) throw new ArgumentException( nameof( text ) );
            int len = text.Length;
            return !IsEnd
                    && len <= _length
                    && String.Compare( _text, _startIndex, text, 0, len, comparisonType ) == 0
                ? UncheckedMove( len )
                : false;
        }

        /// <summary>
        /// Matches a text.
        /// </summary>
        /// <param name="text">The string that must match. Can not be null nor empty.</param>
        /// <param name="comparisonType">Specifies the culture, case, and sort rules.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool MatchText( string text, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase ) => TryMatchText( text ) ? SetSuccess() : SetError();

        /// <summary>
        /// Matches a sequence of white spaces.
        /// </summary>
        /// <param name="minCount">Minimal number of white spaces to match.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool MatchWhiteSpaces( int minCount = 1 )
        {
            int i = _startIndex;
            int len = _length;
            while( len != 0 && Char.IsWhiteSpace( _text, i ) ) { ++i; --len; }
            if( i - _startIndex >= minCount )
            {
                _startIndex = i;
                _length = len;
                _errorDescription = null;
                return true;
            }
            return SetError( minCount + " whitespace(s)" );
        }

        /// <summary>
        /// Matches Int32 values that must not start with '0' ('0' is valid but '0d', where d is any digit, is not).
        /// A signed integer starts with a '-'. '-0' is valid but '-0d' (where d is any digit) is not.
        /// If the value is to big for an Int32, it fails.
        /// </summary>
        /// <param name="i">The result integer. 0 on failure.</param>
        /// <param name="minValue">Optional minimal value.</param>
        /// <param name="maxValue">Optional maximal value.</param>
        /// <returns><c>true</c> when matched, <c>false</c> otherwise.</returns>
        public bool MatchInt32( out int i, int minValue = Int32.MinValue, int maxValue = Int32.MaxValue )
        {
            i = 0;
            int savedIndex = _startIndex;
            long value = 0;
            bool signed;
            if( IsEnd ) return SetError();
            if( (signed = TryMatchChar( '-' )) && IsEnd ) return BackwardSetError( savedIndex );

            char c;
            if( TryMatchChar( '0' ) )
            {
                if( !IsEnd && (c = Head) >= '0' && c <= '9' ) return BackwardSetError( savedIndex, "0...9" );
                return SetSuccess();
            }
            unchecked
            {
                long iMax = Int32.MaxValue;
                if( signed ) iMax = iMax + 1;
                while( !IsEnd && (c = Head) >= '0' && c <= '9' )
                {
                    value = value * 10 + (c - '0');
                    if( value > iMax ) break;
                    ++_startIndex;
                    --_length;
                }
            }
            if( _startIndex > savedIndex )
            {
                if( signed ) value = -value;
                if( value < (long)minValue || value > (long)maxValue )
                {
                    return BackwardSetError( savedIndex, String.Format( CultureInfo.InvariantCulture, "value between {0} and {1}", minValue, maxValue ) );
                }
                i = (int)value;
                return SetSuccess();
            }
            return SetError();
        }

        /// <summary>
        /// Overridden to return a detailed string with <see cref="ErrorMessage"/> (if any),
        /// the <see cref="Head"/> character, <see cref="StartIndex"/> position and
        /// whole <see cref="Text"/>.
        /// </summary>
        /// <returns>Detailed string.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            if( _errorDescription != null )
            {
                b.Append( "Error: " ).Append( _errorDescription ).AppendLine();
            }
            if( !IsEnd )
            {
                b.Append( "Head: " ).Append( Head ).Append( ", StartIndex: " ).Append( StartIndex ).AppendLine();
            }
            b.Append( "Text: " ).Append( _text );
            return b.ToString();
        }

    }
}
