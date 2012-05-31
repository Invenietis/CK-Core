#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\PluginModelExtension.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Carries extension methods for <see cref="N:CK.Plugin"/> interfaces and classes.
    /// </summary>
    public static class PluginModelExtension
    {
        /// <summary>
        /// Adds a <see cref="RequirementLayer"/>. 
        /// The same requirements layer can be added multiple times. 
        /// Only the last (balanced) call to <see cref="PluginModelExtension.Remove(ISimplePluginRunner,RequirementLayer)">Remove</see> will actually remove the layer.
        /// </summary>
        /// <param name="runner">This <see cref="ISimplePluginRunner"/>.</param>
        /// <param name="r">The requirements layer to add.</param>
        public static void Add( this ISimplePluginRunner runner, RequirementLayer r )
        {
            runner.Add( r, true );
        }

        /// <summary>
        /// Removes one <see cref="RequirementLayer"/>. 
        /// Use <see cref="ISimplePluginRunner.Remove(RequirementLayer,bool)"/> to force the remove regardless of the number of times it has been <see cref="ISimplePluginRunner.Add">added</see>.
        /// </summary>
        /// <param name="runner">This <see cref="ISimplePluginRunner"/>.</param>
        /// <param name="r">The requirements layer to remove.</param>
        /// <returns>True if the layer has been found, false otherwise.</returns>
        public static bool Remove( this ISimplePluginRunner runner, RequirementLayer r )
        {
            return runner.Remove( r, false );
        }

    }
}
