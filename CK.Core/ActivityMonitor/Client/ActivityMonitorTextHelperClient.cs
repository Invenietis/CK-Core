#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorTap.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// Base class for <see cref="IActivityMonitorClient"/> that tracks groups and level changes in order
    /// to ease text-based renderer.
    /// </summary>
    public abstract class ActivityMonitorTextHelperClient : IActivityMonitorClient
    {
        int _curLevel;

        /// <summary>
        /// Initialize a new <see cref="ActivityMonitorTextHelperClient"/>.
        /// </summary>
        protected ActivityMonitorTextHelperClient()
        {
            _curLevel = -1;
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            level &= LogLevel.Mask;
            if( text == ActivityMonitor.ParkLevel )
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
                    OnContinueOnSameLevel( tags, level, text, logTimeUtc );
                }
                else
                {
                    if( _curLevel != -1 )
                    {
                        OnLeaveLevel( (LogLevel)_curLevel );
                    }
                    OnEnterLevel( tags, level, text, logTimeUtc );
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
            OnGroupClose( group, conclusions );
        }

        /// <summary>
        /// Called for the first text of a <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">The new current log level (without <see cref="LogLevel.IsFiltered"/>).</param>
        /// <param name="text">Text to start.</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        protected abstract void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc );

        /// <summary>
        /// Called for text with the same <see cref="LogLevel"/> as the previous ones.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">The current log level (without <see cref="LogLevel.IsFiltered"/>).</param>
        /// <param name="text">Text to append.</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        protected abstract void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc );

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

    }
}
