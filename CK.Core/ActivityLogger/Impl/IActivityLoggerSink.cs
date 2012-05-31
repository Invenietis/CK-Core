#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\IActivityLoggerSink.cs) is part of CiviKey. 
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
    /// Defines sink for <see cref="ActivityLoggerTap"/>.
    /// Inherits from this interface to implement your own logger (ie: XmlLogger).
    /// Each method described below provides an easy way to react to <see cref="IActivityLogger"/> calls.
    /// </summary>
    public interface IActivityLoggerSink
    {
        /// <summary>
        /// Called for the first text of a <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="level">The new current log level.</param>
        /// <param name="text">Text to start.</param>
        void OnEnterLevel( LogLevel level, string text );

        /// <summary>
        /// Called for text with the same <see cref="LogLevel"/> as the previous ones.
        /// </summary>
        /// <param name="level">The current log level.</param>
        /// <param name="text">Text to append.</param>
        void OnContinueOnSameLevel( LogLevel level, string text );

        /// <summary>
        /// Called when current log level changes.
        /// </summary>
        /// <param name="level">The previous log level.</param>
        void OnLeaveLevel( LogLevel level );

        /// <summary>
        /// Called whenever a group is opened.
        /// </summary>
        /// <param name="group">The newly opened group.</param>
        void OnGroupOpen( IActivityLogGroup group );

        /// <summary>
        /// Called once the conclusion is known at the group level (if it exists, the <see cref="ActivityLogGroupConclusion.Emitter"/> is the <see cref="IActivityLogger"/> itself) 
        /// but before the group is actually closed: clients can update the conclusions for the group.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">Texts that concludes the group. Never null but can be empty.</param>
        void OnGroupClose( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions );
    }
}
