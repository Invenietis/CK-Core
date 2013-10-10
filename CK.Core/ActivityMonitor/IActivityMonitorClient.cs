#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\IActivityMonitorClient.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Listener for <see cref="IActivityMonitor"/> registered in a <see cref="IActivityMonitorOutput"/>.
    /// </summary>
    public interface IActivityMonitorClient
    {
        /// <summary>
        /// Called for each <see cref="IActivityMonitor.UnfilteredLog"/>.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc );

        /// <summary>
        /// Called for each <see cref="IActivityMonitor.UnfilteredOpenGroup"/>.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        void OnOpenGroup( IActivityLogGroup group );

        /// <summary>
        /// Called once the user conclusions and the <see cref="ActivityMonitor.Group.GetConclusionText"/> are known at the group level but before 
        /// the group is actually closed: clients can update the conclusions for the group.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">
        /// Mutable conclusions associated to the closing group. 
        /// This can be null if no conclusions have been added yet. 
        /// It is up to the first client that wants to add a conclusion to instanciate a new List object to carry the conclusions.
        /// </param>
        void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions );

        /// <summary>
        /// Called when the group is actually closed.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Texts that conclude the group. Never null but can be empty.</param>
        void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions );

    }

}
