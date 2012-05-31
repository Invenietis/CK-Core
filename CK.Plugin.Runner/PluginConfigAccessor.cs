#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\PluginConfigAccessor.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Plugin.Config
{
	internal sealed class PluginConfigAccessor : IPluginConfigAccessor
	{
		public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        INamedVersionedUniqueId _idEdited;
		IObjectPluginConfig _system;
		IObjectPluginConfig _user;
		IObjectPluginConfig _context;
        IConfigContainer _configContainer;

        internal PluginConfigAccessor( INamedVersionedUniqueId idEdited, IConfigManagerExtended cfg, object contextObject )
		{
            Debug.Assert( cfg != null );

            _idEdited = idEdited;
            _configContainer = cfg.Container;

            _system = _configContainer.GetObjectPluginConfig( cfg.ConfigManager.SystemConfiguration, _idEdited );
            _user = _configContainer.GetObjectPluginConfig( cfg.ConfigManager.UserConfiguration, _idEdited );
            _context = _configContainer.GetObjectPluginConfig( contextObject, _idEdited );
		}

        /// <summary>
        /// Change detection is made by the runner itself (one event subscription) since
        /// it use its dictionnary to route the event: this is far more efficient than 
        /// if each PluginConfigAccessor had to subscribe and filter the event for its plugin identifier.
        /// </summary>
        /// <param name="e"></param>
        internal void RaiseConfigChanged( ConfigChangedEventArgs e )
        {
            Debug.Assert( e.MultiPluginId.Contains( _idEdited ) );
            if( ConfigChanged != null ) ConfigChanged( this, e );
        }

		IObjectPluginConfig Get( object o )
		{
            return _configContainer.GetObjectPluginConfig( o, _idEdited );
		}

		public IObjectPluginConfig System
		{
            get { return _system; }
		}

		public IObjectPluginConfig User
		{
            get { return _user; }
		}

		public IObjectPluginConfig Context
		{
            get { return _context; }
		}

		public IObjectPluginConfig this[object o]
		{
			get { return Get( o ); }
		}
	}
}
