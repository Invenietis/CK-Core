#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\RunnerRequirementsSnapshot.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using CK.Core;
using System.Collections;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    public class RunnerRequirementsSnapshot : IReadOnlyCollection<RequirementLayerSnapshot>
    {
        IList<RequirementLayerSnapshot> _layers;
        Dictionary<object,SolvedConfigStatus> _final;

        internal RunnerRequirementsSnapshot( RunnerRequirements cfg )
        {
            _layers = new RequirementLayerSnapshot[ cfg.Count ];
            int i = 0;
            foreach( RequirementLayer l in cfg ) _layers[i++] = new RequirementLayerSnapshot( l );
            _final = cfg.CreateFinalConfigSnapshot();
        }

        internal Dictionary<object, SolvedConfigStatus> FinalConfigSnapshot 
        { 
            get { return _final; } 
        }
 
        public SolvedConfigStatus FinalRequirement( Guid pluginId )
        {
            return _final.GetValueWithDefault( pluginId, SolvedConfigStatus.Optional );
        }

        public SolvedConfigStatus FinalRequirement( string serviceFullName )
        {
            return _final.GetValueWithDefault( serviceFullName, SolvedConfigStatus.Optional );
        }
        
        public bool Contains( object item )
        {
            RequirementLayerSnapshot s = item as RequirementLayerSnapshot;
            return s != null ? _layers.IndexOf( s ) >= 0 : false;
        }

        public int Count
        {
            get { return _layers.Count; }
        }

        public IEnumerator<RequirementLayerSnapshot> GetEnumerator()
        {
            return _layers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _layers.GetEnumerator();
        }

    }
}
