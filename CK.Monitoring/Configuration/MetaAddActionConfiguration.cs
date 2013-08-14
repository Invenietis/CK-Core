using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Declares one or more <see cref="ActionConfiguration"/> and inserts them with their names.
    /// </summary>
    public class MetaAddActionConfiguration : Impl.MetaMultiConfiguration<ActionConfiguration>
    {
        public MetaAddActionConfiguration( ActionConfiguration action, params ActionConfiguration[] otherActions )
            : base( action, otherActions )
        {
        }

        /// <summary>
        /// Gets the <see cref="ActionConfiguration"/>s that must be declared and inserted with their names.
        /// </summary>
        public IReadOnlyList<ActionConfiguration> Actions
        {
            get { return base.Items; }
        }

        public new MetaAddActionConfiguration Add( ActionConfiguration action )
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
            protoContext.AddMeta( this );
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
            foreach( var a in Actions ) context.AddDeclaredAction( a.Name, a.Name, fromDeclaration: true );
        }
    }
}
