#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\ActionCompositeConfigurationResolved.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    class ActionCompositeConfigurationResolved : ActionConfigurationResolved
    {
        readonly List<ActionConfigurationResolved> _children;
        readonly bool _isParallel;
        const string _seqDisplayName = "Sequence";
        const string _parDisplayName = "Parallel";

        internal ActionCompositeConfigurationResolved( IActivityMonitor monitor, int index, IReadOnlyList<string> path, ActionCompositeConfiguration a, bool flattenUselessComposite )
            : base( index, path, a )
        {
            _isParallel = a.IsParallel;
            _children = new List<ActionConfigurationResolved>();
            AppendChildren( monitor, a, path.Append( a.Name ).ToReadOnlyList(), flattenUselessComposite );
        }

        void AppendChildren( IActivityMonitor monitor, ActionCompositeConfiguration a, IReadOnlyList<string> childPath, bool flattenUselessComposite )
        {
            foreach( var child in a.Children )
            {
                ActionCompositeConfiguration composite = child as ActionCompositeConfiguration;
                if( flattenUselessComposite && composite != null && composite.IsParallel == a.IsParallel )
                {
                    AppendChildren( monitor, composite, childPath = childPath.Append( composite.Name ).ToReadOnlyList(), true );
                }
                else _children.Add( ActionConfigurationResolved.Create( monitor, child, flattenUselessComposite, _children.Count, childPath ) );
            }
        }

        public new ActionCompositeConfiguration ActionConfiguration { get { return (ActionCompositeConfiguration)base.ActionConfiguration; } }

        public IReadOnlyList<ActionConfigurationResolved> Children { get { return _children.AsReadOnlyList(); } }


     }
}
