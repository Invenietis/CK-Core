using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Supports for route resolution. The final <see cref="CurrentActions"/> are embedded into <see cref="RouteConfigurationResolved"/>.
    /// </summary>
    public interface IRouteConfigurationContext
    {
        /// <summary>
        /// Gets the monitor to use.
        /// </summary>
        IActivityMonitor Monitor { get; }

        /// <summary>
        /// Gets the intermediate route object.
        /// </summary>
        IProtoRoute ProtoRoute { get; }

        /// <summary>
        /// Gets the list of <see cref="ActionConfigurationResolved"/>.
        /// </summary>
        IEnumerable<ActionConfigurationResolved> CurrentActions { get; }

        /// <summary>
        /// Finds an existing action by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ActionConfigurationResolved FindExisting( string name );

        /// <summary>
        /// Removes a named action by name.
        /// </summary>
        /// <param name="name">Name of the action to remove.</param>
        /// <returns>True if the action has been found and removed. False otherwise.</returns>
        bool RemoveAction( string name );

        /// <summary>
        /// Adds an action that has been previously declared or not: 
        /// <paramref name="fromMetaInsert"/> is true if we insert actions from <see cref="MetaInsertActionConfiguration"/>. 
        /// It is false if the action is added by a direct <see cref="MetaAddActionConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of the action to add.</param>
        /// <param name="declaredName">Name of the declared action.</param>
        /// <param name="fromMetaInsert">True if this is from <see cref="MetaInsertActionConfiguration"/>, false when called by <see cref="MetaAddActionConfiguration"/>.</param>
        /// <returns>True if the action has been found and added.</returns>
        bool AddDeclaredAction( string name, string declaredName, bool fromMetaInsert );
    }

}
