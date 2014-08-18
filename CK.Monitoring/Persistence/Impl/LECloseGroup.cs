#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\Impl\LECloseGroup.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
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

namespace CK.Monitoring.Impl
{
    class LECloseGroup: ILogEntry
    {
        readonly LogLevel _level;
        readonly IReadOnlyList<ActivityLogGroupConclusion> _conclusions;
        readonly DateTimeStamp _time;

        public LECloseGroup( DateTimeStamp t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c ) 
        {
            _time = t;
            _conclusions = c;
            _level = level;
        }

        public LECloseGroup( LEMCCloseGroup e ) 
        {
            _time = e.LogTime;
            _conclusions = e.Conclusions;
            _level = e.LogLevel;
        }

        public LogEntryType LogType { get { return LogEntryType.CloseGroup; } }

        public string Text { get { return null; } }

        public LogLevel LogLevel { get { return _level; } }

        public DateTimeStamp LogTime { get { return _time; } }

        public CKExceptionData Exception { get { return null; } }

        public string FileName { get { return null; } }
        
        public int LineNumber { get { return 0; } }

        public CKTrait Tags { get { return ActivityMonitor.Tags.Empty; } }

        public IReadOnlyList<ActivityLogGroupConclusion> Conclusions { get { return _conclusions; } }

        public virtual void WriteLogEntry( BinaryWriter w )
        {
            LogEntry.WriteCloseGroup( w, _level, _time, _conclusions );
        }
        
    }
}
