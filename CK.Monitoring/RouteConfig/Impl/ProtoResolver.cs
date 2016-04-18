using System;
using System.Collections.Generic;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Internal class used by <see cref="RouteResolver"/>.
    /// </summary>
    class ProtoResolver
    {
        readonly IActivityMonitor _monitor;

        class Route : IProtoRouteConfigurationContext, IProtoRoute, IProtoSubRoute
        {
            readonly ProtoResolver _resolver;
            readonly Route _parent;
            readonly RouteConfiguration _configuration;
            readonly Dictionary<string,ProtoDeclaredAction> _declaredActions;
            readonly List<MetaConfiguration> _metaConfigurations;
            readonly List<Route> _subRoutes;
            readonly string _namespace;
            readonly string _fullName;
            readonly int _routeDepth;

            Route( RouteConfiguration c, Route parent, ProtoResolver r )
            {
                _resolver = r;
                _parent = parent;
                _configuration = c;
                _routeDepth = 0;
                _namespace = c.Namespace;
                _fullName = c.Namespace + c.Name;
                _declaredActions = new Dictionary<string, ProtoDeclaredAction>();
                _metaConfigurations = new List<MetaConfiguration>();
                _subRoutes = new List<Route>();
            }

            internal Route( ProtoResolver r, RouteConfiguration c )
                : this( c, null, r )
            {
                CheckValidity();
            }

            internal Route( ProtoResolver r, Route parent, SubRouteConfiguration c )
                : this( c, parent, r )
            {
                _routeDepth = parent._routeDepth + 1;
                _namespace = parent._namespace + c.Namespace;
                _fullName = parent._namespace + _fullName;
                CheckValidity();
                if( c.ImportParentDeclaredActionsAbove ) _declaredActions.AddRange( _parent._declaredActions );
                if( c.ImportParentActions ) _metaConfigurations.AddRange( parent._metaConfigurations );
            }

            void CheckValidity()
            {
                foreach( var config in _configuration.Configurations ) config.CheckValidity( _fullName, _resolver._monitor );
            }

            internal void ExecuteMetaConfigurations()
            {
                foreach( var meta in _configuration.Configurations ) meta.Apply( this );
            }

            #region IRouteConfigurationProtoContext

            public IActivityMonitor Monitor { get { return _resolver._monitor; } }

            bool IProtoRouteConfigurationContext.AddRoute( SubRouteConfiguration route )
            {
                if( route == null ) throw new ArgumentNullException();
                var newSub = new Route( _resolver, this, route );
                if( !_resolver.RegisterSubRoute( newSub ) )
                {
                    Monitor.SendLine( LogLevel.Error, string.Format( "Route named '{0}' is already declared.", newSub._fullName ), null );
                    return false;
                }
                using( Monitor.OpenGroup( LogLevel.Info, string.Format( "Preprocessing route '{0}'.", newSub._fullName ), null ) )
                {
                    newSub.ExecuteMetaConfigurations();
                    _subRoutes.Add( newSub );
                }
                return true;
            }

            bool IProtoRouteConfigurationContext.DeclareAction( ActionConfiguration a, bool overridden )
            {
                if( a == null ) throw new ArgumentNullException();
                ProtoDeclaredAction existsHere;
                if( _declaredActions.TryGetValue( a.Name, out existsHere ) )
                {
                    if( overridden )
                    {
                        _declaredActions[a.Name] = new ProtoDeclaredAction( a );
                        Monitor.SendLine( LogLevel.Info, string.Format( "Action '{0}' is overridden", a.Name ), null );
                        return true;
                    }
                    Monitor.SendLine( LogLevel.Error, string.Format( "Action '{0}' is already declared. Use Override to alter it or use another name.", a.Name ), null );
                    return false;
                }
                _declaredActions.Add( a.Name, new ProtoDeclaredAction( a ) );
                return true;
            }

            void IProtoRouteConfigurationContext.AddMeta( MetaConfiguration meta )
            {
                if( meta == null ) throw new ArgumentNullException();
                _metaConfigurations.Add( meta );
            }
            #endregion

            #region IProtoRoute & IProtoSubRoute

            SubRouteConfiguration IProtoSubRoute.Configuration
            {
                get { return (SubRouteConfiguration)_configuration; }
            }

            RouteConfiguration IProtoRoute.Configuration
            {
                get { return _configuration; }
            }

            string IProtoRoute.Namespace
            {
                get { return _namespace; }
            }

            string IProtoRoute.FullName
            {
                get { return _fullName; }
            }

            IReadOnlyList<MetaConfiguration> IProtoRoute.MetaConfigurations
            {
                get { return _metaConfigurations; }
            }

            ActionConfiguration IProtoRoute.FindDeclaredAction( string name )
            {
                var d = _declaredActions.GetValueWithDefault( name, null );
                return d != null ? d.Action : null;
            }

            IReadOnlyList<IProtoSubRoute> IProtoRoute.SubRoutes
            {
                get { return _subRoutes; }
            }

            #endregion
        }

        public ProtoResolver( IActivityMonitor monitor, RouteConfiguration root )
        {
            _monitor = monitor;
            NamedSubRoutes = new Dictionary<string, IProtoSubRoute>();
            Route r = new Route( this, root );
            r.ExecuteMetaConfigurations();
            Root = r;
        }

        public readonly IProtoRoute Root;

        public readonly Dictionary<string,IProtoSubRoute> NamedSubRoutes;

        bool RegisterSubRoute( IProtoSubRoute r )
        {
            if( NamedSubRoutes.ContainsKey( r.FullName ) ) return false;
            NamedSubRoutes.Add( r.FullName, r );
            return true;
        }

    }
}
