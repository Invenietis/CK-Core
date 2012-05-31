#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Requirements\Events\ServiceRequirementCollectionChangingEventArgs.cs) is part of CiviKey. 
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
using System.ComponentModel;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Describes a change that is about to occur in a <see cref="IServiceRequirementCollection"/> and can be <see cref="CancelEventArgs.Cancel"/>ed.
    /// </summary>
    public class ServiceRequirementCollectionChangingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The <see cref="ChangeStatus"/> that synthetizes the change.
        /// </summary>
        public ChangeStatus Action { get; private set; }

        /// <summary>
        /// The source <see cref="IServiceRequirementCollection"/> that is changing.
        /// </summary>
        public IServiceRequirementCollection Collection { get; private set; }

        /// <summary>
        /// The service identifier for which a change is occurring. 
        /// It is null if the change is a global change (<see cref="IServiceRequirementCollection.Clear"/> is beeing called for instance).
        /// </summary>
        public string AssemblyQualifiedName { get; private set; }

        /// <summary>
        /// The <see cref="RunningRequirement"/> that is changing.
        /// It is <see cref="RunningRequirement.Optional"/> if the change is a global change (<see cref="IPluginRequirementCollection.Clear"/> is beeing called for instance).
        /// </summary>
        public RunningRequirement Requirement { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="ServiceRequirementCollectionChangingEventArgs"/>.
        /// </summary>
        /// <param name="c">The collection that is changing.</param>
        /// <param name="action">The <see cref="ChangeStatus"/>.</param>
        /// <param name="assemblyQualifiedName">The service identifier concerned.</param>
        /// <param name="requirement">The <see cref="RunningRequirement"/> of the changing service.</param>
        public ServiceRequirementCollectionChangingEventArgs( IServiceRequirementCollection c, ChangeStatus action, string assemblyQualifiedName, RunningRequirement requirement )
        {
            Collection = c;
            Action = action;
            AssemblyQualifiedName = assemblyQualifiedName;
            Requirement = requirement;
        }
    }
}
