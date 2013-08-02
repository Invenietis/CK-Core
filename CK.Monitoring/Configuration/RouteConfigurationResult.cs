using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    public class RouteConfigurationResult
    {
        readonly RouteConfigurationResolved _root;
        readonly Dictionary<string, SubRouteConfigurationResolved> _namedSubRoutes;
        readonly IReadOnlyCollection<SubRouteConfigurationResolved> _namedSubRoutesEx;

        internal RouteConfigurationResult( RouteConfigurationResolved root, Dictionary<string, SubRouteConfigurationResolved> namedSubRoutes )
        {
            _root = root;
            _namedSubRoutes = namedSubRoutes;
            _namedSubRoutesEx = new CKReadOnlyCollectionOnICollection<SubRouteConfigurationResolved>( _namedSubRoutes.Values );
        }

        public RouteConfigurationResolved Root
        {
            get { return _root; }
        } 

        public IReadOnlyCollection<SubRouteConfigurationResolved> SubRoutes
        {
            get { return _namedSubRoutesEx; }
        }

        public SubRouteConfigurationResolved FindSubRouteByName( string name )
        {
            return _namedSubRoutes.GetValueWithDefault( name, null );
        }

    }
}
