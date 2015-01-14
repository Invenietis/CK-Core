#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\RouteResolver.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Internal class used by <see cref="RouteConfiguration.Resolve"/>.
    /// </summary>
    class RouteResolver
    {
        internal readonly RouteConfigurationResolved Root;
        internal readonly Dictionary<string,SubRouteConfigurationResolved> NamedSubRoutes;

        class PreRoute : IRouteConfigurationContext
        {
            readonly IActivityMonitor _monitor;
            readonly IProtoRoute _protoRoute;
            readonly Dictionary<string,ActionConfigurationResolved> _actionsByName;
            readonly List<ActionConfigurationResolved> _actions;

            internal PreRoute( IActivityMonitor monitor, IProtoRoute protoRoute )
            {
                _monitor = monitor;
                _protoRoute = protoRoute;
                _actionsByName = new Dictionary<string,ActionConfigurationResolved>();
                _actions = new List<ActionConfigurationResolved>();
                foreach( var meta in _protoRoute.MetaConfigurations ) meta.Apply( this );
            }

            public List<ActionConfigurationResolved> FinalizeActions()
            {
                for( int i = _actions.Count - 1; i >= 0; --i )
                {
                    if( !_actionsByName.ContainsKey( _actions[i].Name ) ) _actions.RemoveAt( i );
                }
                return _actions;
            }

            #region IRouteConfigurationContext

            IActivityMonitor IRouteConfigurationContext.Monitor { get { return _monitor; } }

            IProtoRoute IRouteConfigurationContext.ProtoRoute { get { return _protoRoute; } }

            IEnumerable<ActionConfigurationResolved> IRouteConfigurationContext.CurrentActions { get { return _actionsByName.Values; } }

            ActionConfigurationResolved IRouteConfigurationContext.FindExisting( string name )
            {
                return _actionsByName.GetValueWithDefault( name, null );
            }

            bool IRouteConfigurationContext.RemoveAction( string name )
            {
                if( !_actionsByName.Remove( name ) )
                {
                    _monitor.Warn().Send( "Action declaration '{0}' to remove is not found.", name );
                    return false;
                }
                return true;
            }

            bool IRouteConfigurationContext.AddDeclaredAction( string name, string declaredName, bool fromMetaInsert )
            {
                var a = _protoRoute.FindDeclaredAction( declaredName );
                if( a == null ) 
                {
                    if( fromMetaInsert ) _monitor.Warn().Send( "Action declaration '{0}' not found. Action '{1}' can not be registered.", declaredName, name );
                    return false;
                }
                ActionConfigurationResolved exists = _actionsByName.GetValueWithDefault( name, null );
                if( exists != null )
                {
                    _monitor.Error().Send( "Action '{0}' can not be added. An action with the same name already exists.", name );
                    return false;
                }
                var added = ActionConfigurationResolved.Create( _monitor, a, true, _actionsByName.Count );
                _actionsByName.Add( name, added );
                _actions.Add( added );
                return true;
            }

            #endregion
        }

        public RouteResolver( IActivityMonitor monitor, RouteConfiguration c )
        {
            try
            {
                using( monitor.OpenInfo().Send( c.Name.Length > 0 ? "Resolving root configuration (name is '{0}')." : "Resolving root configuration.", c.Name ) )
                {
                    ProtoResolver protoResolver = new ProtoResolver( monitor, c );
                    NamedSubRoutes = new Dictionary<string, SubRouteConfigurationResolved>();
                    using( monitor.OpenInfo().Send( "Building final routes." ) )
                    {
                        var preRoot = new PreRoute( monitor, protoResolver.Root );
                        Root = new RouteConfigurationResolved( protoResolver.Root.FullName, c.ConfigData, preRoot.FinalizeActions().AsReadOnlyList() );
                        foreach( IProtoSubRoute sub in protoResolver.NamedSubRoutes.Values )
                        {
                            var preRoute = new PreRoute( monitor, sub );
                            NamedSubRoutes.Add( sub.FullName, new SubRouteConfigurationResolved( sub, preRoute.FinalizeActions().AsReadOnlyList() ) );
                        }
                        Root.SubRoutes = protoResolver.Root.SubRoutes.Select( p => NamedSubRoutes[p.FullName] ).ToArray().AsReadOnlyList();
                        foreach( IProtoSubRoute sub in protoResolver.NamedSubRoutes.Values )
                        {
                            NamedSubRoutes[sub.FullName].SubRoutes = sub.SubRoutes.Select( p => NamedSubRoutes[p.FullName] ).ToArray().AsReadOnlyList();
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                monitor.Fatal().Send( ex );
            }
        }
    }

}
