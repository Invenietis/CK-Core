using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.Core
{
    /// <summary>
    /// <see cref="FormattedString"/> handler.
    /// This is called by Roslyn, this is not intended to be used directly.
    /// <para>
    /// This implementation is an adaptation of the public <see cref="DefaultInterpolatedStringHandler"/>.
    /// </para>
    /// </summary>
    [InterpolatedStringHandler]
    public ref struct FormattedStringHandler
    {
        const int GuessedLengthPerHole = 11;
        const int MinimumArrayPoolLength = 256;
        readonly IFormatProvider? _provider;
        char[] _arrayToReturnToPool;
        Span<char> _chars;
        int _pos;
        (int,int)[] _slots;
        int _currentSlot;
        readonly bool _hasCustomFormatter;

        public FormattedStringHandler( int literalLength, int formattedCount )
        {
            Throw.CheckArgument( "There must not be more than 99 placeholders.", formattedCount <= 100 );
            _provider = null;
            _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent( GetDefaultLength( literalLength, formattedCount ) );
            _pos = 0;
            _currentSlot = 0;
            _slots = formattedCount > 0 ? new (int, int)[formattedCount] : Array.Empty<(int,int)>();
            _hasCustomFormatter = false;
        }

        public FormattedStringHandler( int literalLength, int formattedCount, IFormatProvider? provider )
        {
            Throw.CheckArgument( "There must not be more than 99 placeholders.", formattedCount <= 100 );
            _provider = provider;
            _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent( GetDefaultLength( literalLength, formattedCount ) );
            _pos = 0;
            _currentSlot = 0;
            _slots = formattedCount > 0 ? new (int, int)[formattedCount] : Array.Empty<(int, int)>();
            _hasCustomFormatter = provider is not null && HasCustomFormatter( provider );
        }

        /// <summary>Derives a default length with which to seed the handler.</summary>
        /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )] 
        internal static int GetDefaultLength( int literalLength, int formattedCount ) =>
            Math.Max( MinimumArrayPoolLength, literalLength + (formattedCount * GuessedLengthPerHole) );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void StartSlot()
        {
            _slots[_currentSlot].Item1 = _pos;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void StopSlot()
        {
            ref var s = ref _slots[_currentSlot++];
            s.Item2 = _pos - s.Item1;
        }

        /// <summary>Gets the built <see cref="string"/>.</summary>
        /// <returns>The built string.</returns>
        public override string ToString() => new string( Text );

        internal (string, (int, int)[]) GetResult()
        {
            var t = new string( Text );
            var s = _slots;
            char[] toReturn = _arrayToReturnToPool;
            this = default; // defensive clear
            ArrayPool<char>.Shared.Return( toReturn );
            return (t, s);
        }

        readonly ReadOnlySpan<char> Text => _chars.Slice( 0, _pos );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void AppendLiteral( string value )
        {
            if( value.TryCopyTo( _chars.Slice( _pos ) ) )
            {
                _pos += value.Length;
            }
            else
            {
                GrowThenCopyString( value );
            }
        }

        public void AppendFormatted<T>( T value )
        {
            StartSlot();
            if( _hasCustomFormatter )
            {
                PAppendCustomFormatter( value, format: null );
                StopSlot();
                return;
            }
            string? s;
            if( value is IFormattable )
            {
                // We cannot reproduce the .Net 8 enum optimization here that calls
                // an internal unconstrained (: struct, Enum) version of TryFormat.
                // We use the .NET 8 Enum : ISpanFormattable (and .ToString() for < v8).
                if( value is ISpanFormattable )
                {
                    int charsWritten;
                    while( !((ISpanFormattable)value).TryFormat( _chars.Slice( _pos ), out charsWritten, default, _provider ) ) // constrained call avoiding boxing for value types
                    {
                        Grow();
                    }

                    _pos += charsWritten;
                    StopSlot();
                    return;
                }

                s = ((IFormattable)value).ToString( format: null, _provider ); // constrained call avoiding boxing for value types
            }
            else
            {
                s = value?.ToString();
            }

            if( s is not null )
            {
                PAppendFormattedLiteral( s );
            }
            StopSlot();
        }

        public void AppendFormatted<T>( T value, string? format )
        {
            StartSlot();
            PAppendFormatted( value, format );
            StopSlot();
        }

        public void AppendFormatted<T>( T value, int alignment )
        {
            int startingPos = _pos;
            AppendFormatted( value );
            if( alignment != 0 )
            {
                PAppendOrInsertAlignmentIfNeeded( startingPos, alignment );
            }
        }

        public void AppendFormatted<T>( T value, int alignment, string? format )
        {
            StartSlot();
            int startingPos = _pos;
            PAppendFormatted( value, format );
            if( alignment != 0 )
            {
                PAppendOrInsertAlignmentIfNeeded( startingPos, alignment );
            }
            StopSlot();
        }

        public void AppendFormatted( scoped ReadOnlySpan<char> value )
        {
            StartSlot();
            // Fast path for when the value fits in the current buffer
            if( value.TryCopyTo( _chars.Slice( _pos ) ) )
            {
                _pos += value.Length;
            }
            else
            {
                GrowThenCopySpan( value );
            }
            StopSlot();
        }

        public void AppendFormatted( scoped ReadOnlySpan<char> value, int alignment = 0, string? format = null )
        {
            bool leftAlign = false;
            if( alignment < 0 )
            {
                leftAlign = true;
                alignment = -alignment;
            }

            int paddingRequired = alignment - value.Length;
            if( paddingRequired <= 0 )
            {
                AppendFormatted( value );
                return;
            }
            StartSlot();
            EnsureCapacityForAdditionalChars( value.Length + paddingRequired );
            if( leftAlign )
            {
                value.CopyTo( _chars.Slice( _pos ) );
                _pos += value.Length;
                _chars.Slice( _pos, paddingRequired ).Fill( ' ' );
                _pos += paddingRequired;
            }
            else
            {
                _chars.Slice( _pos, paddingRequired ).Fill( ' ' );
                _pos += paddingRequired;
                value.CopyTo( _chars.Slice( _pos ) );
                _pos += value.Length;
            }
            StopSlot();
        }

        public void AppendFormatted( string? value )
        {
            StartSlot();
            // Fast-path for no custom formatter and a non-null string that fits in the current destination buffer.
            if( !_hasCustomFormatter &&
                value is not null &&
                value.TryCopyTo( _chars.Slice( _pos ) ) )
            {
                _pos += value.Length;
            }
            else
            {
                PAppendFormattedSlow( value );
            }
            StopSlot();
        }

        public void AppendFormatted( string? value, int alignment = 0, string? format = null ) => AppendFormatted<string?>( value, alignment, format );

        public void AppendFormatted( object? value, int alignment = 0, string? format = null ) => AppendFormatted<object?>( value, alignment, format );

        [MethodImpl( MethodImplOptions.NoInlining )]
        void PAppendFormattedSlow( string? value )
        {
            if( _hasCustomFormatter )
            {
                PAppendCustomFormatter( value, format: null );
            }
            else if( value is not null )
            {
                EnsureCapacityForAdditionalChars( value.Length );
                value.CopyTo( _chars.Slice( _pos ) );
                _pos += value.Length;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void PAppendFormattedLiteral( string value )
        {
            if( value.TryCopyTo( _chars.Slice( _pos ) ) )
            {
                _pos += value.Length;
            }
            else
            {
                GrowThenCopyString( value );
            }
        }

        void PAppendFormatted<T>( T value, string? format )
        {
            // If there's a custom formatter, always use it.
            if( _hasCustomFormatter )
            {
                PAppendCustomFormatter( value, format );
                return;
            }

            // Check first for IFormattable, even though we'll prefer to use ISpanFormattable, as the latter
            // requires the former.  For value types, it won't matter as the type checks devolve into
            // JIT-time constants.  For reference types, they're more likely to implement IFormattable
            // than they are to implement ISpanFormattable: if they don't implement either, we save an
            // interface check over first checking for ISpanFormattable and then for IFormattable, and
            // if it only implements IFormattable, we come out even: only if it implements both do we
            // end up paying for an extra interface check.
            string? s;
            if( value is IFormattable )
            {
                // If the value can format itself directly into our buffer, do so.
                if( value is ISpanFormattable )
                {
                    int charsWritten;
                    while( !((ISpanFormattable)value).TryFormat( _chars.Slice( _pos ), out charsWritten, format, _provider ) ) // constrained call avoiding boxing for value types
                    {
                        Grow();
                    }

                    _pos += charsWritten;
                    return;
                }

                s = ((IFormattable)value).ToString( format, _provider ); // constrained call avoiding boxing for value types
            }
            else
            {
                s = value?.ToString();
            }

            if( s is not null )
            {
                PAppendFormattedLiteral( s );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static bool HasCustomFormatter( IFormatProvider provider )
        {
            Debug.Assert( provider is not null );
            Debug.Assert( provider is not CultureInfo || provider.GetFormat( typeof( ICustomFormatter ) ) is null, "Expected CultureInfo to not provide a custom formatter" );
            return
                provider.GetType() != typeof( CultureInfo ) && // optimization to avoid GetFormat in the majority case
                provider.GetFormat( typeof( ICustomFormatter ) ) != null;
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        void PAppendCustomFormatter<T>( T value, string? format )
        {
            // This case is very rare, but we need to handle it prior to the other checks in case
            // a provider was used that supplied an ICustomFormatter which wanted to intercept the particular value.
            // We do the cast here rather than in the ctor, even though this could be executed multiple times per
            // formatting, to make the cast pay for play.
            Debug.Assert( _hasCustomFormatter );
            Debug.Assert( _provider != null );

            ICustomFormatter? formatter = (ICustomFormatter?)_provider.GetFormat( typeof( ICustomFormatter ) );
            Debug.Assert( formatter != null, "An incorrectly written provider said it implemented ICustomFormatter, and then didn't" );

            if( formatter is not null && formatter.Format( format, value, _provider ) is string customFormatted )
            {
                PAppendFormattedLiteral( customFormatted );
            }
        }

        void PAppendOrInsertAlignmentIfNeeded( int startingPos, int alignment )
        {
            Debug.Assert( startingPos >= 0 && startingPos <= _pos );
            Debug.Assert( alignment != 0 );

            int charsWritten = _pos - startingPos;

            bool leftAlign = false;
            if( alignment < 0 )
            {
                leftAlign = true;
                alignment = -alignment;
            }

            int paddingNeeded = alignment - charsWritten;
            if( paddingNeeded > 0 )
            {
                EnsureCapacityForAdditionalChars( paddingNeeded );

                if( leftAlign )
                {
                    _chars.Slice( _pos, paddingNeeded ).Fill( ' ' );
                }
                else
                {
                    _chars.Slice( startingPos, charsWritten ).CopyTo( _chars.Slice( startingPos + paddingNeeded ) );
                    _chars.Slice( startingPos, paddingNeeded ).Fill( ' ' );
                }

                _pos += paddingNeeded;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void EnsureCapacityForAdditionalChars( int additionalChars )
        {
            if( _chars.Length - _pos < additionalChars )
            {
                Grow( additionalChars );
            }
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        void GrowThenCopyString( string value )
        {
            Grow( value.Length );
            value.CopyTo( _chars.Slice( _pos ) );
            _pos += value.Length;
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        void GrowThenCopySpan( scoped ReadOnlySpan<char> value )
        {
            Grow( value.Length );
            value.CopyTo( _chars.Slice( _pos ) );
            _pos += value.Length;
        }

        [MethodImpl( MethodImplOptions.NoInlining )] 
        void Grow( int additionalChars )
        {
            Debug.Assert( additionalChars > _chars.Length - _pos );
            GrowCore( (uint)_pos + (uint)additionalChars );
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        void Grow()
        {
            GrowCore( (uint)_chars.Length + 1 );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void GrowCore( uint requiredMinCapacity )
        {
            // string.MaxLength is internal. 
            uint newCapacity = Math.Max( requiredMinCapacity, Math.Min( (uint)_chars.Length * 2, 0x3FFFFFDF ) );
            int arraySize = (int)Math.Clamp( newCapacity, MinimumArrayPoolLength, int.MaxValue );

            char[] newArray = ArrayPool<char>.Shared.Rent( arraySize );
            _chars.Slice( 0, _pos ).CopyTo( newArray );

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = newArray;

            if( toReturn is not null )
            {
                ArrayPool<char>.Shared.Return( toReturn );
            }
        }
    }


}
