using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig
{
    /// <summary>
    /// Specialized composite for sequence configuration.
    /// </summary>
    public class ActionSequenceConfiguration : Impl.ActionCompositeConfiguration
    {
        /// <summary>
        /// Initializes a new sequence.
        /// </summary>
        /// <param name="name">Name of the configuration.</param>
        public ActionSequenceConfiguration( string name )
            : base( name, false )
        {
        }

        /// <summary>
        /// Overridden to return a this as a <see cref="ActionSequenceConfiguration"/> for fluent syntax.
        /// </summary>
        /// <param name="a">The action to add.</param>
        /// <returns>This sequence.</returns>
        public ActionSequenceConfiguration AddAction( ActionConfiguration a )
        {
            base.Add( a );
            return this;
        }
    }
}
