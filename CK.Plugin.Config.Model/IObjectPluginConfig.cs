#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IObjectPluginConfig.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Easy to use name-value dictionary bound to an object and a plugin.
    /// </summary>
    public interface IObjectPluginConfig
    {
        /// <summary>
        /// Fires whenever a change that concerns this collection occurs.
        /// The change may concern other objects and other plugins but
        /// the object and the plugin bound to this dictionary belongs to <see cref="ConfigChangedEventArgs.MultiObj"/>
        /// and <see cref="ConfigChangedEventArgs.MultiPluginId"/> respectively.
        /// </summary>
        // event EventHandler<ConfigChangedEventArgs> Changed;

        /// <summary>
        /// Gets the number of configuration entries.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Removes all configuration entries for the plugin.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets whether this configuration contains the given key.
        /// </summary>
        /// <param name="key">Key to find.</param>
        /// <returns>True if the key exists in the configuration, false otherwise.</returns>
        bool Contains( string key );

        /// <summary>
        /// Adds a configuration entry. If the key already exists, an exception is thrown.
        /// Use <see cref="GetOrSet{T}(string,T)"/> to add a value only if it does not exist.
        /// </summary>
        /// <param name="key">Name of the configuration entry.</param>
        /// <param name="value">Value of the confiration entry.</param>
        void Add( string key, object value );

        /// <summary>
        /// Gets an existing configuration entry or sets a new one if it does not exist. 
        /// If the existing value is not compatible with <typeparam name="T">, an <see cref="InvalidCastException"/> is thrown.
        /// The <see cref="GetOrSet{T}(string,T,Func{object,T})"/> overload offers a converter function.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="value">Value to set (and return) if no value exists..</param>
        /// <returns>The value associated to the key.</returns>
        T GetOrSet<T>( string key, T value );

        /// <summary>
        /// Gets an existing value and converts it if needed or sets a new one if it does not exist. 
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="value">Value to set (and return) if no value exists.</param>
        /// <param name="converter">
        /// Delegate that converts an existing value to the <typeparamref name="T"/> type.
        /// If a conversion occurs, the converted value replaces the previous one and a <see cref="IConfigContainer.Changed"/> event is fired.
        /// It can not be null and is called only if the existing value's type is not compatible with T.
        /// </param>
        /// <returns>The value associated to the key.</returns>
        T GetOrSet<T>( string key, T value, Func<object, T> converter );

        /// <summary>
        /// Gets an existing configuration entry or sets a new one if it does not exist (deferred obtention). 
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="value">
        /// Delegate that creates/obtains a value. Called only if the entry does not exist. 
        /// When this function is null, the default value for <typeparamref name="T"/> is used.
        /// </param>
        /// <returns>The value associated to the key.</returns>
        T GetOrSet<T>( string key, Func<T> value );

        /// <summary>
        /// Gets an existing configuration entry and converts it if needed or sets a new one if it does not exist (deferred obtention).
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="value">
        /// Delegate that creates/obtains a value. Called only if the entry does not exist. 
        /// When this function is null, the default value for <typeparamref name="T"/> is used.
        /// </param>
        /// <param name="converter">
        /// Delegate that converts an existing value to the <typeparamref name="T"/> type.
        /// If a conversion occurs, the converted value replaces the previous one and a <see cref="IConfigContainer.Changed"/> event is fired.
        /// It can not be null and is called only if the existing value's type is not compatible with T.
        /// </param>
        /// <returns>The value associated to the key.</returns>
        T GetOrSet<T>( string key, Func<T> value, Func<object, T> converter );

        /// <summary>
        /// Removes the configuration entry. Does nothing if the key does not exist.
        /// </summary>
		/// <param name="key">Name of the configuration entry.</param>
        /// <returns>True if the entry has been removed, false if it did not exist.</returns>
        bool Remove( string key );

        /// <summary>
        /// Gets or sets the object for a given key. When getting, null is returned if the key does not exist.
        /// When setting, the entry is either added or updated with the new value.
        /// </summary>
        /// <param name="key">The key for which a configuration must be obtained or set.</param>
        /// <returns>The value or null if the key does not exists.</returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Gets or sets the object associated to a given key just as the indexer but 
        /// returns whether the entry has actually been updated or not.
        /// </summary>
        /// <param name="k">The key for which a configuration must be set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>It can be <see cref="ChangeStatus.None"/>, <see cref="ChangeStatus.Update"/> or <see cref="ChangeStatus.Add"/>.</returns>
        ChangeStatus Set( string k, object value );


        [Obsolete( "Use GetOrSet instead", true )]
        void TryAdd( string key, object value );

        /* TODO: 
         * Implement these helpers (spi).
        sbyte GetInt8( string key );
        byte GetUInt8( string key );
        short GetInt16( string key );
        ushort GetUInt16( string key );
        int GetInt32( string key );
        uint GetUInt32( string key );
        long GetInt32( string key );
        ulong GetUInt64( string key );
        double GetDouble( string key );
        float GetFloat( string key );
        Guid GetGuid( string key );
        string GetString( string key );
        DateTime GetDateTime( string key );
         */
    }
}
