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

        public SubRouteConfiguration( string name, Func<string,bool> routePredicate )
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
        /// Gets or sets whether actions declared by parent routes before this one are available to this route: <see cref="Insert"/> can reference
        /// any action declared before (and above).
        /// Defaults to true: by default a sub route can reuse action declared before and above. Setting this to false implies that <see cref="ImportParentActions"/>
        /// is also false.
        /// </summary>
        public bool ImportParentDeclaredActionsAbove
        {
            get { return _importParentDeclaredActionsAbove; }
            set { _importParentDeclaredActionsAbove = value; }
        }

        public new SubRouteConfiguration AddAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            base.AddAction( a, otherActions );
            return this;
        }

        public new SubRouteConfiguration DeclareAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            base.DeclareAction( a, otherActions );
            return this;
        }

        public new SubRouteConfiguration OverrideAction( ActionConfiguration a, params ActionConfiguration[] otherActions )
        {
            base.OverrideAction( a, otherActions );
            return this;
        }

        public new SubRouteConfiguration RemoveAction( string name, params string[] otherNames )
        {
            base.RemoveAction( name, otherNames );
            return this;
        }

        public new SubRouteConfiguration InsertAction( string name, string declarationName )
        {
            base.InsertAction( name, declarationName );
            return this;
        }

        public new SubRouteConfiguration DeclareRoute( SubRouteConfiguration channel )
        {
            base.DeclareRoute( channel );
            return this;
        }

    }
}
