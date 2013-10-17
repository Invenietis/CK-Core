using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig.Impl;

namespace CK.RouteConfig
{
    /// <summary>
    /// Primary configuration object that contains multiple <see cref="SubRouteConfiguration"/>s and <see cref="ActionConfiguration"/>s.
    /// </summary>
    public class RouteConfiguration
    {
        readonly string _name;
        readonly List<MetaConfiguration> _configurations;
        string _namespace;
        object _configData;

        /// <summary>
        /// Initializes a new root <see cref="RouteConfiguration"/>.
        /// </summary>
        public RouteConfiguration()
            : this( String.Empty )
        {
        }

        /// <summary>
        /// Initializes a specialized <see cref="RouteConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of the route. Can not be null.</param>
        protected RouteConfiguration( string name )
        {
            if( name == null ) throw new ArgumentNullException( "name" );
            _name = name;
            _configurations = new List<MetaConfiguration>();
            _namespace = String.Empty;
        }

        /// <summary>
        /// Gets the name of this configuration.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets or sets any configuration data for this route.
        /// </summary>
        public object ConfigData 
        {
            get { return _configData; }
            set { _configData = value; } 
        }

        /// <summary>
        /// Gets or sets an optional namespace for this route: declared <see cref="SubRouteConfiguration"/>
        /// are automatically prefixed with this namespace. Never null.
        /// This namespace has no effect on any <see cref="ActionConfiguration.Name"/>.
        /// </summary>
        public string Namespace 
        { 
            get { return _namespace; } 
            set { _namespace = value ?? String.Empty; } 
        }

        public IReadOnlyList<MetaConfiguration> Configurations { get { return _configurations.AsReadOnlyList(); } }

        public RouteConfiguration AddAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            _configurations.Add( new MetaAddActionConfiguration( a, otherActions ) );
            return this;
        }

        public RouteConfiguration DeclareAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            _configurations.Add( new MetaDeclareActionConfiguration( a, otherActions ) );
            return this;
        }

        public RouteConfiguration OverrideAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            _configurations.Add( new MetaOverrideActionConfiguration( a, otherActions ) );
            return this;
        }

        public RouteConfiguration RemoveAction( string name, params string[] otherNames )
        {
            _configurations.Add( new MetaRemoveActionConfiguration( name, otherNames ) );
            return this;
        }

        public RouteConfiguration InsertAction( string name, string declarationName )
        {
            _configurations.Add( new MetaInsertActionConfiguration( name, declarationName ) );
            return this;
        }

        public RouteConfiguration DeclareRoute( SubRouteConfiguration route )
        {
            _configurations.Add( new MetaDeclareRouteConfiguration( route ) );
            return this;
        }

        protected void AddMeta( MetaConfiguration m )
        {
            _configurations.Add( m );
        }

        /// <summary>
        /// Attempts to resolve the configuration. Null if an error occured.
        /// </summary>
        /// <param name="monitor">Monitor to use. Must not be null nor the empty monitor.</param>
        /// <returns>Null or a set of resolved route configuration.</returns>
        public RouteConfigurationResult Resolve( IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            int errorCount = 0;
            RouteConfigurationResult result;
            using( monitor.CatchCounter( e => errorCount = e ) )
            {
                var r = new RouteResolver( monitor, this );
                result = new RouteConfigurationResult( r.Root, r.NamedSubRoutes );
            }
            return errorCount == 0 ? result : null;
        }

    }
}
