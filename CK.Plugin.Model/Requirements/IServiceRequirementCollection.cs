using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Service requirements associates services identifier (the assemnly qualified name 
    /// of the <see cref="IDynamicService"/> type) to <see cref="RunningRequirement"/>.
    /// </summary>
    public interface IServiceRequirementCollection : IReadOnlyCollection<ServiceRequirement>
    {
        /// <summary>
        /// Fires before a change occurs.
        /// </summary>
        event EventHandler<ServiceRequirementCollectionChangingEventArgs> Changing;
        
        /// <summary>
        /// Fires when a requirement is updated.
        /// </summary>
        event EventHandler<ServiceRequirementCollectionChangedEventArgs> Changed;

        /// <summary>
        /// Add or set the given requirement to the given service (by its fullname).
        /// </summary>
        /// <param name="serviceAssemblyQualifiedName">AssemblyQualifiedName of the service to add or update.</param>
        /// <param name="requirement">Requirement to add or set.</param>
        /// <returns>New or updated requirement. May be unchanged if <see cref="Changing"/> canceled the action.</returns>
        ServiceRequirement AddOrSet( string serviceAssemblyQualifiedName, RunningRequirement requirement );

        /// <summary>
        /// Gets the <see cref="ServiceRequirement"/> for the given service (by its fullname).
        /// </summary>
        /// <param name="serviceAssemblyQualifiedName">AssemblyQualifiedName of the service to find.</param>
        /// <returns>Found requirement (if any, null otherwise).</returns>
        ServiceRequirement Find( string serviceAssemblyQualifiedName );

        /// <summary>
        /// Removes the given service requirement.
        /// When no explicit requirement exists, <see cref="RunningRequirement.Optional"/> is the default.
        /// </summary>
        /// <param name="serviceAssemblyQualifiedName">AssemblyQualifiedName of the service requirement to remove.</param>
        /// <returns>True if the element does not exist or has been successfully removed. False if <see cref="Changing"/> canceled the action.</returns>
        bool Remove( string serviceAssemblyQualifiedName );

        /// <summary>
        /// Clears all requirements.
        /// </summary>
        bool Clear();
    }
}
