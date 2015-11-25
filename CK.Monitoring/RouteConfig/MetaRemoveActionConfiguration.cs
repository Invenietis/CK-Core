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
        /// <summary>
        /// Initializes a <see cref="MetaRemoveActionConfiguration"/> with at least one action to remove.
        /// </summary>
        /// <param name="nameToRemove">Name of the first action to remove.</param>
        /// <param name="otherNames">Other names to remove.</param>
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

        /// <summary>
        /// Check the configuration validity: always true.
        /// </summary>
        /// <param name="routeName">Name of the route that contains this configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>Always true.</returns>
        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            return true;
        }

        /// <summary>
        /// Adds a name to the <see cref="NamesToRemove"/>.
        /// </summary>
        /// <param name="nameToRemove">New name to remove.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public MetaRemoveActionConfiguration Remove( string nameToRemove )
        {
            base.Add( nameToRemove );
            return this;
        }

        /// <summary>
        /// Applies this configuration by removing all actions in <see cref="NamesToRemove"/> from the context.
        /// </summary>
        /// <param name="context">The context to modify.</param>
        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
            foreach( var n in NamesToRemove ) context.RemoveAction( n );
        }
    }
}
