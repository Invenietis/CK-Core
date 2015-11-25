#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\ILogEntry.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CK.Monitoring
{

    /// <summary>
    /// Unified interface for log entries whatever their <see cref="LogType"/> is.
    /// All log entries can be exposed through this "rich" interface.
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>
        /// Gets the type of the log entry.
        /// </summary>
        LogEntryType LogType { get; }

        /// <summary>
        /// Get the log level (between LogLevel.Trace and LogLevel.Fatal).
        /// This is available whatever <see cref="LogType"/> is.
        /// </summary>
        LogLevel LogLevel { get; }

        /// <summary>
        /// Gets the log text.
        /// Null when when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets the tags for this entry.
        /// Always equals to <see cref="ActivityMonitor.Tags.Empty"/> when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        CKTrait Tags { get; }

        /// <summary>
        /// Gets the log time.
        /// This is available whatever <see cref="LogType"/> is.
        /// </summary>
        DateTimeStamp LogTime { get; }

        /// <summary>
        /// Gets the exception data if any (can be not null only when <see cref="LogType"/> is <see cref="LogEntryType.OpenGroup"/>: exceptions are exclusively carried by groups).
        /// </summary>
        CKExceptionData Exception { get; }

        /// <summary>
        /// Gets the file name of the source code that emitted the log.
        /// Null when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the line number in the source code <see cref="FileName"/> that emitted the log.
        /// 0 when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets any group conclusion. 
        /// Always null except of course when <see cref="LogType"/> is <see cref="LogEntryType.CloseGroup"/>.
        /// </summary>
        IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get; }

        /// <summary>
        /// Writes the entry in a <see cref="BinaryWriter"/>.
        /// Use <see cref="LogEntry.Read"/> to read it back.
        /// </summary>
        /// <param name="w">The binary writer.</param>
        void WriteLogEntry( BinaryWriter w );
    }
}
