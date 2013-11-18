using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    public class SubRouteConfiguration : RouteConfiguration
    {
        Func<string,bool> _routePredicate;
        bool _importParentActions;
        bool _importParentDeclaredActionsAbove;

        internal SubRouteConfiguration( string name, Func<string, bool> routePredicate )
            : base( name )
        {
            _routePredicate = routePredicate;
            _importParentDeclaredActionsAbove = _importParentActions = true;
        }

        /// <summary>
        /// Gets or sets the filter for this route.
        /// </summary>
        public Func<string, bool> RoutePredicate 
        {
            get { return _routePredicate; }
            set { _routePredicate = value; } 
        }
        
        /// <summary>
        /// Gets or sets whether actions inserted above at the parent level initially apply to this route.
        /// Defaults to true: by default, a route inherits the actions of its parent, setting this to false, makes this route initially empty.
        /// When <see cref="ImportParentDeclaredActionsAbove"/> is set to false, this is always false (one can not reuse actions for which declaration are not available).
        /// </summary>
        public bool ImportParentActions
        {
            get { return _importParentDeclaredActionsAbove && _importParentActions; }
            set { _importParentActions = value; }
        }

        /// <summary>
        /// Gets or sets whether actions declared by parent routes before this one are available to this route: <see cref="InsertAction"/> can reference
        /// any action declared before (and above).
        /// Defaults to true: by default a sub route can reuse action declared before and above. Setting this to false implies that <see cref="ImportParentActions"/>
        /// is also false.
        /// </summary>
        public bool ImportParentDeclaredActionsAbove
        {
            get { return _importParentDeclaredActionsAbove; }
            set { _importParentDeclaredActionsAbove = value; }
        }

        /// <summary>
        /// Adds one or more actions.
        /// </summary>
        /// <param name="a">Action to add.</param>
        /// <param name="otherActions">Optional other actions to add.</param>
        /// <returns>This <see cref="SubRouteConfiguration"/> to enable fluent syntax.</returns>
        public new SubRouteConfiguration AddAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            base.AddAction( a, otherActions );
            return this;
        }

        /// <summary>
        /// Declares one or more actions.
        /// </summary>
        /// <param name="a">Action to declare.</param>
        /// <param name="otherActions">Optional other actions to declare.</param>
        /// <returns>This <see cref="SubRouteConfiguration"/> to enable fluent syntax.</returns>
        public new SubRouteConfiguration DeclareAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            base.DeclareAction( a, otherActions );
            return this;
        }

        /// <summary>
        /// Overrides one or more existing actions.
        /// <see cref="ActionConfiguration.Name"/> is the key.
        /// </summary>
        /// <param name="a">Action to to override.</param>
        /// <param name="otherActions">Optional other actions to override.</param>
        /// <returns>This <see cref="SubRouteConfiguration"/> to enable fluent syntax.</returns>
        public new SubRouteConfiguration OverrideAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            base.OverrideAction( a, otherActions );
            return this;
        }

        /// <summary>
        /// Removes one or more actions by name.
        /// </summary>
        /// <param name="name">Name of the action to remove.</param>
        /// <param name="otherNames">Optional other actions' name to remove.</param>
        /// <returns>This <see cref="SubRouteConfiguration"/> to enable fluent syntax.</returns>
        public new SubRouteConfiguration RemoveAction( string name, params string[] otherNames )
        {
            base.RemoveAction( name, otherNames );
            return this;
        }

        /// <summary>
        /// Inserts one action that have been previously <see cref="DeclareAction">declared</see>.
        /// </summary>
        /// <param name="name">Name of the action to insert.</param>
        /// <param name="declarationName">Name of the declared action.</param>
        /// <returns>This <see cref="SubRouteConfiguration"/> to enable fluent syntax.</returns>
        public new SubRouteConfiguration InsertAction( string name, string declarationName )
        {
            base.InsertAction( name, declarationName );
            return this;
        }

        /// <summary>
        /// Declares a subordinated route.
        /// </summary>
        /// <param name="channel">Configuration of the route.</param>
        /// <returns>This <see cref="SubRouteConfiguration"/> to enable fluent syntax.</returns>
        public new SubRouteConfiguration DeclareRoute( SubRouteConfiguration channel )
        {
            base.DeclareRoute( channel );
            return this;
        }

    }
}
