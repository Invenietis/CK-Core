#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CK.Mon2Htm\Impl\PagedLogEntry.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;
using CK.Core;

namespace CK.Mon2Htm
{
    class PagedLogEntry : IPagedLogEntry
    {
        ILogEntry _entry;
        List<IPagedLogEntry> _children;

        public PagedLogEntry(ILogEntry rootEntry)
        {
            _entry = rootEntry;
            if( rootEntry.LogType == LogEntryType.OpenGroup ) _children = new List<IPagedLogEntry>();
        }

        internal void AddChild( IPagedLogEntry entry )
        {
            _children.Add( entry );
        }

        public IReadOnlyList<IPagedLogEntry> Children { get { return _children.AsReadOnlyList(); } }

        public int GroupStartsOnPage { get; internal set; }

        public int GroupEndsOnPage { get; internal set; }

        #region ILogEntry Members

        public LogEntryType LogType
        {
            get { return _entry.LogType; }
        }

        public Core.LogLevel LogLevel
        {
            get { return _entry.LogLevel; }
        }

        public string Text
        {
            get { return _entry.Text; }
        }

        public Core.CKTrait Tags
        {
            get { return _entry.Tags; }
        }

        public Core.DateTimeStamp LogTime
        {
            get { return _entry.LogTime; }
        }

        public Core.CKExceptionData Exception
        {
            get { return _entry.Exception; }
        }

        public string FileName
        {
            get { return _entry.FileName; }
        }

        public int LineNumber
        {
            get { return _entry.LineNumber; }
        }

        public IReadOnlyList<Core.ActivityLogGroupConclusion> Conclusions
        {
            get { return _entry.Conclusions; }
        }

        public void WriteLogEntry( System.IO.BinaryWriter w )
        {
            _entry.WriteLogEntry( w );
        }

        #endregion
    }
}
