using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Encapsulates the resolution of routes configuration: the <see cref="RouteConfiguration.Resolve"/> method computes it.
    /// </summary>
    public class RouteConfigurationResult
    {
        readonly RouteConfigurationResolved _root;
        readonly Dictionary<string, SubRouteConfigurationResolved> _namedSubRoutes;
        readonly IReadOnlyCollection<SubRouteConfigurationResolved> _namedSubRoutesEx;

        internal RouteConfigurationResult( RouteConfigurationResolved root, Dictionary<string, SubRouteConfigurationResolved> namedSubRoutes )
        {
            _root = root;
            _namedSubRoutes = namedSubRoutes;
            #if NET451
            _namedSubRoutesEx = new CKReadOnlyCollectionOnICollection<SubRouteConfigurationResolved>( _namedSubRoutes.Values );
            #else
            _namedSubRoutesEx = _namedSubRoutes.Values;
            #endif
        }

        /// <summary>
        /// Gets the resolved root route.
        /// </summary>
        public RouteConfigurationResolved Root
        {
            get { return _root; }
        } 

        /// <summary>
        /// Gets all the subordinated routes.
        /// </summary>
        public IReadOnlyCollection<SubRouteConfigurationResolved> AllSubRoutes
        {
            get { return _namedSubRoutesEx; }
        }

        /// <summary>
        /// Finds a subordinated route by its name.
        /// </summary>
        /// <param name="name">Name of the route.</param>
        /// <returns>The route or null if it does not exist.</returns>
        public SubRouteConfigurationResolved FindSubRouteByName( string name )
        {
            return _namedSubRoutes.GetValueWithDefault( name, null );
        }

    }
}
