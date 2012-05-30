#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\IMuxActivityLoggerClient.cs) is part of CiviKey. 
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
    /// Listener for multiple <see cref="IActivityLogger"/>. See <see cref="IMuxActivityLoggerClientRegistrar"/>.
    /// </summary>
    public interface IMuxActivityLoggerClient : IActivityLoggerClientBase
    {
        /// <summary>
        /// Called when <see cref="IActivityLogger.Filter"/> is about to change.
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="current">Current level filter.</param>
        /// <param name="newValue">The new level filter.</param>
        void OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue );

        /// <summary>
        /// Called for each <see cref="IActivityLogger.UnfilteredLog"/>.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="sender">The sender logger.</param>
        /// <param name="text">Text (not null).</param>
        void OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text );

        /// <summary>
        /// Called for each <see cref="IActivityLogger.OpenGroup"/>.
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        void OnOpenGroup( IActivityLogger sender, IActivityLogGroup group );

        /// <summary>
        /// Called once the conclusion is known at the group level (if it exists, the <see cref="ActivityLogGroupConclusion.Emitter"/> is the <see cref="IActivityLogger"/> itself) 
        /// but before the group is actually closed: clients can update the conclusions for the group.
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">Mutable conclusions associated to the closing group.</param>
        void OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions );

        /// <summary>
        /// Called when the group is actually closed.
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Texts that conclude the group. Never null but can be empty.</param>
        void OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions );
    }

}
