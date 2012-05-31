#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Requirements\IServiceRequirementCollection.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
