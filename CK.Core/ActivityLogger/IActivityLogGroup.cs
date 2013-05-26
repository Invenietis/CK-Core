#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\IActivityLogGroup.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Exposes all the relevant information for a currently opened group.
    /// Groups are linked together from the current one to the very first one 
    /// thanks to the <see cref="Parent"/> property.
    /// </summary>
    public interface IActivityLogGroup
    {
        /// <summary>
        /// Gets the tags for the log group.
        /// </summary>
        CKTrait GroupTags { get; }

        /// <summary>
        /// Gets the origin <see cref="IActivityLogger"/> for the log group.
        /// </summary>
        IActivityLogger OriginLogger { get; }

        /// <summary>
        /// Gets the log time for the log.
        /// </summary>
        DateTime LogTimeUtc { get; }

        /// <summary>
        /// Gets the log time of the group closing.
        /// It is <see cref="DateTime.MinValue"/> when the group is not closed yet.
        /// </summary>
        DateTime CloseLogTimeUtc { get; }

        /// <summary>
        /// Get the previous group in its <see cref="OriginLogger"/>. Null if this is a top level group.
        /// </summary>
        IActivityLogGroup Parent { get; }

        /// <summary>
        /// Gets the depth of this group in its <see cref="OriginLogger"/> (1 for top level groups).
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets the <see cref="IActivityLogger.Filter"/> that will be restored when group will be closed.
        /// Initialized with the current value of IActivityLogger.Filter when the group has been opened.
        /// </summary>
        LogLevelFilter SavedLoggerFilter { get; }

        /// <summary>
        /// Gets the <see cref="IActivityLogger.AutoTags"/> that will be restored when group will be closed.
        /// Initialized with the current value of IActivityLogger.Tags when the group has been opened.
        /// </summary>
        CKTrait SavedLoggerTags { get; }

        /// <summary>
        /// Gets the level associated to this group.
        /// </summary>
        LogLevel GroupLevel { get; }

        /// <summary>
        /// Getst the text associated to this group.
        /// </summary>
        string GroupText { get; }

        /// <summary>
        /// Gets the associated <see cref="Exception"/> if it exists.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Gets whether the <see cref="GroupText"/> is actually the <see cref="Exception"/> message.
        /// </summary>
        bool IsGroupTextTheExceptionMessage { get; }
    }
}
