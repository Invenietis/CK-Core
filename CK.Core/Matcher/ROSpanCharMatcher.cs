using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

        sealed class ErrorTracker
        {
            (int Pos, object, string Caller)[]? _errors;
            int _count;

            public bool AddExpectation( int pos, object expect, string caller )
            {
                if( _errors == null ) _errors = new (int, object, string)[8];
                else if( _count == _errors.Length )
                {
                    Array.Resize( ref _errors, _count * 2 );
                }
                _errors[_count++] = (pos, expect, caller);
                return false;
            }

            public bool ClearExpectations()
            {
                _count = 0;
                return true;
            }

            public bool HasError => _count != 0;

            public IEnumerable<(int Pos, string Expectation, string CallerName, int Depth)> GetErrors(int allTextLength )
            {
                if( _count == 0 ) return Enumerable.Empty<(int, string, string, int)>();
                Debug.Assert( _errors != null );
                return GetErrors( allTextLength, 0, _errors );
            }

            static IEnumerable<(int, string, string, int)> GetErrors( int allTextLength, int depth, (int Pos, object, string Caller)[] raw )
            {
                foreach( var error in raw )
                {
                    if( error.Item2 is string s )
                    {
                        yield return (allTextLength - error.Pos, s, error.Caller, depth);
                    }
                    else
                    {
                        if( error.Item2 == null ) break;
                        foreach( var r in GetErrors( allTextLength, depth + 1, ((int, object, string)[])error.Item2 ) )
                        {
                            yield return r;
                        }
                    }
                }
            }
        }

        readonly ErrorTracker _tracker;

        public ROSpanCharMatcher( ReadOnlySpan<char> text )
        {
            Head = text;
            AllText = text;
            _tracker = new ErrorTracker();
        }

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
        /// Clears any recorded expectations and returns true.
        /// </summary>
        /// <returns>Always true so it can be directly returned by the TryMatch function.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ClearExpectations() => _tracker.ClearExpectations();

        /// <summary>
        /// Gets whether this matcher has unsatisfied expectations.
        /// </summary>
        public bool HasError => _tracker.HasError;

        /// <summary>
        /// Gets the list of expectations if any: the expected position, the expected expression and name of the
        /// method that failed, and its depth in the parsing.
        /// <para>
        /// The Depth is not used yet (always 0) but it will be: the error will describe a tree of failed matches
        /// once subordinated matcher will be implemented.
        /// </para>
        /// </summary>
        /// <returns>The set of expectations.</returns>
        public IEnumerable<(int Pos, string Expectation, string CallerName, int Depth)> GetErrors() => _tracker.GetErrors( AllText.Length );

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
            => Head.TryMatch( value, comparison ) ? ClearExpectations() : AddExpectation( $"String '{value}'" );

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
            => Head.TrySkipWhiteSpaces( minCount ) ? ClearExpectations() : AddExpectation( $"At least {minCount} white space(s)." );

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
        /// <param name="allowLeadingZeros">False to forbid leading zeros.</param>
        /// <param name="minValue">Optional minimal value.</param>
        /// <param name="maxValue">Optional maximal value.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TryMatchInt32( out int i, bool allowLeadingZeros = true, int minValue = int.MinValue, int maxValue = int.MaxValue )
        {
            if( Head.TryMatchInt32( out i, allowLeadingZeros, minValue, maxValue ) )
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
        /// Tries to skip a double value, using the regular expression "-?[0-9]+(\.[0-9]+)?((e|E)(\+|-)?[0-9]+)?".
        /// </summary>
        /// <param name="head">This head.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool TrySkipDouble()
            => Head.TrySkipDouble() ? ClearExpectations() : AddExpectation( $"Float number" );

        /// <summary>
        /// Tries to match a double value. See <see cref="ReadOnlySpanCharExtensions.TryMatchDouble(ref ReadOnlySpan{char}, out double)"/>.
        /// </summary>
        /// <param name="value">The result double. 0.0 on failure.</param>
        /// <returns></returns>
        public bool TryMatchDouble( out double value )
            => Head.TryMatchDouble( out value ) ? ClearExpectations() : AddExpectation( "Float number" );


    }
}
