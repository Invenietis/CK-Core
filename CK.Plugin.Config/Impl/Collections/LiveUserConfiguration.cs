#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\Collections\LiveUserConfiguration.cs) is part of CiviKey. 
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
using System.Collections;
using System.Collections.Generic;
using CK.Core;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Holds the <see cref="ConfigUserAction"/> for each plugin
    /// </summary>
    internal class LiveUserConfiguration : ILiveUserConfiguration
    {
        Dictionary<Guid,LiveUserAction> _actions;
        IReadOnlyCollection<ILiveUserAction> _collection;

        public event EventHandler<LiveUserConfigurationChangingEventArgs> Changing;

        public event EventHandler<LiveUserConfigurationChangedEventArgs> Changed;

        public LiveUserConfiguration()
        {
            _actions = new Dictionary<Guid, LiveUserAction>();
            _collection = new ReadOnlyCollectionOnICollection<LiveUserAction>( _actions.Values );
        }

        internal bool CanChange( ChangeStatus changeAction, Guid pluginId, ConfigUserAction action )
        {
            if( Changing != null )
            {
                LiveUserConfigurationChangingEventArgs eCancel = new LiveUserConfigurationChangingEventArgs( changeAction, pluginId, action );
                Changing( this, eCancel );
                return !eCancel.Cancel;
            }
            return true;
        }

        internal void Change( ChangeStatus changeAction, Guid pluginId, ConfigUserAction action )
        {
            if( Changed != null )
            {
                LiveUserConfigurationChangedEventArgs e = new LiveUserConfigurationChangedEventArgs( changeAction, pluginId, action );
                Changed( this, e );
            }
        }

        /// <summary>
        /// Sets the <see cref="ConfigUserAction"/> for the specified <see cref="IPluginLoaderInfo"/>
        /// </summary>
        /// <param name="pluginId">PluginId of the plugin</param>
        /// <param name="actionType">Action of the Use</param>
        /// <returns>The setted LiveUserAction of the plugin set as parameter</returns>
        public ILiveUserAction SetAction( Guid pluginId, ConfigUserAction actionType )
        {
            LiveUserAction action;
            if( !_actions.TryGetValue( pluginId, out action ) && CanChange( ChangeStatus.Add, pluginId, actionType ) )
            {
                action = new LiveUserAction( pluginId, actionType );
                _actions.Add( pluginId, action );

                Change( ChangeStatus.Add, pluginId, actionType );
            }
            else if( CanChange( ChangeStatus.Update, pluginId, actionType ) )
            {
                action.Action = actionType;
                Change( ChangeStatus.Update, pluginId, actionType );
            }
            return action;
        }

        /// <summary>
        /// Gets the <see cref="ConfigUserAction"/> for the specified PluginId
        /// </summary>
        /// <param name="pluginId">ID of the plugin</param>
        /// <returns>The UserAction for the specified PluginId</returns>
        public ConfigUserAction GetAction( Guid pluginId )
        {
            LiveUserAction action = _actions.GetValueWithDefault<Guid, LiveUserAction>( pluginId, null );
            return action != null ? action.Action : ConfigUserAction.None;
        }

        /// <summary>
        /// Removes the <see cref="ConfigUserAction"/> attached to the specified plugin
        /// </summary>
        /// <param name="pluginId">ID of the plugin</param>
        public void ResetAction( Guid pluginId )
        {
            if( _actions.ContainsKey( pluginId ) && CanChange( ChangeStatus.Delete, pluginId, ConfigUserAction.None ) )
            {
                _actions.Remove( pluginId );
                Change( ChangeStatus.Delete, pluginId, ConfigUserAction.None );
            }
        }

        #region IReadOnlyCollection<ILiveUserAction> Members

        public bool Contains( object item )
        {
            return _collection.Contains( item );
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public IEnumerator<ILiveUserAction> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
