#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\Impl\LELog.cs) is part of CiviKey. 
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
using System.IO;
using CK.Core;

namespace CK.Monitoring.Impl
{
    class LELog : ILogEntry
    {
        readonly string _text;
        readonly CKTrait _tags;
        readonly string _fileName;
        readonly int _lineNumber;
        readonly LogLevel _level;
        readonly CKExceptionData _ex;
        readonly DateTimeStamp _time;

        public LELog( string text, DateTimeStamp t, string fileName, int lineNumber, LogLevel l, CKTrait tags, CKExceptionData ex )
        {
            _text = text;
            _time = t;
            _fileName = fileName;
            _lineNumber = lineNumber;
            _level = l;
            _tags = tags;
            _ex = ex;
        }

        public LELog( LEMCLog e )
        {
            _text = e.Text;
            _time = e.LogTime;
            _fileName = e.FileName;
            _lineNumber = e.LineNumber;
            _level = e.LogLevel;
            _tags = e.Tags;
            _ex = e.Exception;
        }

        public LogEntryType LogType { get { return LogEntryType.Line; } }

        public LogLevel LogLevel { get { return _level; } }

        public string Text { get { return _text; } }

        public CKTrait Tags { get { return _tags; } }

        public DateTimeStamp LogTime { get { return _time; } }

        public string FileName { get { return _fileName; } }

        public int LineNumber { get { return _lineNumber; } }

        public CKExceptionData Exception { get { return _ex; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return null; } }

        public virtual void WriteLogEntry( BinaryWriter w )
        {
            LogEntry.WriteLog( w, false, _level, _time, _text, _tags, _ex, _fileName, _lineNumber );
        }
    }
}
