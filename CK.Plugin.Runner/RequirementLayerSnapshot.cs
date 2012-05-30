#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\RequirementLayerSnapshot.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// 
    /// </summary>
    public class RequirementLayerSnapshot
    {
        public class PluginRequirementIdentifier
        {
            internal RunningRequirement Requirement { get; set; }
            public Guid PluginId { get; internal set; }
        }

        public class ServiceRequirementIdentifier
        {
            internal RunningRequirement Requirement { get; set; }
            public string AssemblyQualifiedName { get; internal set; }
        }

        public string LayerName { get; private set; }

        public IReadOnlyCollection<PluginRequirementIdentifier> PluginRequirements { get; private set; }

        public IReadOnlyCollection<ServiceRequirementIdentifier> ServiceRequirements { get; private set; }

        internal RequirementLayerSnapshot( RequirementLayer l )
        {
            LayerName = l.LayerName;

            var plugins = l.PluginRequirements.Select( ( r, idx ) => new PluginRequirementIdentifier() { PluginId = r.PluginId, Requirement = r.Requirement } ).ToArray();
            PluginRequirements = new ReadOnlyCollectionOnICollection<PluginRequirementIdentifier>( plugins );

            var services = l.ServiceRequirements.Select( ( r, idx ) => new ServiceRequirementIdentifier() { AssemblyQualifiedName = r.AssemblyQualifiedName, Requirement = r.Requirement } ).ToArray();
            ServiceRequirements = new ReadOnlyCollectionOnICollection<ServiceRequirementIdentifier>( services );
        }
    }
}
