using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.ComponentModel;

namespace CK.Plugin
{
    /// <summary>
    /// Plugin requirements associates plugin <see cref="Guid"/> to <see cref="RunningRequirement"/>.
    /// </summary>
    public interface IPluginRequirementCollection : IReadOnlyCollection<PluginRequirement>
    {
        /// <summary>
        /// Fires before a change occurs.
        /// </summary>
        event EventHandler<PluginRequirementCollectionChangingEventArgs> Changing;

        /// <summary>
        /// Fires when a change occured.
        /// </summary>
        event EventHandler<PluginRequirementCollectionChangedEventArgs> Changed;

        /// <summary>
        /// Add or set the given requirement to the given plugin (by its UniqueID).
        /// </summary>
        /// <param name="pluginId">Identifier of the plugin to configure.</param>
        /// <param name="requirement">Requirement to add or set.</param>
        /// <returns>New or updated requirement. May be unchanged if <see cref="Changing"/> canceled the action.</returns>
        PluginRequirement AddOrSet( Guid pluginId, RunningRequirement requirement );
        
        /// <summary>
        /// Gets the <see cref="PluginRequirement"/> for the given plugin (by its unique identifier).
        /// </summary>
        /// <param name="pluginId">Unique identifier of the plugin to find.</param>
        /// <returns>Found requirement (if any, null otherwise).</returns>
        PluginRequirement Find( Guid pluginId );

        /// <summary>
        /// Removes the given requirement.
        /// When no explicit requirement exists, <see cref="RunningRequirement.Optional"/> is the default.
        /// </summary>
        /// <param name="pluginId">Unique identifier of the plugin to remove.</param>
        /// <returns>True if the element does not exist or has been successfully removed. False if <see cref="Changing"/> canceled the action.</returns>
        bool Remove( Guid pluginId );

        /// <summary>
        /// Clears all requirements.
        /// </summary>
        bool Clear();
    }
}
