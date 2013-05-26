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
        /// <param name="tags">Tags (from <see cref="ActivityLogger.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">The new current log level.</param>
        /// <param name="text">Text to start.</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc );

        /// <summary>
        /// Called for text with the same <see cref="LogLevel"/> as the previous ones.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityLogger.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">The current log level.</param>
        /// <param name="text">Text to append.</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc );

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
        /// Called when the group is actually closed.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">Texts that concludes the group. Never null but can be empty.</param>
        void OnGroupClose( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions );
    }
}
