using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Removes one or more actions that have been inserted before.
    /// </summary>
    public class MetaRemoveActionConfiguration : Impl.MetaMultiConfiguration<string>
    {
        public MetaRemoveActionConfiguration( string nameToRemove, params string[] otherNames )
            : base( nameToRemove, otherNames )
        {
        }

        /// <summary>
        /// Gets the names of the actions that must be removed.
        /// </summary>
        public IReadOnlyList<string> NamesToRemove
        {
            get { return base.Items; }
        }

        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            return true;
        }

        public MetaRemoveActionConfiguration Remove( string nameToRemove )
        {
            base.Add( nameToRemove );
            return this;
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
            foreach( var n in NamesToRemove ) context.RemoveAction( n );
        }
    }
}
