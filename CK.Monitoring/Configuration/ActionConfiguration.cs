using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Configuration for an action defines a <see cref="Name"/>.
    /// This name must be unique among its sequence.
    /// </summary>
    public abstract class ActionConfiguration
    {
        readonly string _name;

        /// <summary>
        /// Initializes a new <see cref="ActionConfiguration"/>.
        /// </summary>
        /// <param name="name">Required non empty name that identifies this configuration.</param>
        protected ActionConfiguration( string name )
        {
            _name = name ?? String.Empty;
        }

        /// <summary>
        /// Gets the name of this configuration. Can not be null.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Checks the configuration validity. By default returns true.
        /// </summary>
        /// <param name="routeName">Name of the route that contains the configuration.</param>
        /// <param name="monitor">Monitor to use to explain errors.</param>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        public virtual bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            return true;
        }

        /// <summary>
        /// Gets whether this configuration is cloneable.
        /// Defaults to false.
        /// </summary>
        public virtual bool IsCloneable
        {
            get { return false; }
        }

        /// <summary>
        /// Clones this configuration.
        /// By default throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>A clone.</returns>
        public virtual ActionConfiguration Clone()
        {
            throw new NotSupportedException();
        }

    }
}
