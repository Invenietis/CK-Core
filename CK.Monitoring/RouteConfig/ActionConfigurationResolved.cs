#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\ActionConfigurationResolved.cs) is part of CiviKey. 
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

namespace CK.RouteConfig
{
    /// <summary>
    /// Resolved action configurations belong to <see cref="RouteConfigurationResolved"/> and <see cref="SubRouteConfigurationResolved"/>.
    /// </summary>
    public class ActionConfigurationResolved
    {
        readonly  string _fullName;
        readonly IReadOnlyList<string> _path;
        readonly  ActionConfiguration _action;
        readonly  int _index;

        internal ActionConfigurationResolved( int index, IReadOnlyList<string> path, ActionConfiguration a )
        {
            _index = index;
            _path = path;
            _action = a;
            _fullName = String.Join( "/", path.Append( a.Name ) );
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string Name { get { return _action.Name; } }

        /// <summary>
        /// Gets the full name of the action (this <see cref="Path"/>/<see cref="Name"/>).
        /// </summary>
        public string FullName { get { return _fullName; } }

        /// <summary>
        /// Path of this action.
        /// </summary>
        public IReadOnlyList<string> Path { get { return _path; } }
        
        /// <summary>
        /// Gets the index of this action in its route or in its composite.
        /// </summary>
        public int Index { get { return _index; } }

        /// <summary>
        /// Gets the action configuration instance (possibly shared by multiple routes if the <see cref="ActionConfiguration"/>.<see cref="P:ActionConfiguration.IsCloneable"/> is false).
        /// </summary>
        public ActionConfiguration ActionConfiguration { get { return _action; } }

        /// <summary>
        /// Internal factory for ActionConfigurationResolved avoids externally visible virtual protected method on ActionConfiguration.
        /// This prevents any other composite implementations than our.
        /// </summary>
        internal static ActionConfigurationResolved Create( IActivityMonitor monitor, ActionConfiguration a, bool flattenUselessComposite, int index = 0, IReadOnlyList<string> path = null )
        {
            if( path == null ) path = CKReadOnlyListEmpty<string>.Empty;
            Impl.ActionCompositeConfiguration c = a as Impl.ActionCompositeConfiguration;
            if( c != null ) return new Impl.ActionCompositeConfigurationResolved( monitor, index, path, c, flattenUselessComposite );
            return new ActionConfigurationResolved( index, path, a );
        }


    }
}
