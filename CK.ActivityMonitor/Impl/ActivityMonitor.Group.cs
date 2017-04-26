#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitor.Group.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// Concrete implementation of <see cref="IActivityMonitor"/>.
    /// </summary>
    public partial class ActivityMonitor
    {
        /// <summary>
        /// Groups are bound to an <see cref="ActivityMonitor"/> and are linked together from 
        /// the current one to the very first one (a kind of stack).
        /// </summary>
        protected sealed class Group : IActivityLogGroup, IDisposableGroup
        {
            /// <summary>
            /// The monitor that owns this group.
            /// </summary>
            public readonly ActivityMonitor Monitor;
            
            /// <summary>
            /// The raw index of the group. 
            /// </summary>
            public readonly int Index;

            ActivityMonitorGroupData _data;
            DateTimeStamp _closeLogTime;
            string _previousTopic;
            Group _unfilteredParent;
            int _depth;

            /// <summary>
            /// Initialized a new Group at a given index.
            /// </summary>
            /// <param name="monitor">Monitor.</param>
            /// <param name="index">Index of the group.</param>
            internal Group( ActivityMonitor monitor, int index )
            {
                Monitor = monitor;
                Index = index;
            }

            /// <summary>
            /// Initializes or reinitializes this group (if it has been disposed). 
            /// </summary>
            internal void Initialize( ActivityMonitorGroupData data )
            {
                SavedMonitorFilter = Monitor._configuredFilter;
                SavedMonitorTags = Monitor._currentTag;
                if( (_unfilteredParent = Monitor._currentUnfiltered) != null ) _depth = _unfilteredParent._depth + 1;
                else _depth = 1;
                // Logs everything when a Group is a fatal or an error: we then have full details available without
                // requiring to log all with Error or Fatal level.
                if( data.MaskedLevel >= LogLevel.Error && Monitor._configuredFilter != LogFilter.Debug ) Monitor.DoSetConfiguredFilter( LogFilter.Debug );
                _closeLogTime = DateTimeStamp.MinValue;
                _data = data;
            }

            /// <summary>
            /// Initializes or reinitializes this group (if it has been disposed) as a filtered group. 
            /// </summary>
            internal void InitializeRejectedGroup( ActivityMonitorGroupData data )
            {
                SavedMonitorFilter = Monitor._configuredFilter;
                SavedMonitorTags = Monitor._currentTag;
                _unfilteredParent = Monitor._currentUnfiltered;
                _depth = 0;
                _data = data;
            }

            /// <summary>
            /// Gets whether the group is rejected: Depth is 0 when ActualFilter is Off or the GroupLevel 
            /// is None (the OpenGroup has been filtered).
            /// </summary>
            internal bool IsRejectedGroup
            {
                get { return _depth == 0 || _data.Level == LogLevel.None; }
            }

            /// <summary>
            /// Gets the tags for the log group.
            /// </summary>
            public CKTrait GroupTags => _data.Tags;

            /// <summary>
            /// Gets the log time for the log.
            /// </summary>
            public DateTimeStamp LogTime => _data.LogTime; 

            /// <summary>
            /// Gets the log time of the group closing.
            /// It is <see cref="DateTimeStamp.MinValue"/> when the group is not closed yet.
            /// </summary>
            public DateTimeStamp CloseLogTime 
            { 
                get { return _closeLogTime; } 
                internal set { _closeLogTime = value; } 
            }

            /// <summary>
            /// Gets the <see cref="CKExceptionData"/> that captures exception information 
            /// if it exists. Returns null if no <see cref="P:Exception"/> exists.
            /// </summary>
            public CKExceptionData ExceptionData => _data.ExceptionData; 

            /// <summary>
            /// Gets or creates the <see cref="CKExceptionData"/> that captures exception information.
            /// If <see cref="P:Exception"/> is null, this returns null.
            /// </summary>
            /// <returns></returns>
            public CKExceptionData EnsureExceptionData() => _data.EnsureExceptionData();

            /// <summary>
            /// Get the previous group in its origin monitor. Null if this group is a top level group.
            /// </summary>
            public IActivityLogGroup Parent => _unfilteredParent;
            
            /// <summary>
            /// Gets the depth of this group in its origin monitor (1 for top level groups).
            /// </summary>
            public int Depth => _depth; 

            /// <summary>
            /// Gets the level associated to this group.
            /// The <see cref="LogLevel.IsFiltered"/> can be set here: use <see cref="MaskedGroupLevel"/> to get 
            /// the actual level from <see cref="LogLevel.Trace"/> to <see cref="LogLevel.Fatal"/>.
            /// </summary>
            public LogLevel GroupLevel => _data.Level;

            /// <summary>
            /// Gets the actual level (from <see cref="LogLevel.Trace"/> to <see cref="LogLevel.Fatal"/>) associated to this group
            /// without <see cref="LogLevel.IsFiltered"/> bit.
            /// </summary>
            public LogLevel MaskedGroupLevel => _data.MaskedLevel;

            /// <summary>
            /// Gets the text with which this group has been opened. Null if and only if the group is closed.
            /// </summary>
            public string GroupText => _data.Text;

            /// <summary>
            /// Gets the associated <see cref="Exception"/> if it exists.
            /// </summary>
            public Exception Exception => _data.Exception;

            /// <summary>
            /// Gets or sets the <see cref="IActivityMonitor.MinimalFilter"/> that will be restored when group will be closed.
            /// Initialized with the current value of IActivityMonitor.Filter when the group has been opened.
            /// </summary>
            public LogFilter SavedMonitorFilter { get; private set; }

            /// <summary>
            /// Gets or sets the <see cref="IActivityMonitor.AutoTags"/> that will be restored when group will be closed.
            /// Initialized with the current value of IActivityMonitor.Tags when the group has been opened.
            /// </summary>
            public CKTrait SavedMonitorTags { get; private set; }

            /// <summary>
            /// Gets whether the <see cref="GroupText"/> is actually the <see cref="Exception"/> message.
            /// </summary>
            public bool IsGroupTextTheExceptionMessage => _data.IsTextTheExceptionMessage; 

            /// <summary>
            /// Gets the previous topic it it must be restored. Null otherwise.
            /// </summary>
            public string PreviousTopic => _previousTopic; 

            /// <summary>
            /// Gets the file name of the source code that issued the log.
            /// </summary>
            public string FileName => _data.FileName;

            /// <summary>
            /// Gets the line number of the <see cref="FileName"/> that issued the log.
            /// </summary>
            public int LineNumber => _data.LineNumber;

            IDisposable IDisposableGroup.ConcludeWith( Func<string> getConclusionText )
            {
                _data.GetConclusionText = getConclusionText;
                return this;
            }

            IDisposableGroup IDisposableGroup.SetTopic( string topicOtherThanGroupText )
            {
                _previousTopic = Monitor.Topic;
                Monitor.SetTopic( topicOtherThanGroupText ?? _data.Text, _data.FileName, _data.LineNumber );
                return this;
            }

            /// <summary>
            /// Ensures that any groups opened after this one are closed before closing this one.
            /// </summary>
            void IDisposable.Dispose()
            {
                if( _data != null )
                {
                    while( Monitor._current != this ) ((IDisposable)Monitor._current).Dispose();
                    Monitor.CloseGroup( Monitor.NextLogTime(), null );
                }
            }           

            internal void GroupClosing( ref List<ActivityLogGroupConclusion> conclusions )
            {
                string auto = _data.ConsumeConclusionText();
                if( auto != null )
                {
                    if( conclusions == null ) conclusions = new List<ActivityLogGroupConclusion>();
                    conclusions.Add( new ActivityLogGroupConclusion( Tags.GetTextConclusion, auto ) );
                }
            }

            internal void GroupClosed()
            {
                _data = null;
                _previousTopic = null;
            }
        }

        IActivityLogGroup IActivityMonitorImpl.CurrentGroup => _current; 

        /// <summary>
        /// Gets the currently opened group.
        /// Null when no group is currently opened.
        /// </summary>
        protected IActivityLogGroup CurrentGroup => _current; 

    }
}
