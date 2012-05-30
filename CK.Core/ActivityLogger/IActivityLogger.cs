#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\IActivityLogger.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Simple activity logger for end user communication. This is not the same as a classical logging framework: this 
    /// is dedicated to capture activities in order to display them to an end user.
    /// </summary>
    public interface IActivityLogger
    {
        /// <summary>
        /// Gets or sets a filter based on the log level.
        /// This filter applies to the currently opened group (it is automatically restored when <see cref="CloseGroup"/> is called).
        /// </summary>
        LogLevelFilter Filter { get; set; }

        /// <summary>
        /// Logs a text regardless of <see cref="Filter"/> level. 
        /// Each call to log is considered as a line: a paragraph (or line separator) is appended
        /// between each text if the <paramref name="level"/> is the same as the previous one.
        /// See remarks.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log. Ignored if null or empty.</param>
        /// <param name="ex">Optional exception associated to the log. When not null, a Group is automatically created.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        /// <remarks>
        /// A null or empty <paramref name="text"/> is not logged.
        /// The special text "PARK-LEVEL" breaks the current <see cref="LogLevel"/>
        /// and resets it: the next log, even with the same LogLevel, will be treated as if
        /// a different LogLevel is used.
        /// </remarks>
        IActivityLogger UnfilteredLog( LogLevel level, string text, Exception ex = null );

        /// <summary>
        /// Opens a log level. <see cref="CloseGroup"/> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="Filter"/> is ignored.</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing.</param>
        /// <param name="text">Text to log (the title of the group). Null text is valid and considered as <see cref="String.Empty"/>.</param>
        /// <param name="ex">Optional exception associated to the group.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not filtered since any subordinated logs may occur with a much higher level.
        /// It is left to the implementation to handle (or not) filtering when <see cref="CloseGroup"/> is called.
        /// </remarks>
        IDisposable OpenGroup( LogLevel level, Func<string> getConclusionText, string text, Exception ex = null );

        /// <summary>
        /// Closes the current group level, appending an optional conclusion to the opening logged information.
        /// </summary>
        /// <param name="conclusion">
        /// Optional object text (usually a string but can be any object with an 
        /// overriden <see cref="Object.ToString"/> method) to conclude the group.
        /// </param>
        void CloseGroup( object conclusion = null );

        /// <summary>
        /// Gets the <see cref="IActivityLoggerOutput"/> for this logger.
        /// </summary>
        IActivityLoggerOutput Output { get; }
    }

}
