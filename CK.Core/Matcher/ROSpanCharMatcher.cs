using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CK.Core
{
    public ref struct ROSpanCharMatcher
    {
        /// <summary>
        /// The mutable current head.
        /// </summary>
        public ReadOnlySpan<char> Head;

        /// <summary>
        /// The immutable whole initial text.
        /// </summary>
        public readonly ReadOnlySpan<char> AllText;

        interface ITracker
        {
            bool AddExpectation( int pos, string expect, string caller );
            bool ClearExpectations();
            IDisposable OpenSubTracker( int pos, string? scopedExpectation, string callerName );
        }

        sealed class ErrorTracker : ITracker
        {
            ITracker _current;
            (int P, int D, string? E, string C)[]? _errors;
            Sub? _firstFree;
            int _errorCount;

            sealed class Sub : ITracker, IDisposable
            {
                readonly ErrorTracker _root;
                int _depth;
                int _startError;
                int _pos;
                ITracker _prev;
                string _header;
                string _callerName;
                internal Sub? _parentOrNextFree;
                bool _hasPendingHeader;
                bool _clearCalled;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
                public Sub( ErrorTracker root )
                {
                    _root = root;
                }
#pragma warning restore CS8618 

                internal Sub Initialize( int pos, int depth, string? header, string callerName, Sub? parent )
                {
                    _prev = _root._current;
                    _root._current = this;
                    _depth = depth;
                    _pos = pos;
                    _startError = _root._errorCount;
                    _parentOrNextFree = parent;
                    _hasPendingHeader = header != null || (parent != null && parent._hasPendingHeader);
                    _header = header ?? callerName;
                    _callerName = callerName;
                    return this;
                }

                public bool AddExpectation( int pos, string expect, string caller )
                {
                    _clearCalled = false;
                    if( _hasPendingHeader ) SetHeaders();
                    return _root.AddExpectation( pos, _depth, expect, caller );
                }

                void SetHeaders()
                {
                    Debug.Assert( _hasPendingHeader );
                    if( _parentOrNextFree != null && _parentOrNextFree._hasPendingHeader ) _parentOrNextFree.SetHeaders();
                    if( _header != null ) _root.AddExpectation( _pos, _depth - 1, _header, _callerName );
                    _hasPendingHeader = false;
                }

                public bool ClearExpectations()
                {
                    _clearCalled = true;
                    return _root.ClearExpectations( _startError );
                }

                public IDisposable OpenSubTracker( int pos, string? scopedExpectation, string callerName )
                {
                    return _root.AcquireSub().Initialize( pos, _depth + 1, scopedExpectation, callerName, this );
                }

                public void Dispose()
                {
                    if( !_clearCalled && _hasPendingHeader )
                    {
                        SetHeaders();
                    }
                    _root._current = _prev;
                    _root.ReleaseSub( this );
                }
            }

            Sub AcquireSub()
            {
                if( _firstFree == null ) return new Sub( this );
                var s = _firstFree;
                _firstFree = s._parentOrNextFree;
                return s;
            }

            void ReleaseSub( Sub s )
            {
                s._parentOrNextFree = _firstFree;
                _firstFree = s;
            }

            public ErrorTracker()
            {
                _current = this;
            }

            public bool AddExpectation( int pos, string expect, string caller ) => _current.AddExpectation( pos, expect, caller );

            bool ITracker.AddExpectation( int pos, string expect, string caller ) => AddExpectation( pos, 0, expect, caller );

            bool AddExpectation( int pos, int depth, string expect, string caller )
            {
                if( _errors == null ) _errors = new (int, int, string?, string)[16];
                else if( _errorCount == _errors.Length )
                {
                    Array.Resize( ref _errors, _errorCount * 2 );
                }
                _errors[_errorCount++] = (pos, depth, expect, caller);
                return false;
            }

            public bool ClearExpectations() => _current.ClearExpectations();

            bool ITracker.ClearExpectations() => ClearExpectations( 0 );

            bool ClearExpectations( int errorCount )
            {
                _errorCount = errorCount;
                return true;
            }

            public IDisposable OpenSubTracker( int pos, string? scopedExpectation, string callerName ) => _current.OpenSubTracker( pos, scopedExpectation, callerName );

            IDisposable ITracker.OpenSubTracker( int pos, string? scopedExpectation, string callerName )
            {
                return AcquireSub().Initialize( pos, 1, scopedExpectation, callerName, null );
            }

            public bool HasError => _errorCount != 0;

            public IEnumerable<(int Pos, int Depth, string Expectation, string CallerName)> GetErrors(int allTextLength )
            {
                if( _errorCount == 0 ) return Enumerable.Empty<(int, int, string, string)>();
                Debug.Assert( _errors != null );
                return _errors.Take( _errorCount ).Select( e => (allTextLength - e.P, e.D, e.E!, e.C ) );
            }
        }

        readonly ErrorTracker _tracker;

        /// <summary>
        /// Initializes a new <see cref="ROSpanCharMatcher"/>.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        public ROSpanCharMatcher( ReadOnlySpan<char> text )
        {
            Head = text;
            AllText = text;
            _tracker = new ErrorTracker();
        }

        /// <summary>
        /// Gets whether this matcher has unsatisfied expectations.
        /// </summary>
        public bool HasError => _tracker.HasError;

        /// <summary>
        /// Adds an expectation. <paramref name="expect"/>> must be written without "expect" word, only with the
        /// description of what was expected: "Json string", "Date", "Numbered item" without trailing dot.
        /// </summary>
        /// <param name="expect">The expected pattern description without trailing dot.</param>
        /// <param name="callerName">Method name of the caller (automatically set by the compiler).</param>
        /// <returns>Always false so it can be directly returned by the TryMatch function.</returns>
        /// <remarks>
        /// We use the CallerName here because it cannot be the awful mangled name of a code generated lambda since ReadOnlySpan
        /// as a ref struct cannot be used in a lambda (except in a <see cref="System.Buffers.ReadOnlySpanAction{T, TArg}"/> that
        /// has few chances to used here).
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool AddExpectation( string expect, [CallerMemberName] string? callerName = null ) => _tracker.AddExpectation( Head.Length, expect, callerName! );

        /// <summary>
        /// Adds an expectation. <paramref name="expect"/>> must be written without "expect" word, only with the
        /// description of what was expected: "Json string", "Date", "Numbered item" without trailing dot.
        /// </summary>
        /// <param name="offset">Offset of the expectation relative to the <see cref="Head"/>.</param>
        /// <param name="expect">The expected pattern description without trailing dot.</param>
        /// <param name="callerName">Method name of the caller (automatically set by the compiler).</param>
        /// <returns>Always false so it can be directly returned by the TryMatch function.</returns>
        /// <remarks>
        /// We use the CallerName here because it cannot be the awful mangled name of a code generated lambda since ReadOnlySpan
        /// as a ref struct cannot be used in a lambda (except in a <see cref="System.Buffers.ReadOnlySpanAction{T, TArg}"/>
        /// or other explicit delegate signatures that has few chances to used here).
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool AddExpectation( int offset, string expect, [CallerMemberName] string? callerName = null ) => _tracker.AddExpectation( Head.Length - offset, expect, callerName! );

        /// <summary>
        /// Clears any recorded expectations and returns true.
        /// </summary>
        /// <returns>Always true so it can be directly returned by the TryMatch function.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ClearExpectations() => _tracker.ClearExpectations();

        /// <summary>
        /// Opens a group of subordinated expectations that must be disposed.
        /// </summary>
        /// <param name="expectHeader">Optional header for the subordinated expectations.</param>
        /// <param name="callerName">Method name of the caller (automatically set by the compiler).</param>
        /// <returns>The disposable to close the group.</returns>
        public IDisposable OpenExpectations( string? expectHeader = null, [CallerMemberName] string? callerName = null ) => _tracker.OpenSubTracker( Head.Length, expectHeader, callerName! );

        /// <summary>
        /// Gets the list of expectations if any: the expected position, the expected expression and name of the
        /// method that failed, and its depth in the parsing.
        /// </summary>
        /// <returns>The set of expectations.</returns>
        public IEnumerable<(int Pos, int Depth, string Expectation, string CallerName)> GetErrors() => _tracker.GetErrors( AllText.Length );

        /// <summary>
        /// Forwards <see cref="Head"/> by <paramref name="length"/> even if actual head's length is shorter and
        /// returns the count of remaining characters (the new head's length).
        /// </summary>
        /// <param name="length">The length. Must be 0 or positive otherwise an ArgumentOutOfRangeException is thrown.</param>
        /// <returns>The remainder's head length.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int SafeForward( int length ) => Head.SafeForward( length );

        /// <summary>
        /// Tries to match a specific string.
        /// </summary>
        /// <param name="value">The string value to match.</param>
        /// <param name="comparison">How to compare.</param>
        /// <returns>True on success, false otherwise.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryMatch( ReadOnlySpan<char> value, StringComparison comparison = StringComparison.Ordinal )
            => Head.TryMatch( value, comparison ) ? ClearExpectations() : AddExpectation( $"String '{value}'" );

        /// <summary>
        /// Tries to match a character.
        /// </summary>
        /// <param name="h">The head.</param>
        /// <param name="value">The character to match.</param>
        /// <param name="comparison">How to compare.</param>
        /// <returns>True on success</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool TryMatch( char value, StringComparison comparison = StringComparison.Ordinal )
            => Head.TryMatch( value, comparison ) ? ClearExpectations() : AddExpectation( $"Character '{value}'" );

        /// <summary>
        /// Tries to parse a Guid.
        /// </summary>
        /// <remarks>
        /// Any of the 5 forms of Guid can be matched:
        /// <list type="table">
        /// <item><term>N</term><description>00000000000000000000000000000000</description></item>
        /// <item><term>D</term><description>00000000-0000-0000-0000-000000000000</description></item>
        /// <item><term>B</term><description>{00000000-0000-0000-0000-000000000000}</description></item>
        /// <item><term>P</term><description>(00000000-0000-0000-0000-000000000000)</description></item>
        /// <item><term>X</term><description>{0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}</description></item>
        /// </list>
        /// </remarks>
        /// <param name="id">The result Guid. <see cref="Guid.Empty"/> on failure.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TryMatchGuid( out Guid id )
            => Head.TryMatchGuid( out id ) ? ClearExpectations() : AddExpectation( "Guid" );

        /// <summary>
        /// Tries to skip a sequence of white spaces.
        /// Use <paramref name="minCount"/> = 0 to skip any number of white spaces.
        /// </summary>
        /// <param name="h">The head.</param>
        /// <param name="minCount">Minimal number of white spaces to skip.</param>
        /// <returns>True on success, false if <paramref name="minCount"/> white spaces cannot be skipped before the end of the head).</returns>
        public bool TrySkipWhiteSpaces( int minCount = 1 )
            => Head.TrySkipWhiteSpaces( minCount ) ? ClearExpectations() : AddExpectation( minCount == 1 ? "At least one white space" : $"At least {minCount} white space(s)" );

        /// <summary>
        /// Tries to match an Int32 value. A signed integer starts with a '-' and must not be followed by white spaces.
        /// If the value is too big for an Int32, it fails.
        /// <para>
        /// When <paramref name="allowLeadingZeros"/> is false, the value must not start with '0' ('0' is valid but '0d', where d is any digit, is not)
        /// and '-0' is valid but '-0d' (where d is any digit) is not.
        /// </para>
        /// </summary>
        /// <param name="head">This head.</param>
        /// <param name="i">The result integer. 0 on failure.</param>
        /// <param name="minValue">Optional minimal value.</param>
        /// <param name="maxValue">Optional maximal value.</param>
        /// <param name="allowLeadingZeros">True to allow leading zeros.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TryMatchInt32( out int i, int minValue = int.MinValue, int maxValue = int.MaxValue, bool allowLeadingZeros = false )
        {
            if( Head.TryMatchInt32( out i, minValue, maxValue, allowLeadingZeros ) )
            {
                return ClearExpectations();
            }
            string s;
            if( minValue >= 0 )
            {
                s = $"Integer between {minValue} and {maxValue}";
            }
            else
            {
                s = $"Signed integer between {minValue} and {maxValue}";
            }
            if( !allowLeadingZeros ) s += " (without leading zeros)";
            return AddExpectation( s );
        }

        /// <summary>
        /// Tries to skip a double value. This skips a pattern like the regular expression "^-?[0-9]+(\.[0-9]+)?((e|E)(\+|-)?[0-9]+)?".
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public bool TrySkipDouble()
            => Head.TrySkipDouble() ? ClearExpectations() : AddExpectation( $"Floating number" );

        /// <summary>
        /// Tries to match a double value. See <see cref="ReadOnlySpanCharExtensions.TryMatchDouble(ref ReadOnlySpan{char}, out double)"/>.
        /// </summary>
        /// <param name="value">The result double. 0.0 on failure.</param>
        /// <returns></returns>
        public bool TryMatchDouble( out double value )
            => Head.TryMatchDouble( out value ) ? ClearExpectations() : AddExpectation( "Floating number" );

        /// <summary>
        /// Tries to parse an hexadecimal values of 1 to 16 '0'-'9', 'A'-'F' or 'a'-'f' digits.
        /// </summary>
        /// <param name="value">Resulting value on success.</param>
        /// <param name="minDigit">Minimal digit count. Must be between 1 and 16 and smaller or equal to <paramref name="maxDigit"/>.</param>
        /// <param name="maxDigit">Maximal digit count. Must be between 1 and 16.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TryMatchHexNumber( out ulong value, int minDigit = 1, int maxDigit = 16 )
            => Head.TryMatchHexNumber( out value, minDigit, maxDigit )
                ? ClearExpectations()
                : AddExpectation( minDigit == maxDigit ? $"{minDigit} digits hexadecimal number" : $"Hexadecimal number ({minDigit} to {maxDigit} digits)" );

        /// <summary>
        /// Tries to skip a quoted string. This handles escaped \" and \\ but not other
        /// escaped characters: the string may be invalid regarding JSON string grammar.
        /// See the string definition https://www.json.org/json-en.html.
        /// </summary>
        /// <param name="allowNull">True to allow 'null' token.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TrySkipJSONQuotedString( bool allowNull = false )
            => Head.TrySkipJSONQuotedString( allowNull )
                ? ClearExpectations()
                : AddExpectation( allowNull ? "JSON string or null" : "JSON string" );

        /// <summary>
        /// Tries to match a JSON quoted string. Invalid escaped characters (like \') are invalid and will fail.
        /// See the string definition https://www.json.org/json-en.html.
        /// </summary>
        /// <param name="content">Extracted content.</param>
        /// <param name="allowNull">True to allow 'null' token.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TryMatchJSONQuotedString( out string? content, bool allowNull = false )
        {
            content = null;
            bool isEmpty = Head.Length == 0;
            if( isEmpty || Head[0] != '"' )
            {
                if( !isEmpty && allowNull && Head.TryMatch( "null" ) ) return ClearExpectations();
                return AddExpectation( allowNull ? "JSON string or null" : "JSON string" );
            }
            int i = 1;
            int len = Head.Length - 1;
            StringBuilder? b = null;
            while( len >= 0 )
            {
                if( len == 0 ) return AddExpectation( 1, "Ending quote" ); ;
                char c = Head[i++];
                --len;
                if( c == '"' ) break;
                if( c == '\\' )
                {
                    if( len == 0 ) return AddExpectation( i - 1, "Valid JSON escape character" ); ;
                    if( b == null )
                    {
                        b = new StringBuilder();
                        b.Append( Head.Slice( 1, i - 1 ) );
                    }
                    switch( (c = Head[i++]) )
                    {
                        case 'r': c = '\r'; break;
                        case 'n': c = '\n'; break;
                        case 'b': c = '\b'; break;
                        case 't': c = '\t'; break;
                        case 'f': c = '\f'; break;
                        case 'u':
                            {
                                var h = Head.Slice( i );
                                if( !h.TryMatchHexNumber( out var u, 4, 4 ) )
                                {
                                    return AddExpectation( i - 1, "4 digits hexadecimal number" );
                                }
                                len -= 4;
                                i += 4;
                                c = (char)u;
                                break;
                            }
                        case '\\': // These are the only other valid escaped characters in JSON.
                        case '"':
                        case '/': break;
                        default:
                            {
                                return AddExpectation( i - 1, "Valid JSON escape character" );
                            }
                    }
                }
                if( b != null ) b.Append( c );
            }
            if( b != null ) content = b.ToString();
            else content = new string( Head.Slice( 1, i - 2 ) );
            Head = Head.Slice( i );
            return ClearExpectations();
        }

        /// <summary>
        /// Tries to skip a JSON terminal value: a "string", null, a number (double value), true or false.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public bool TrySkipJSONTerminalValue()
            => Head.TrySkipJSONTerminalValue() ? ClearExpectations() : AddExpectation("null, true, false, a floating number or a \"string\"" );

        /// <summary>
        /// Tries to match a JSON terminal value: a "string", null, a number (double value), true or false.
        /// </summary>
        /// <param name="value">The parsed value.</param>
        /// <returns>True if a JSON value has been matched, false otherwise.</returns>
        public bool TryMatchJSONTerminalValue( out object? value )
        {
            if( TryMatchJSONQuotedString( out string? s, true ) )
            {
                value = s;
                return true;
            }
            if( TryMatchDouble( out double d ) )
            {
                value = d;
                return true;
            }
            if( TryMatch( "true" ) )
            {
                value = true;
                return true;
            }
            if( TryMatch( "false" ) )
            {
                value = false;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Matches a very simple version of a JSON object content: this match stops at the first closing }.
        /// White spaces and JS comments (//... or /* ... */) are skipped.
        /// </summary>
        /// <param name="o">The read object on success as a list of tuples.</param>
        /// <returns>True on success, false on error.</returns>
        public bool TryMatchJSONObjectContent( [NotNullWhen(true)]out List<(string, object?)>? o )
        {
            o = new List<(string, object?)>();
            var savedHead = Head;
            using( OpenExpectations( "JSON object properties" ) )
            {
                while( Head.Length != 0 )
                {
                    Head.SkipWhiteSpacesAndJSComments();
                    if( Head.TryMatch( '}' ) )
                    {
                        return ClearExpectations();
                    }
                    if( !TryMatchJSONQuotedString( out string? propName ) )
                    {
                        o = null;
                        Head = savedHead;
                        return AddExpectation( "Quoted JSON Property Name" );
                    }
                    Debug.Assert( propName != null );
                    Head.SkipWhiteSpacesAndJSComments();
                    if( !TryMatch( ':' ) || !TryMatchAnyJSON( out object? value ) )
                    {
                        o = null;
                        Head = savedHead;
                        return false;
                    }
                    o.Add( (propName, value) );
                    Head.SkipWhiteSpacesAndJSComments();
                    // This accepts a trailing comma at the end of a property list: ..."a":0,} is not an error.
                    Head.TryMatch( ',' );
                }
                o = null;
                Head = savedHead;
                return false;
            }
        }

        /// <summary>
        /// Matches a JSON array content: the match ends with the first ].
        /// White spaces and JS comments (//... or /* ... */) are skipped.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="value">The list of objects on success.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TryMatchJSONArrayContent( [NotNullWhen(true)]out List<object?>? value )
        {
            value = new List<object?>();
            var savedHead = Head;
            using( OpenExpectations( "JSON array values" ) )
            {
                while( Head.Length != 0 )
                {
                    Head.SkipWhiteSpacesAndJSComments();
                    if( Head.TryMatch( ']' ) )
                    {
                        return ClearExpectations();
                    }
                    if( !TryMatchAnyJSON( out object? cell ) )
                    {
                        value = null;
                        Head = savedHead;
                        return false;
                    }
                    value.Add( cell );
                    Head.SkipWhiteSpacesAndJSComments();
                    // Allow trailing comma: ,] is valid.
                    Head.TryMatch( ',' );
                }
                value = null;
                Head = savedHead;
                return false;
            }
        }

        /// <summary>
        /// Tries to match a { "JSON" : "object" }, a ["JSON", "array"] or a terminal value (string, null, double, true or false) 
        /// and any combination of them.
        /// White spaces and JS comments (//... or /* ... */) are skipped.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="value">
        /// A list of nullable objects (for array), a list of tuples (string,object?) for object or
        /// a double, string, boolean or null (for null).
        /// </param>
        /// <returns>True on success, false on error.</returns>
        public bool TryMatchAnyJSON( out object? value )
        {
            value = null;
            var savedHead = Head;
            using( OpenExpectations( "Any JSON token or object" ) )
            {
                Head.SkipWhiteSpacesAndJSComments();
                if( Head.TryMatch( '{' ) )
                {
                    if( !TryMatchJSONObjectContent( out var c ) ) return false;
                    value = c;
                    Debug.Assert( !HasError );
                    return true;
                }
                if( Head.TryMatch( '[' ) )
                {
                    if( !TryMatchJSONArrayContent( out var t ) ) return false;
                    value = t;
                    Debug.Assert( !HasError );
                    return true;
                }
                if( TryMatchJSONTerminalValue( out value ) )
                {
                    Debug.Assert( !HasError );
                    return true;
                }
                value = null;
                Head = savedHead;
                return false;
            }
        }


    }
}
