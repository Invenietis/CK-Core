#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IConfigContainer.cs) is part of CiviKey. 
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
    /// Root interface for object/plugin configuration management.
    /// </summary>
    public interface IConfigContainer
    {
        /// <summary>
        /// Fires whenever a change occurs.
        /// </summary>
        event EventHandler<ConfigChangedEventArgs> Changed;

        /// <summary>
        /// Returns true if the object exists in the configuration.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <returns>True if the object exists in the configuration, false otherwise.</returns>
        bool Contains( object o );

        /// <summary>
        /// Clears all data associated to an object.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        void Clear( object o );

        /// <summary>
        /// Removes the data associated to an object and removes the object itself from internal maps.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        void Destroy( object o );

        /// <summary>
        /// Ensures that the object exists in the configuration.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        void Ensure( object o );

        /// <summary>
        /// Gets the <see cref="INamedVersionedUniqueId"/> by its <see cref="Guid"/> if it exists.
        /// </summary>
        /// <param name="pluginIdentifier">The <see cref="Guid"/> that identifies the plugin.</param>
        /// <returns>Null if not found.</returns>
        INamedVersionedUniqueId FindPlugin( Guid pluginIdentifier );

        /// <summary>
        /// Returns true if the plugin exists in the configuration.
        /// </summary>
        /// <param name="p">Plugin identifier.</param>
        bool Contains( INamedVersionedUniqueId p );

        /// <summary>
        /// Clears the data associated to a plugin.
        /// </summary>
        /// <param name="p">Plugin identifier.</param>
        void Clear( INamedVersionedUniqueId p );

        /// <summary>
        /// Clears the data associated to a plugin and removes the plugin itself from the internal maps.
        /// </summary>
        /// <param name="p">Plugin identifier.</param>
        void Destroy( INamedVersionedUniqueId p );

        /// <summary>
        /// Ensures that the plugin exists in the configuration. This combines a <see cref="FindPlugin"/>
        /// and an actual registration if it is not yet known.
        /// </summary>
        /// <param name="p">Plugin identifier.</param>
        /// <returns>
        /// The registered instance: it can be the parameter or the previously registered
        /// plugin with the same <see cref="IUniqueId.UniqueId"/>.
        /// </returns>
        INamedVersionedUniqueId Ensure( INamedVersionedUniqueId p );

        /// <summary>
        /// Returns true if the couple object/plugin exists in the configuration.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <returns>True if the couple exists in the configuration.</returns>
        bool Contains( object o, INamedVersionedUniqueId p );

        /// <summary>
        /// Gets the number of data held by a couple object/plugin.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <returns>Number of data element for the couple.</returns>
        int Count( object o, INamedVersionedUniqueId p );

        /// <summary>
        /// Gets an easy to use <see cref="IObjectPluginConfig"/> that acts as a standard name-value dictionary.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="ensure">True to ensure that the object and plugin exist.</param>
        /// <returns>Esay accessor.</returns>
        IObjectPluginConfig GetObjectPluginConfig( object o, INamedVersionedUniqueId p, bool ensure );

        /// <summary>
        /// Clears the data associated to a couple object/plugin.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        void Clear( object o, INamedVersionedUniqueId p );

        /// <summary>
        /// Returns true if the triplet object/plugin/key exists in the configuration.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key fo the data.</param>
        /// <returns>True if the triplet exists in the configuration.</returns>
        bool Contains( object o, INamedVersionedUniqueId p, string k );

        /// <summary>
        /// Adds a configuration entry. An exception is thrown if the triplet object/plugin/key already exists.
        /// Use <see cref="GetOrSet{T}(object,INamedVersionedUniqueId,string,T)"/> to add a value only if it does not exist.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="value">Value to set.</param>
        void Add( object o, INamedVersionedUniqueId p, string k, object value );

        /// <summary>
        /// Gets an existing configuration entry or sets a new one if it does not exist. 
        /// If the existing value is not compatible with <typeparam name="T">, an <see cref="InvalidCastException"/> is thrown.
        /// The <see cref="GetOrSet{T}(object,IUniqueID,string,T,Func{object,T})"/> overload offers a converter function.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="value">Value to set (and return) if no value exists.</param>
        /// <returns>The configuration value associated to the triplet object/plugin/key.</returns>
        T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, T value );

        /// <summary>
        /// Gets an existing configuration entry and converts it if needed or sets a new one if it does not exist. 
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="value">Value to set (and return) if no value exists.</param>
        /// <param name="converter">
        /// Delegate that converts an existing value to the <typeparamref name="T"/> type.
        /// If a conversion occurs, the converted value replaces the previous one an a <see cref="Changed"/> event is fired.
        /// It can not be null and is called only if the existing value's type is not compatible with T.
        /// </param>
        /// <returns>The configuration value associated to the triplet object/plugin/key.</returns>
        T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, T value, Func<object, T> converter );

        /// <summary>
        /// Gets an existing configuration entry or sets a new one if it does not exist (deferred obtention). 
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="value">
        /// Delegate that creates/obtains a value. Called only if the entry does not exist. 
        /// When this function is null, the default value for <typeparamref name="T"/> is used.
        /// </param>
        /// <returns>The configuration value associated to the triplet object/plugin/key.</returns>
        T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, Func<T> value );

        /// <summary>
        /// Gets an existing configuration entry and converts it if needed or sets a new one if it does not exist (deferred obtention).
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key fo the data.</param>
        /// <param name="value">
        /// Delegate that creates/obtains a value. Called only if the entry does not exist. 
        /// When this function is null, the default value for <typeparamref name="T"/> is used.
        /// </param>
        /// <param name="converter">
        /// Delegate that converts an existing value to the <typeparamref name="T"/> type.
        /// If a conversion occurs, the converted value replaces the previous one an a <see cref="Changed"/> event is fired.
        /// It can not be null and is called only if the existing value's type is not compatible with T.
        /// </param>
        /// <returns>The configuration value associated to the triplet object/plugin/key.</returns>
        T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, Func<T> value, Func<object, T> converter );

        /// <summary>
        /// Clear all data contained by this container.
        /// Objects and plugins are removed.
        /// </summary>
        void DestroyAll();

        /// <summary>
        /// Empty all data contained by this container.
        /// Objects and plugins are NOT removed.
        /// </summary>
        void ClearAll();

        /// <summary>
        /// Gets or sets the object associated to a given triplet object/plugin/key. 
        /// When getting, null is returned if the key does not exist.
        /// When setting, the entry is either added or updated with the new value.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">The key for which a configuration must be obtained or set.</param>
        /// <returns>The value or null if the configuration entry does not exists.</returns>
        object this[object o, INamedVersionedUniqueId p, string k] { get; set; }

        /// <summary>
        /// Gets or sets the object associated to a given triplet object/plugin/key just as the indexer but 
        /// returns whether the entry has actually been updated or not.
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">The key for which a configuration must be set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>It can be <see cref="ChangeStatus.None"/>, <see cref="ChangeStatus.Update"/> or <see cref="ChangeStatus.Add"/>.</returns>
        ChangeStatus Set( object o, INamedVersionedUniqueId p, string k, object value );

        /// <summary>
        /// Removes a triplet object/plugin/key. 
        /// </summary>
        /// <param name="o">Object that carries the properties.</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">The key to remove.</param>
        /// <returns>True if the entry has been removed, false if it did not exist.</returns>
        bool Remove( object o, INamedVersionedUniqueId p, string k );


    }
}
