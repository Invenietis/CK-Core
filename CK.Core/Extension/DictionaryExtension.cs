using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable

namespace CK.Core;

/// <summary>
/// Provides extension methods for <see cref="IDictionary{TKey,TValue}"/>.
/// </summary>
public static class DictionaryExtension
{

    sealed class ReadOnlyIDictionaryWrapper<TKey, TValue, TReadOnlyValue> : IReadOnlyDictionary<TKey, TReadOnlyValue>
                where TValue : TReadOnlyValue
                where TKey : notnull
    {
        readonly IDictionary<TKey, TValue> _dictionary;

        public ReadOnlyIDictionaryWrapper( IDictionary<TKey, TValue> dictionary )
        {
            Throw.CheckNotNullArgument( dictionary );
            _dictionary = dictionary;
        }
        public bool ContainsKey( TKey key ) => _dictionary.ContainsKey( key );

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public bool TryGetValue( TKey key, [MaybeNullWhen( false )] out TReadOnlyValue value )
        {
            var r = _dictionary.TryGetValue( key, out var v );
            value = v;
            return r;
        }

        public IEnumerable<TReadOnlyValue> Values => _dictionary.Values.Cast<TReadOnlyValue>();

        public TReadOnlyValue this[TKey key] => _dictionary[key];

        public int Count => _dictionary.Count;

        public IEnumerator<KeyValuePair<TKey, TReadOnlyValue>> GetEnumerator() => _dictionary.Select( x => new KeyValuePair<TKey, TReadOnlyValue>( x.Key, x.Value ) ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    sealed class ReadOnlyIReadOnlyDictionaryWrapper<TKey, TValue, TReadOnlyValue> : IReadOnlyDictionary<TKey, TReadOnlyValue>
                where TValue : TReadOnlyValue
                where TKey : notnull
    {
        readonly IReadOnlyDictionary<TKey, TValue> _dictionary;

        public ReadOnlyIReadOnlyDictionaryWrapper( IReadOnlyDictionary<TKey, TValue> dictionary )
        {
            Throw.CheckNotNullArgument( dictionary );
            _dictionary = dictionary;
        }
        public bool ContainsKey( TKey key ) => _dictionary.ContainsKey( key );

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public bool TryGetValue( TKey key, [MaybeNullWhen( false )] out TReadOnlyValue value )
        {
            var r = _dictionary.TryGetValue( key, out var v );
            value = v;
            return r;
        }

        public IEnumerable<TReadOnlyValue> Values => _dictionary.Values.Cast<TReadOnlyValue>();

        public TReadOnlyValue this[TKey key] => _dictionary[key];

        public int Count => _dictionary.Count;

        public IEnumerator<KeyValuePair<TKey, TReadOnlyValue>> GetEnumerator() => _dictionary.Select( x => new KeyValuePair<TKey, TReadOnlyValue>( x.Key, x.Value ) ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Creates a wrapper (a read only facade) on a IDictionary that adapts the type of the values.
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
        return new ReadOnlyIDictionaryWrapper<TKey, TValue, TReadOnlyValue>( @this );
    }

    /// <summary>
    /// Creates a wrapper (a read only facade) on a dictionary that adapts the type of the values.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key.</typeparam>
    /// <typeparam name="TValue">The dictionary value.</typeparam>
    /// <typeparam name="TReadOnlyValue">The base type of the <typeparamref name="TValue"/>.</typeparam>
    /// <param name="this">This dictionary.</param>
    /// <returns>A dictionary where values are a base type of this dictionary.</returns>
    public static IReadOnlyDictionary<TKey, TReadOnlyValue> AsIReadOnlyDictionary<TKey, TValue, TReadOnlyValue>( this Dictionary<TKey, TValue> @this )
        where TValue : TReadOnlyValue
        where TKey : notnull
    {
        return new ReadOnlyIDictionaryWrapper<TKey, TValue, TReadOnlyValue>( @this );
    }

    /// <summary>
    /// Creates a wrapper (a read only facade) on an already existing IReadOnlyDictionary but that adapts the type of the values.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key.</typeparam>
    /// <typeparam name="TValue">The dictionary value.</typeparam>
    /// <typeparam name="TReadOnlyValue">The base type of the <typeparamref name="TValue"/>.</typeparam>
    /// <param name="this">This dictionary.</param>
    /// <returns>A dictionary where values are a base type of this dictionary.</returns>
    public static IReadOnlyDictionary<TKey, TReadOnlyValue> AsIReadOnlyDictionary<TKey, TValue, TReadOnlyValue>( this IReadOnlyDictionary<TKey, TValue> @this )
        where TValue : TReadOnlyValue
        where TKey : notnull
    {
        return new ReadOnlyIReadOnlyDictionaryWrapper<TKey, TValue, TReadOnlyValue>( @this );
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

    /// <inheritdoc cref="CollectionExtensions.GetValueOrDefault{TKey, TValue}(IReadOnlyDictionary{TKey, TValue}, TKey)"/>
    /// <remarks>
    /// To avoid ambiguities with the .Net CollectionExtensions.GetValueOrDefault(IReadOnlyDictionary) extension method, CK.Core defines
    /// this same extension for both IDictionary and the Dictionary class.
    /// </remarks>
    public static TValue? GetValueOrDefault<TKey, TValue>( this IDictionary<TKey, TValue> @this, TKey key ) => @this.TryGetValue( key, out var value ) ? value : default;

    /// <inheritdoc cref="System.Collections.Generic.CollectionExtensions.GetValueOrDefault{TKey, TValue}(IReadOnlyDictionary{TKey, TValue}, TKey, TValue)"/>
    /// <remarks>
    /// To avoid ambiguities with the .Net CollectionExtensions.GetValueOrDefault(IReadOnlyDictionary) extension method, CK.Core defines
    /// this same extension for both IDictionary and the Dictionary class.
    /// </remarks>
    public static TValue GetValueOrDefault<TKey, TValue>( this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue )
    {
        TValue? value;
        return @this.TryGetValue( key, out value ) ? value : defaultValue;
    }

    /// <inheritdoc cref="System.Collections.Generic.CollectionExtensions.GetValueOrDefault{TKey, TValue}(IReadOnlyDictionary{TKey, TValue}, TKey)"/>
    /// <remarks>
    /// To avoid ambiguities with the .Net CollectionExtensions.GetValueOrDefault(IReadOnlyDictionary) extension method, CK.Core defines
    /// this same extension for both IDictionary and the Dictionary class.
    /// </remarks>
    public static TValue? GetValueOrDefault<TKey, TValue>( this Dictionary<TKey, TValue> @this, TKey key ) where TKey : notnull
        => @this.TryGetValue( key, out var value ) ? value : default!;

    /// <inheritdoc cref="System.Collections.Generic.CollectionExtensions.GetValueOrDefault{TKey, TValue}(IReadOnlyDictionary{TKey, TValue}, TKey, TValue)"/>
    /// <remarks>
    /// To avoid ambiguities with the .Net CollectionExtensions.GetValueOrDefault(IReadOnlyDictionary) extension method, CK.Core defines
    /// this same extension for both IDictionary and the Dictionary class.
    /// </remarks>
    public static TValue GetValueOrDefault<TKey, TValue>( this Dictionary<TKey, TValue> @this, TKey key, TValue defaultValue ) where TKey : notnull
        => @this.TryGetValue( key, out var value ) ? value : defaultValue;

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
