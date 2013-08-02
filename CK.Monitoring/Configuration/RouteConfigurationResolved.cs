using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    public class RouteConfigurationResolved
    {
        readonly string _name;
        readonly IReadOnlyList<ActionConfigurationResolved> _actions;
        IReadOnlyList<SubRouteConfigurationResolved> _routes;

        internal RouteConfigurationResolved( string name, IReadOnlyList<ActionConfigurationResolved> actions )
        {
            _name = name;
            _actions = actions;
        }

        /// <summary>
        /// Gets the name of the route.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the subordinated routes that this route contains.
        /// </summary>
        public IReadOnlyList<SubRouteConfigurationResolved> SubRoutes 
        {
            get { return _routes; }
            internal set { _routes = value; }
        }

        /// <summary>
        /// Gets the actions that apply to this route.
        /// </summary>
        public IReadOnlyList<ActionConfigurationResolved> ActionsResolved { get { return _actions; } }
    }

}
