#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\DictionaryExtension.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    public static class DictionaryExtension
    {

        /// <summary>
        /// Gets the value associated with the specified key if it exists otherwise returns the <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="that">This generic IDictionary.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">Default value to use if the key does not exist.</param>
        /// <returns>
        /// The value associated with the specified key, if the key is found; otherwise, the <paramref name="defaultValue"/>. 
        /// </returns>
        public static TValue GetValueWithDefault<TKey, TValue>( this IDictionary<TKey, TValue> that, TKey key, TValue defaultValue )
        {
            TValue result;
            if( !that.TryGetValue( key, out result ) ) result = defaultValue;
            return result;
        }

        /// <summary>
        /// Gets the value associated with the specified key if it exists otherwise calls the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="that">This generic IDictionary.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="defaultValue">A delegate that will be called if the key does not exist.</param>
        /// <returns>
        /// The value associated with the specified key, if the key is found; otherwise, the result 
        /// of the <paramref name="defaultValue"/> delegate.
        /// </returns>
        public static TValue GetValueWithDefaultFunc<TKey, TValue>( this IDictionary<TKey, TValue> that, TKey key, Func<TKey,TValue> defaultValue )
        {
            TValue result;
            if( !that.TryGetValue( key, out result ) ) result = defaultValue( key );
            return result;
        }

        /// <summary>
        /// Gets the value associated with the specified key if it exists otherwise calls the <paramref name="createValue"/> function
        /// and adds the newly obtained value into the dictionary.
        /// </summary>
        /// <param name="that">This generic IDictionary.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="createValue">A delegate that will be called if the key does not exist.</param>
        /// <returns>
        /// The value associated with the specified key, if the key is found; otherwise, the result 
        /// of the <paramref name="createValue"/> delegate (this result has been added to the dictionary).
        /// </returns>
        public static TValue GetOrSet<TKey, TValue>( this IDictionary<TKey, TValue> that, TKey key, Func<TKey, TValue> createValue )
        {
            TValue result;
            if( !that.TryGetValue( key, out result ) )
            {
                result = createValue( key );
                that.Add( key, result );
            }
            return result;
        }

        /// <summary>
        /// Adds the content of a dictionary to this <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="that">This generic IDictionary.</param>
        /// <param name="source">The <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair {TKey,TValue}"/> from which content will be copied.</param>
        public static void AddRange<TKey, TValue>( this IDictionary<TKey, TValue> that, IEnumerable<KeyValuePair<TKey, TValue>> source )
        {
            foreach( var e in source ) that.Add( e.Key, e.Value );
        }


    }
}
