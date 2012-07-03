#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\IDefaultActivityLogger.cs) is part of CiviKey. 
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
    /// Specialized <see cref="IActivityLogger"/> that contains a non removable <see cref="ActivityLoggerTap"/>.
    /// Concrete implementation must be obtained through <see cref="DefaultActivityLogger.Create"/> factrory method.
    /// </summary>
    public interface IDefaultActivityLogger : IActivityLogger
    {
        /// <summary>
        /// Gets the <see cref="ActivityLoggerErrorCounter"/> that manages fatal errors, errors and warnings
        /// and automatic conclusion of groups with such information.
        /// </summary>
        ActivityLoggerErrorCounter ErrorCounter { get; }

        /// <summary>
        /// Gets the <see cref="ActivityLoggerPathCatcher"/> that exposes and maintains <see cref="ActivityLoggerPathCatcher.DynamicPath">DynamicPath</see>,
        /// <see cref="ActivityLoggerPathCatcher.LastErrorPath">LastErrorPath</see> and <see cref="ActivityLoggerPathCatcher.LastWarnOrErrorPath">LastWarnOrErrorPath</see>.
        /// </summary>
        ActivityLoggerPathCatcher PathCatcher { get; } 

        /// <summary>
        /// Gets the <see cref="ActivityLoggerTap"/> that manages <see cref="IActivityLoggerSink"/>
        /// for this <see cref="DefaultActivityLogger"/>.
        /// </summary>
        ActivityLoggerTap Tap { get; }

        /// <summary>
        /// Adds an <see cref="IActivityLoggerSink"/> to the <see cref="RegisteredSinks"/>.
        /// Duplicate <see cref="IActivityLoggerSink"/> are silently ignored.
        /// </summary>
        /// <param name="sink">An activity logger sink implementation.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        IDefaultActivityLogger Register( IActivityLoggerSink sink );

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerSink"/> from the collection of loggers.
        /// Silently ignored unregistered logger.
        /// </summary>
        /// <param name="sink">An activity logger sink implementation.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        IDefaultActivityLogger Unregister( IActivityLoggerSink sink );

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerSink"/>.
        /// </summary>
        IReadOnlyList<IActivityLoggerSink> RegisteredSinks { get; }
    }
}
