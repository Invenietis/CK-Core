#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Client\ActivityMonitorTextHelperClient.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// Base class for <see cref="IActivityMonitorClient"/> that tracks groups and level changes in order
    /// to ease text-based renderer.
    /// </summary>
    public abstract class ActivityMonitorTextHelperClient : IActivityMonitorBoundClient
    {
        int _curLevel;
        LogFilter _filter;
        Stack<bool> _openGroups;
        IActivityMonitorImpl _source;

        /// <summary>
        /// Initialize a new <see cref="ActivityMonitorTextHelperClient"/> with a filter.
        /// </summary>
        protected ActivityMonitorTextHelperClient( LogFilter filter )
        {
            _curLevel = -1;
            _openGroups = new Stack<bool>();
            _filter = filter;
        }

        /// <summary>
        /// Initialize a new <see cref="ActivityMonitorTextHelperClient"/>.
        /// </summary>
        protected ActivityMonitorTextHelperClient()
            : this( LogFilter.Undefined )
        {
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            var level = data.MaskedLevel;

            if( !CanOutputLine( level ) )
            {
                return;
            }

            if( data.Text == ActivityMonitor.ParkLevel )
            {
                if( _curLevel != -1 )
                {
                    OnLeaveLevel( (LogLevel)_curLevel );
                }
                _curLevel = -1;
            }
            else
            {
                if( _curLevel == (int)level )
                {
                    OnContinueOnSameLevel( data );
                }
                else
                {
                    if( _curLevel != -1 )
                    {
                        OnLeaveLevel( (LogLevel)_curLevel );
                    }
                    OnEnterLevel( data );
                    _curLevel = (int)level;
                }
            }
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            if( _curLevel != -1 )
            {
                OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }
            if( !CanOutputGroup( group.MaskedGroupLevel ) )
            {
                _openGroups.Push( false );
                return;
            }
            _openGroups.Push( true );
            OnGroupOpen( group );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _curLevel != -1 )
            {
                OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }
            // Has the Group actually been opened?
            if( _openGroups.Count > 0 && _openGroups.Pop() ) OnGroupClose( group, conclusions );
        }

        /// <summary>
        /// Called for the first text of a <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected abstract void OnEnterLevel( ActivityMonitorLogData data );

        /// <summary>
        /// Called for text with the same <see cref="LogLevel"/> as the previous ones.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected abstract void OnContinueOnSameLevel( ActivityMonitorLogData data );

        /// <summary>
        /// Called when current log level changes.
        /// </summary>
        /// <param name="level">The previous log level (without <see cref="LogLevel.IsFiltered"/>).</param>
        protected abstract void OnLeaveLevel( LogLevel level );

        /// <summary>
        /// Called whenever a group is opened.
        /// </summary>
        /// <param name="group">The newly opened group.</param>
        protected abstract void OnGroupOpen( IActivityLogGroup group );

        /// <summary>
        /// Called when the group is actually closed.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">Texts that concludes the group. Never null but can be empty.</param>
        protected abstract void OnGroupClose( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions );


        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }

        /// <summary>
        /// Gets or sets the filter for this client.
        /// </summary>
        public LogFilter Filter 
        { 
            get { return _filter; }
            set
            {
                LogFilter oldFilter = _filter;
                _filter = value;
                if( _source != null ) _source.OnClientMinimalFilterChanged( oldFilter, _filter );
            }
        }

        bool CanOutputLine( LogLevel logLevel )
        {
            Debug.Assert( (logLevel & LogLevel.IsFiltered) == 0, "The level must already be masked." );
            return _filter.Line == LogLevelFilter.None || (int)logLevel >= (int)_filter.Line;
        }

        bool CanOutputGroup( LogLevel logLevel )
        {
            Debug.Assert( (logLevel & LogLevel.IsFiltered) == 0, "The level must already be masked." );
            return _filter.Group == LogLevelFilter.None || (int)logLevel >= (int)_filter.Group;
        }

        #region IActivityMonitorBoundClient Members

        LogFilter IActivityMonitorBoundClient.MinimalFilter
        {
            get { return _filter; }
        }

        void IActivityMonitorBoundClient.SetMonitor( Impl.IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( !forceBuggyRemove )
            {
                if( source != null && _source != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            }
            _openGroups.Clear();
            _source = source;
        }

        #endregion
    }
}
