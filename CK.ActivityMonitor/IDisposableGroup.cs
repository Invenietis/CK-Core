#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\IDisposableGroup.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
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

namespace CK.Core
{
    /// <summary>
    /// Interface obtained once a Group has been opened.
    /// </summary>
    public interface IDisposableGroup : IDisposable
    {
        /// <summary>
        /// Sets a temporary topic associated to this group.
        /// The current monitor's topic will be automatically restored when group will be closed.
        /// </summary>
        /// <param name="topicOtherThanGroupText">Explicit topic it it must differ from the group's text.</param>
        /// <returns>This object in order to call <see cref="ConcludeWith"/> or to dispose it to close the group.</returns>
        IDisposableGroup SetTopic( string topicOtherThanGroupText = null );

        /// <summary>
        /// Sets a function that will be called on group closing to generate a conclusion.
        /// </summary>
        /// <param name="getConclusionText">Function that generates a group conclusion.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        IDisposable ConcludeWith( Func<string> getConclusionText );

    }
}
