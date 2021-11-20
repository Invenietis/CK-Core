using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    public static class DictionaryExtension
    {

        class ReadOnlyDictionaryWrapper<TKey, TValue, TReadOnlyValue> : IReadOnlyDictionary<TKey, TReadOnlyValue>
                    where TValue : TReadOnlyValue
                    where TKey : notnull
        {
            readonly IDictionary<TKey, TValue> _dictionary;

            public ReadOnlyDictionaryWrapper( IDictionary<TKey, TValue> dictionary )
            {
                _dictionary = dictionary ?? throw new ArgumentNullException( nameof( dictionary ) );
            }
            public bool ContainsKey( TKey key ) => _dictionary.ContainsKey( key );

            public IEnumerable<TKey> Keys => _dictionary.Keys;

            public bool TryGetValue( TKey key, [MaybeNullWhen( false )] out TReadOnlyValue value )
            {
                var r = _dictionary.TryGetValue( key, out var v );
                value = v!;
                return r;
            }

            public IEnumerable<TReadOnlyValue> Values => _dictionary.Values.Cast<TReadOnlyValue>();

            public TReadOnlyValue this[TKey key] => _dictionary[key];

            public int Count => _dictionary.Count;

            public IEnumerator<KeyValuePair<TKey, TReadOnlyValue>> GetEnumerator() => _dictionary.Select( x => new KeyValuePair<TKey, TReadOnlyValue>( x.Key, x.Value ) ).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Creates a wrapper (a read only facade) on a dictionary that adapts the type of the values.
        /// </summary>
        /// <typeparam name="TKey">The dictionary key.</typeparam>
        /// <typeparam name="TValue">The dictionary value.</typeparam>
        /// <typeparam name="TReadOnlyValue">The base type of the <typeparamref name="TValue"/>.</typeparam>
        /// <param name="this">This dictionary.</param>
        /// <returns>A dictionary where values are a base type of this dictionary.</returns>
        public static IReadOnlyDictionary<TKey, TReadOnlyValue> AsIReadOnlyDictionary<TKey, TValue, TReadOnlyValue>( this IDictionary<TKey, TValue> @this )
            where TValue : TReadOnlyValue
            where TKey : notnull
        {
            return new ReadOnlyDictionaryWrapper<TKey, TValue, TReadOnlyValue>( @this );
        }

        /// <summary>
        /// Gets the value associated with the specified key if it exists otherwise calls the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <remarks>
        /// This version uses a <typeparamref name="TResult"/> type with the trick that <typeparamref name="TValue"/> is constrained to
        /// be a TResult: this correctly propagates null constraints at the cost of requiring an explicit type cast when the <paramref name="defaultValue"/>
        /// function returns the null literal.
        /// </remarks>
        /// <param name="this">This generic IDictionary.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">A delegate that will be called if the key does not exist.</param>
        /// <returns>
        /// The value associated with the specified key, if the key is found; otherwise, the result 
        /// of the <paramref name="defaultValue"/> delegate.
        /// </returns>
        public static TResult GetValueWithDefaultFunc<TKey, TValue, TResult>( this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TResult> defaultValue )
            where TKey : notnull
            where TValue : TResult
        {
            if( !@this.TryGetValue( key, out var result ) ) return defaultValue( key );
            return result;
        }

        /// <summary>
        /// Gets the value associated with the specified key if it exists otherwise calls the <paramref name="createValue"/> function
        /// and adds the newly obtained value into the dictionary.
        /// <para>
        /// The factory function can return a more specific type <typeparamref name="TNew"/> than <typeparamref name="TValue"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <typeparam name="TNew">Type of new value. Must be or specialize TValue.</typeparam>
        /// <param name="this">This generic IDictionary.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="createValue">A delegate that will be called if the key does not exist.</param>
        /// <returns>
        /// The value associated with the specified key, if the key is found; otherwise, the result 
        /// of the <paramref name="createValue"/> delegate (this result has been added to the dictionary).
        /// </returns>
        public static TValue GetOrSet<TKey, TValue, TNew>( this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TNew> createValue )
            where TKey : notnull
            where TNew : TValue
        {
            if( !@this.TryGetValue( key, out TValue? result ) )
            {
                result = createValue( key );
                @this.Add( key, result );
            }
            return result;
        }

        /// <summary>
        /// Adds the content of a dictionary to this <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="this">This generic IDictionary.</param>
        /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair {TKey,TValue}"/> from which content will be copied.</param>
        public static void AddRange<TKey, TValue>( this IDictionary<TKey, TValue> @this, IEnumerable<KeyValuePair<TKey, TValue>> source ) where TKey : notnull
        {
            foreach( var e in source ) @this.Add( e.Key, e.Value );
        }

    }
}
