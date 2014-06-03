using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Redefines an existing <see cref="ActionConfiguration"/>.
    /// </summary>
    public class MetaOverrideActionConfiguration : Impl.MetaMultiConfiguration<ActionConfiguration>
    {
        public MetaOverrideActionConfiguration( ActionConfiguration action, params ActionConfiguration[] otherActions )
            : base( action, otherActions )
        {
        }

        public IReadOnlyList<ActionConfiguration> Actions
        {
            get { return base.Items; }
        }

        public MetaOverrideActionConfiguration Override( ActionConfiguration action )
        {
            base.Add( action );
            return this;
        }

        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            bool result = true;
            // If we override in a composite we will allow name with / inside.
            foreach( var a in Actions ) result &= CheckActionNameValidity( routeName, monitor, a.Name ) && a.CheckValidity( routeName, monitor );
            return result;
        }

        /// <summary>
        /// This method declares the action as being overridden.
        /// </summary>
        /// <param name="protoContext">The temporary context used to build the routes.</param>
        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            foreach( var a in Actions ) protoContext.DeclareAction( a, overridden: true );
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
        }
    }
}
