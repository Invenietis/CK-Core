using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig
{
    /// <summary>
    /// Specialized composite for parallel configuration.
    /// </summary>
    public class ActionParallelConfiguration : Impl.ActionCompositeConfiguration
    {
        /// <summary>
        /// Initializes a new parallel.
        /// </summary>
        /// <param name="name">Name of the configuration.</param>
        public ActionParallelConfiguration( string name )
            : base( name, true )
        {
        }

        /// <summary>
        /// Overridden to return a this as a <see cref="ActionParallelConfiguration"/> for fluent syntax.
        /// </summary>
        /// <param name="a">The action to add.</param>
        /// <returns>This parallel.</returns>
        public ActionParallelConfiguration AddAction( ActionConfiguration a )
        {
            base.Add( a );
            return this;
        }
    }
}
