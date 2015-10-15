using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Class TextMatcher. This class cannot be inherited.
    /// </summary>
    public sealed class TextMatcher
    {
        readonly string _text;
        readonly int _maxLength;
        int _start;
        int _head;
        string _errorDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMatcher"/> class.
        /// </summary>
        /// <param name="text">The string to parse.</param>
        /// <param name="start">
        /// Index where the match must start (can be equal to or greater than <paramref name="maxLength"/>: the match fails).
        /// </param>
        /// <param name="maxLength">
        /// Maximum index to consider in the string (it shortens the default <see cref="String.Length"/>), it can be zero or negative.
        /// If maxLength is greater than String.Length an <see cref="ArgumentException"/> is thrown.
        /// </param>
        public TextMatcher( string text, int start, int maxLength )
        {
            if( text == null ) throw new ArgumentNullException( nameof( text ) );
            if( start < 0 ) throw new ArgumentOutOfRangeException( nameof( start ) );
            if( maxLength > text.Length ) throw new ArgumentException( nameof( maxLength) );
            _text = text;
            _head = _start = start;
            _maxLength = maxLength;
        }

        /// <summary>
        /// Gets the whole text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get { return _text; } }

        public int HeadPosition { get { return _head; } }

        public char Head { get { return _text[_head]; } }
        public int MaxLength { get { return _maxLength; } }

        public bool IsEnd { get { return _head >= _maxLength; } }

        public bool IsError { get { return _errorDescription != null; } }

        /// <summary>
        /// Matches an exact single character.
        /// </summary>
        /// <param name="c">The character that must match.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool MatchChar( char c )
        {
            if( IsEnd ) return false;
            return Head == c ? Forward( 1 ) : SetError( c );
        }

        /// <summary>
        /// Matches a string at a given position in a string.
        /// </summary>
        /// <param name="s">The string that must match. Can not be null.</param>
        /// <param name="comparisonType">Specifies the culture, case, and sort rules.</param>
        /// <returns>True on success, false if the match failed.</returns>
        public bool MatchString( string s, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase )
        {
            if( string.IsNullOrEmpty( s ) ) throw new ArgumentException( nameof( s ) );
            if( IsEnd ) return false;
            int len = s.Length;
            return _head + len > _maxLength || String.Compare( s, _head, s, 0, len, comparisonType ) != 0 
                ? SetError( s )
                : Forward( len );
        }

        public bool SetError( object expectedMessage = null, [CallerMemberName]string callerName = null )
        {
            _errorDescription = callerName;
            string tail = expectedMessage != null ? expectedMessage.ToString() : null;
            if( !string.IsNullOrEmpty( tail ) )
            {
                _errorDescription += ": expected '" + tail + "'.";
            }
            return false; 
        }

        public bool Forward( int charCount )
        {
            if( charCount < 0 ) throw new ArgumentException( nameof( charCount ) );
            _head += charCount;
            if( _head > _maxLength ) throw new ArgumentException( nameof( charCount ) );
            _errorDescription = null;
            return true;
        }
    }
}
