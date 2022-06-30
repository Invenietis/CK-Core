using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    // https://github.com/dotnet/runtime/issues/39919
    /// <summary>
    /// ConcurrentHashSet backed by a <see cref="ConcurrentDictionary{TKey, byte}"/>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashSet<T> : ICollection<T>, IEnumerable<T> where T : notnull
    {
        readonly ConcurrentDictionary<T, byte> _backingDictionary;


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ConcurrentHashSet() => _backingDictionary = new();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ConcurrentHashSet( IEnumerable<T> collection )
            : this( collection, null ) { }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ConcurrentHashSet( IEqualityComparer<T>? comparer )
            => _backingDictionary = new( comparer );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ConcurrentHashSet( IEnumerable<T> collection, IEqualityComparer<T>? comparer )
            => _backingDictionary = new( collection.Select( s => new KeyValuePair<T, byte>( s, default ) ), comparer );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ConcurrentHashSet( int concurrencyLevel, int capacity )
            => _backingDictionary = new( concurrencyLevel, capacity );

        public bool IsEmpty => _backingDictionary.IsEmpty;

        public int Count => _backingDictionary.Count;

        public IEqualityComparer<T> Comparer => _backingDictionary.Comparer;

        bool ICollection<T>.IsReadOnly => ((ICollection<KeyValuePair<T, byte>>)_backingDictionary).IsReadOnly;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Add( T value ) => _backingDictionary.TryAdd( value, 0 );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains( T value ) => _backingDictionary.ContainsKey( value );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( T value ) => _backingDictionary.TryRemove( value, out _ );
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Clear() => Clear();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public IEnumerator<T> GetEnumerator() => new Enumerator( _backingDictionary.GetEnumerator() );

        struct Enumerator : IEnumerator<T>
        {
            readonly IEnumerator<KeyValuePair<T, byte>> _baseEnumerator;

            public Enumerator( IEnumerator<KeyValuePair<T, byte>> baseEnumerator )
                => _baseEnumerator = baseEnumerator;

            public T Current => _baseEnumerator.Current.Key;

            object IEnumerator.Current => _baseEnumerator.Current.Key;

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void Dispose() => _baseEnumerator.Dispose();

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public bool MoveNext() => _baseEnumerator.MoveNext();

            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public void Reset() => _baseEnumerator.Reset();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void ICollection<T>.Add( T item ) => Add( item );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        void ICollection<T>.CopyTo( T[] array, int arrayIndex )
            => _backingDictionary.Keys.CopyTo( array, arrayIndex );
    }
}
