using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Declares a new <see cref="ActionConfiguration"/>.
    /// </summary>
    public class MetaDeclareActionConfiguration : Impl.MetaMultiConfiguration<ActionConfiguration>
    {
        public MetaDeclareActionConfiguration( ActionConfiguration action, params ActionConfiguration[] otherActions )
            : base( action, otherActions )
        {
        }

        public IReadOnlyList<ActionConfiguration> Actions
        {
            get { return base.Items; }
        }

        public MetaDeclareActionConfiguration Declare( ActionConfiguration action )
        {
            base.Add( action );
            return this;
        }

        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            bool result = true;
            foreach( var a in Actions ) result &= CheckActionNameValidity( routeName, monitor, a.Name ) && a.CheckValidity( routeName, monitor );
            return result;
        }

        protected internal override void Apply( Impl.IProtoRouteConfigurationContext protoContext )
        {
            foreach( var a in Actions ) protoContext.DeclareAction( a, overridden: false );
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
        }
    }
}
