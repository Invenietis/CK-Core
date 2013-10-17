using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    class RouteResolver
    {
        internal readonly RouteConfigurationResolved Root;
        internal readonly Dictionary<string,SubRouteConfigurationResolved> NamedSubRoutes;

        class PreRoute : IRouteConfigurationContext
        {
            readonly IActivityMonitor _monitor;
            readonly IProtoRoute _protoRoute;
            readonly Dictionary<string,ActionConfigurationResolved> _idxActions;
            readonly List<ActionConfigurationResolved> _actions;

            internal PreRoute( IActivityMonitor monitor, IProtoRoute protoRoute )
            {
                _monitor = monitor;
                _protoRoute = protoRoute;
                _idxActions = new Dictionary<string,ActionConfigurationResolved>();
                _actions = new List<ActionConfigurationResolved>();
                foreach( var meta in _protoRoute.MetaConfigurations ) meta.Apply( this );
            }

            public List<ActionConfigurationResolved> FinalizeActions()
            {
                for( int i = _actions.Count - 1; i >= 0; --i )
                {
                    if( !_idxActions.ContainsKey( _actions[i].Name ) ) _actions.RemoveAt( i );
                }
                return _actions;
            }

            #region IRouteConfigurationContext

            IActivityMonitor IRouteConfigurationContext.Monitor { get { return _monitor; } }

            IProtoRoute IRouteConfigurationContext.ProtoRoute { get { return _protoRoute; } }

            IEnumerable<ActionConfigurationResolved> IRouteConfigurationContext.CurrentActions { get { return _idxActions.Values; } }

            ActionConfigurationResolved IRouteConfigurationContext.FindExisting( string name )
            {
                return _idxActions.GetValueWithDefault( name, null );
            }

            bool IRouteConfigurationContext.RemoveAction( string name )
            {
                if( !_idxActions.Remove( name ) )
                {
                    _monitor.Warn( "Action declaration '{0}' to remove is not found.", name );
                    return false;
                }
                return true;
            }

            bool IRouteConfigurationContext.AddDeclaredAction( string name, string declaredName, bool fromDeclaration )
            {
                var a = _protoRoute.FindDeclaredAction( declaredName );
                if( a == null ) 
                {
                    if( !fromDeclaration ) _monitor.Warn( "Action declaration '{0}' not found. Action '{1}' can not be registered.", declaredName );
                    return false;
                }
                ActionConfigurationResolved exists = _idxActions.GetValueWithDefault( name, null );
                if( exists != null )
                {
                    _monitor.Error( "Action '{0}' can not be added. An action with the same name already exists.", name );
                    return false;
                }
                var added = ActionConfigurationResolved.Create( _monitor, a, true, _idxActions.Count );
                _idxActions.Add( name, added );
                _actions.Add( added );
                return true;
            }

            #endregion
        }

        public RouteResolver( IActivityMonitor monitor, RouteConfiguration c )
        {
            try
            {
                using( monitor.OpenGroup( LogLevel.Info, c.Name.Length > 0 ? "Resolving root configuration (name is '{0}')." : "Resolving root configuration.", c.Name ) )
                {
                    ProtoResolver protoResolver = new ProtoResolver( monitor, c );
                    NamedSubRoutes = new Dictionary<string, SubRouteConfigurationResolved>();
                    using( monitor.OpenGroup( LogLevel.Info, "Building final routes." ) )
                    {
                        var preRoot = new PreRoute( monitor, protoResolver.Root );
                        Root = new RouteConfigurationResolved( protoResolver.Root.FullName, c.ConfigData, preRoot.FinalizeActions().AsReadOnlyList() );
                        foreach( IProtoSubRoute sub in protoResolver.NamedSubRoutes.Values )
                        {
                            var preRoute = new PreRoute( monitor, sub );
                            NamedSubRoutes.Add( sub.FullName, new SubRouteConfigurationResolved( sub, preRoute.FinalizeActions().AsReadOnlyList() ) );
                        }
                        Root.SubRoutes = protoResolver.NamedSubRoutes.Values.Select( p => NamedSubRoutes[p.FullName] ).ToArray().AsReadOnlyList();
                        foreach( IProtoSubRoute sub in protoResolver.NamedSubRoutes.Values )
                        {
                            NamedSubRoutes[sub.FullName].SubRoutes = sub.SubRoutes.Select( p => NamedSubRoutes[p.FullName] ).ToArray().AsReadOnlyList();
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                monitor.Fatal( ex );
            }
        }
    }

}
