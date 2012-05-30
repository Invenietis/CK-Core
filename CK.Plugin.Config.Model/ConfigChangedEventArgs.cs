#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\ConfigChangedEventArgs.cs) is part of CiviKey. 
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
using System.Text;
using CK.Core;

namespace CK.Plugin.Config
{
	public class ConfigChangedEventArgs : EventArgs
	{
        /// <summary>
        /// The object whose configuration has changed if there is only one object concerned.
        /// If multiple objects are concerned this field is null and <see cref="MultiObj"/> must be used. 
        /// </summary>
        public readonly object Obj;

        /// <summary>
        /// The objects whose configuration have changed.
        /// It is always available (if <see cref="Obj"/> is not null, this collection contains it).
        /// </summary>
        public readonly IReadOnlyCollection<object> MultiObj;

        /// <summary>
        /// The plugins whose configuration have changed.
        /// </summary>
        public readonly IReadOnlyCollection<INamedVersionedUniqueId> MultiPluginId;

        /// <summary>
        /// The name of the entry whose configuration has changed. 
        /// Null if the configuration, the object, the plugin or the association (by <see cref="IConfigContainer.Remove(object,INamedVersionedUniqueId)"/>) has been cleared (<see cref="Status"/> is <see cref="ChangeStatus.ContainerClear"/> or <see cref="ChangeStatus.ContainerDestroy"/>).
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// The new value when <see cref="Key"/> is not null and <see cref="Status"/> is <see cref="ChangeStatus.Update"/> or <see cref="ChangeStatus.Add"/>.
        /// Null when <see cref="Status"/> is <see cref="ChangeStatus.Delete"/> or <see cref="ChangeStatus.ContainerClear"/> or <see cref="ChangeStatus.ContainerDestroy"/>.
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Status of the change. 
        /// Can be <see cref="ChangeStatus.Update"/>, <see cref="ChangeStatus.Add"/>,  <see cref="ChangeStatus.Delete"/> or <see cref="ChangeStatus.ContainerClear"/> or <see cref="ChangeStatus.ContainerDestroy"/>.
        /// </summary>
        public readonly ChangeStatus Status;

        /// <summary>
        /// True if the change is global to the configuration: all objects and all plugins are concerned.
        /// </summary>
        public bool IsAllConcerned
        {
            get { return IsAllObjectsConcerned && IsAllPluginsConcerned; }
        }

        /// <summary>
        /// True if the change concerns all the plugins: the <see cref="MultiPluginId"/> collection contains all the plugin identifiers managed by the container.
        /// </summary>
        public readonly bool IsAllPluginsConcerned;

        /// <summary>
        /// True if the change concerns all the objects: the <see cref="MultiObj"/> collection contains all the object managed by the container.
        /// </summary>
        public readonly bool IsAllObjectsConcerned;

        /// <summary>
        /// Gets whether this change is the result of a <see cref="IConfigContainer.ClearAll"/>.
        /// Meaning that objects have been emptied, but not destroyed
        /// </summary>
        /// <returns>True if the whole configuration is cleared.</returns>
        public bool IsClearAll
        {
            get { return IsAllConcerned && Status == ChangeStatus.ContainerClear; }
        }

        /// <summary>
        /// Gets whether this change is the result of a <see cref="IConfigContainer.DestroyAll"/>.
        /// </summary>
        /// <returns>True if the whole configuration is destroyed.</returns>
        public bool IsDestroyedAll
        {
            get { return IsAllConcerned && Status == ChangeStatus.ContainerClear; }
        }

        public ConfigChangedEventArgs( IObjectPluginAssociation a, IConfigEntry e, ChangeStatus status )
            : this( a.Obj, new ReadOnlyListMono<object>( a.Obj ), new ReadOnlyListMono<INamedVersionedUniqueId>( a.PluginId ), e.Key, e.Value, status )
        {
        }

        public ConfigChangedEventArgs( IReadOnlyCollection<object> multiObj, bool allObjectsConcerned, INamedVersionedUniqueId pluginId, ChangeStatus status )
            : this( null, multiObj, new ReadOnlyListMono<INamedVersionedUniqueId>( pluginId ), null, null, status )
        {
            IsAllObjectsConcerned = allObjectsConcerned;
        }

        public ConfigChangedEventArgs( object obj, IReadOnlyCollection<INamedVersionedUniqueId> multiPluginId, bool allPluginsConcerned, ChangeStatus status )
            : this( obj, new ReadOnlyListMono<object>( obj ), multiPluginId, null, null, status )
        {
            IsAllPluginsConcerned = allPluginsConcerned;
        }

        public ConfigChangedEventArgs( object obj, INamedVersionedUniqueId pluginId, ChangeStatus status )
            : this( obj, new ReadOnlyListMono<object>( obj ), new ReadOnlyListMono<INamedVersionedUniqueId>( pluginId ), null, null, status )
        {
        }

        public ConfigChangedEventArgs( IReadOnlyCollection<object> multiObj, bool allObjectsConcerned, IReadOnlyCollection<INamedVersionedUniqueId> multiPluginId, bool allPluginsConcerned, ChangeStatus status )
            : this( null, multiObj, multiPluginId, null, null, status )
        {
            IsAllPluginsConcerned = allPluginsConcerned;
            IsAllObjectsConcerned = allObjectsConcerned;
        }

        private ConfigChangedEventArgs( 
            object obj, 
            IReadOnlyCollection<object> multiObj,
            IReadOnlyCollection<INamedVersionedUniqueId> multiPluginId, 
            string key, 
            object value, 
            ChangeStatus status )
        {
            Obj = obj;
            MultiObj = multiObj;
            MultiPluginId = multiPluginId;
            Key = key;
            Value = value;
            Status = status;
        }



    }
}
