#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CK.Mon2Htm\Impl\LogPage.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;
using CK.Core;

namespace CK.Mon2Htm
{
    class LogPage : IStructuredLogPage
    {
        readonly IReadOnlyList<IPagedLogEntry> _entries;
        readonly IReadOnlyList<ILogEntry> _openGroupsAtStart;
        readonly IReadOnlyList<ILogEntry> _openGroupsAtEnd;
        readonly int _pageNumber;

        internal LogPage( IReadOnlyList<ILogEntry> entries, IReadOnlyList<ILogEntry> openGroupsAtStart, IReadOnlyList<ILogEntry> openGroupsAtEnd, int pageNumber, MonitorIndexInfo indexInfo )
        {
            _entries = BuildStructuredLogEntries( entries, openGroupsAtStart, openGroupsAtEnd, indexInfo, pageNumber );

            _openGroupsAtStart = openGroupsAtStart;
            _openGroupsAtEnd = openGroupsAtEnd;
            _pageNumber = pageNumber;
        }

        private static IReadOnlyList<IPagedLogEntry> BuildStructuredLogEntries( IEnumerable<ILogEntry> entries, IReadOnlyList<ILogEntry> openGroupsAtStart, IReadOnlyList<ILogEntry> openGroupsAtEnd, MonitorIndexInfo indexInfo, int pageNumber )
        {
            List<PagedLogEntry> logEntries = new List<PagedLogEntry>();

            List<PagedLogEntry> currentPath = new List<PagedLogEntry>();

            // Process already-opened group

            foreach( var entry in openGroupsAtStart )
            {
                bool exists;

                var pagedEntry = new PagedLogEntry( entry );

                int groupStartPage = indexInfo.GetPageIndexOf( entry.LogTime ) + 1;
                int groupEndPage = indexInfo.GetPageIndexOf( indexInfo.Groups.GetByKey( entry.LogTime, out exists ).CloseGroupTimestamp ) + 1;

                pagedEntry.GroupStartsOnPage = groupStartPage;
                if( pageNumber != groupEndPage ) pagedEntry.GroupEndsOnPage = groupEndPage;

                if( currentPath.Count > 0 )
                {
                    // Add current path as child
                    currentPath[currentPath.Count - 1].AddChild( pagedEntry );
                }
                else
                {
                    // Add to root
                    logEntries.Add( pagedEntry );
                }

                currentPath.Add( pagedEntry );
            }

            foreach( var entry in entries )
            {
                var pagedEntry = new PagedLogEntry( entry );

                if( currentPath.Count > 0 )
                {
                    // Add current path as child
                    currentPath[currentPath.Count - 1].AddChild( pagedEntry );
                }
                else
                {
                    // Add to root
                    logEntries.Add( pagedEntry );
                }

                if( pagedEntry.LogType == LogEntryType.OpenGroup )
                {
                    bool exists;

                    int groupEndPage = indexInfo.GetPageIndexOf( indexInfo.Groups.GetByKey( entry.LogTime, out exists ).CloseGroupTimestamp ) + 1;

                    pagedEntry.GroupStartsOnPage = 0;
                    if( pageNumber != groupEndPage ) pagedEntry.GroupEndsOnPage = groupEndPage;

                    currentPath.Add( pagedEntry );
                }
                else if( pagedEntry.LogType == LogEntryType.CloseGroup )
                {
                    var openGroupRef = indexInfo.Groups.FirstOrDefault( x => x.CloseGroupTimestamp == pagedEntry.LogTime );
                    if( openGroupRef != null )
                    {
                        // Not Missing open group
                        var openGroupTimestamp = openGroupRef.OpenGroupTimestamp;
                        var groupStartPage = indexInfo.GetPageIndexOf( openGroupTimestamp );

                        if( pageNumber != groupStartPage ) pagedEntry.GroupStartsOnPage = groupStartPage;

                        currentPath.RemoveAt( currentPath.Count - 1 );
                    }
                    else
                    {
                        pagedEntry.GroupStartsOnPage = 0;
                    }
                    pagedEntry.GroupEndsOnPage = 0;
                }

            }

            return logEntries.AsReadOnlyList();
        }

        public IReadOnlyList<ILogEntry> OpenGroupsAtStart
        {
            get { return _openGroupsAtStart; }
        }

        public IReadOnlyList<ILogEntry> OpenGroupsAtEnd
        {
            get { return _openGroupsAtEnd; }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
        }

        public IReadOnlyList<IPagedLogEntry> Entries
        {
            get { return _entries; }
        }
    }
}
