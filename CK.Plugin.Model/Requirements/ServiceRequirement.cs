#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Requirements\ServiceRequirement.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using System.Xml;

namespace CK.Plugin
{
    /// <summary>
    /// Represents the state that a <see cref="RequirementLayer"/> (Context, Keboard, Layout) requires for a specific service.
    /// </summary>
    public class ServiceRequirement
    {
        ServiceRequirementCollection _holder;
        RunningRequirement _req;

        internal ServiceRequirement NextElement;

        /// <summary>
        /// Full name of the required service.
        /// </summary>
        public string AssemblyQualifiedName { get; private set; }

        /// <summary>
        /// Gets the <see cref="RunningRequirement"/> corresponding to this ServiceRequirement instance.
        /// </summary>
        public RunningRequirement Requirement
        {
            get { return _req; }
            internal set { _req = value; }
        }

        internal ServiceRequirementCollection Holder
        {
            get { return _holder; }
            set { _holder = value; }
        }

        internal ServiceRequirement( ServiceRequirementCollection holder )
        {
            Debug.Assert( holder != null );
            _holder = holder;
        }

        internal ServiceRequirement( ServiceRequirementCollection holder, string serviceAssemblyQualifiedName, RunningRequirement requirement )
            : this( holder )
        {
            Debug.Assert( serviceAssemblyQualifiedName != null );
            AssemblyQualifiedName = serviceAssemblyQualifiedName;
            _req = requirement;
        }
    }
}
