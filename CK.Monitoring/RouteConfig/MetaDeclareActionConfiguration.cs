using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Declares one or more <see cref="ActionConfiguration"/> but does not actually insert them.
    /// </summary>
    public class MetaDeclareActionConfiguration : Impl.MetaMultiConfiguration<ActionConfiguration>
    {
        /// <summary>
        /// Initializes a <see cref="MetaDeclareActionConfiguration"/> with at least one action to declare.
        /// </summary>
        /// <param name="action">First, required, action to declare.</param>
        /// <param name="otherActions">Optional other actions to declare.</param>
        public MetaDeclareActionConfiguration( ActionConfiguration action, params ActionConfiguration[] otherActions )
            : base( action, otherActions )
        {
        }

        /// <summary>
        /// Gets the <see cref="ActionConfiguration"/>s that must be declared.
        /// </summary>
        public IReadOnlyList<ActionConfiguration> Actions
        {
            get { return base.Items; }
        }

        /// <summary>
        /// Declares an <see cref="ActionConfiguration"/>.
        /// </summary>
        /// <param name="action">The action to declare.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public MetaDeclareActionConfiguration Declare( ActionConfiguration action )
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
            foreach( var a in Actions ) result &= CheckActionNameValidity( routeName, monitor, a.Name ) && a.CheckValidity( routeName, monitor );
            return result;
        }

        /// <summary>
        /// Applies the configuration (first step) by <see cref="Impl.IProtoRouteConfigurationContext.DeclareAction">declaring</see> all the <see cref="Actions"/>.
        /// </summary>
        /// <param name="protoContext">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            foreach( var a in Actions ) protoContext.DeclareAction( a, overridden: false );
        }

        /// <summary>
        /// Never called: the first <see cref="Apply(Impl.IProtoRouteConfigurationContext)"/> does not register this object
        /// since we have nothing more to do than declaring actions.
        /// </summary>
        /// <param name="context">Enables context lookup and manipulation, exposes a <see cref="IActivityMonitor"/> to use.</param>
        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
        }
    }
}
