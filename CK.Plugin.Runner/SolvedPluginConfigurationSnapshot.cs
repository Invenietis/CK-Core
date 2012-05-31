#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\SolvedPluginConfigurationSnapshot.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;
using CK.Plugin.Config;

namespace CK.Plugin.Hosting
{
    internal sealed class SolvedPluginConfigurationSnapshot : ISolvedPluginConfiguration
    {
        Dictionary<Guid, SolvedPluginConfigElement> _cfg;

        internal SolvedPluginConfigurationSnapshot( ISolvedPluginConfiguration cfg )
        {
            _cfg = new Dictionary<Guid, SolvedPluginConfigElement>();
            FillFrom( cfg );
        }

        /// <summary>
        /// Unused by this implementation (snapshot).
        /// </summary>
        public event EventHandler<SolvedPluginConfigurationChangedEventArs> Changed;

        internal void Apply( ISolvedPluginConfiguration newCfg )
        {
            _cfg.Clear();
            FillFrom( newCfg );
            if( Changed != null ) Changed( this, new SolvedPluginConfigurationChangedEventArs( null ) );
        }

        void FillFrom( ISolvedPluginConfiguration c )
        {
            _cfg.Clear();
            foreach( SolvedPluginConfigElement element in c )
            {
                _cfg.Add( element.PluginId, new SolvedPluginConfigElement( element.PluginId, element.Status ) );
            }
        }

        public SolvedPluginConfigElement Find( Guid pluginId )
        {
            return _cfg.GetValueWithDefault( pluginId, null );
        }

        public bool Contains( object item )
        {
            SolvedPluginConfigElement e = item as SolvedPluginConfigElement;
            return e != null ? _cfg.ContainsKey( e.PluginId ) : false;
        }

        public int Count
        {
            get { return _cfg.Count; }
        }

        public IEnumerator<SolvedPluginConfigElement> GetEnumerator()
        {
            return _cfg.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _cfg.Values.GetEnumerator();
        }


        public SolvedConfigStatus GetStatus( Guid pluginID )
        {
            SolvedPluginConfigElement e = null;
            _cfg.TryGetValue( pluginID, out e );
            return e != null ? e.Status : SolvedConfigStatus.Optional;
        }

    }
}
