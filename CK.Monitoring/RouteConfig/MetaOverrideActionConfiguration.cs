using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Overrides, redefines, one or more <see cref="ActionConfiguration"/>: based on their names they will replace previously defined actions.
    /// </summary>
    public class MetaOverrideActionConfiguration : Impl.MetaMultiConfiguration<ActionConfiguration>
    {
        /// <summary>
        /// Initializes a new <see cref="MetaOverrideActionConfiguration"/> with at least one action to override.
        /// </summary>
        /// <param name="action">First, required, action to override.</param>
        /// <param name="otherActions">Optional other actions to override.</param>
        public MetaOverrideActionConfiguration( ActionConfiguration action, params ActionConfiguration[] otherActions )
            : base( action, otherActions )
        {
        }

        /// <summary>
        /// Gets the <see cref="ActionConfiguration"/>s that must be overridden.
        /// </summary>
        public IReadOnlyList<ActionConfiguration> Actions
        {
            get { return base.Items; }
        }

        /// <summary>
        /// Adds an <see cref="ActionConfiguration"/> that must be overridden.
        /// </summary>
        /// <param name="action">The action that will override an existing one with the same name.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public MetaOverrideActionConfiguration Override( ActionConfiguration action )
        {
            base.Add( action );
            return this;
        }

        /// <summary>
        /// Checks the validity: each action's name must be valid and each <see cref="ActionConfiguration.CheckValidity"/> must return true.
        /// </summary>
        /// <param name="routeName">Name of the route that contains this configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>True if valid, false otherwise.</returns>
        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            bool result = true;
            // If we override in a composite we will allow name with / inside.
            foreach( var a in Actions ) result &= CheckActionNameValidity( routeName, monitor, a.Name ) && a.CheckValidity( routeName, monitor );
            return result;
        }

        /// <summary>
        /// This method declares the <see cref="Actions"/> as being overridden by calling <see cref="Impl.IProtoRouteConfigurationContext.DeclareAction"/>
        /// with the overridden parameter sets to true.
        /// </summary>
        /// <param name="protoContext">The temporary context used to build the routes.</param>
        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            foreach( var a in Actions ) protoContext.DeclareAction( a, overridden: true );
        }

        /// <summary>
        /// Never called: the first <see cref="Apply(Impl.IProtoRouteConfigurationContext)"/> does not register this object
        /// since we have nothing more to do than declaring overridden actions.
        /// </summary>
        /// <param name="context">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
        }
    }
}
