#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Client\ActivityMonitorPathCatcher.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// The "Path Catcher" captures the current path of the opened groups and the last, current, line and exposes it thanks to 
    /// a read only list of <see cref="PathElement"/> (the <see cref="DynamicPath"/> property),
    /// plus two other specific paths, the <see cref="LastErrorPath"/> and the <see cref="LastWarnOrErrorPath"/>.
    /// </summary>
    public sealed class ActivityMonitorPathCatcher : ActivityMonitorClient, IActivityMonitorBoundClient
    {
        /// <summary>
        /// Element of the <see cref="ActivityMonitorPathCatcher.DynamicPath">DynamicPath</see>, <see cref="ActivityMonitorPathCatcher.LastErrorPath">LastErrorPath</see>,
        /// or <see cref="ActivityMonitorPathCatcher.LastWarnOrErrorPath">LastWarnOrErrorPath</see>.
        /// </summary>
        public class PathElement
        {
            /// <summary>
            /// Gets the tags of the log entry.
            /// </summary>
            public CKTrait Tags { get; internal set; }
            /// <summary>
            /// Gets the log level of the log entry.
            /// </summary>
            public LogLevel MaskedLevel { get; internal set; }
            /// <summary>
            /// Gets the text of the log entry.
            /// </summary>
            public string Text { get; internal set; }
            /// <summary>
            /// Gets the conclusions associated to a group. Null if this element does not correspond to a group.
            /// </summary>
            public IReadOnlyList<ActivityLogGroupConclusion> GroupConclusion { get; internal set; }

            /// <summary>
            /// Overridden to return the <see cref="Text"/> of this element.
            /// </summary>
            /// <returns>This <see cref="Text"/> property.</returns>
            public override string ToString()
            {
                return Text;
            }
        }

        IReadOnlyList<PathElement> _errorSnaphot;
        IReadOnlyList<PathElement> _warnSnaphot;

        readonly List<PathElement> _path;
        PathElement _current;
        IActivityMonitor _source;
        bool _currentIsGroup;
        bool _currentIsGroupClosed;
        bool _locked;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorPathCatcher"/>.
        /// </summary>
        public ActivityMonitorPathCatcher()
        {
            _path = new List<PathElement>();
        }

        void IActivityMonitorBoundClient.SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( !forceBuggyRemove )
            {
                if( source != null && _source != null ) throw CreateMultipleRegisterOnBoundClientException( this );
            }
            _source = source;
        }

        /// <summary>
        /// Gets or sets whether this <see cref="ActivityMonitorPathCatcher"/> can be removed from its monitor.
        /// Defaults to false. When setting this to true, <see cref="IActivityMonitorOutput.UnregisterClient"/>
        /// does not remove it.
        /// </summary>
        public bool IsLocked
        {
            get { return _locked; }
            set { _locked = value; }
        }

        /// <summary>
        /// Gets the current (mutable) path. You may use ToArray or ToList methods to take a snapshot of this list.
        /// Use the extension method <see cref="ActivityMonitorExtension.ToStringPath"/> to easily format this path.
        /// </summary>
        public IReadOnlyList<PathElement> DynamicPath
        {
            get { return _path; }
        }

        /// <summary>
        /// Gets the last <see cref="DynamicPath"/> where an <see cref="LogLevel.Error"/> or a <see cref="LogLevel.Fatal"/> occurred.
        /// Null if no error nor fatal occurred.
        /// Use the extension method <see cref="ActivityMonitorExtension.ToStringPath"/> to easily format this path.
        /// </summary>
        public IReadOnlyList<PathElement> LastErrorPath
        {
            get { return _errorSnaphot; }
        }

        /// <summary>
        /// Clears current <see cref="LastErrorPath"/> (sets it to null).
        /// </summary>
        public void ClearLastErrorPath()
        {
            _errorSnaphot = null;
        }

        /// <summary>
        /// Gets the last path with a <see cref="LogLevel.Fatal"/>, <see cref="LogLevel.Error"/> or a <see cref="LogLevel.Warn"/>.
        /// Null if no error, fatal nor warn occurred.
        /// Use the extension method <see cref="ActivityMonitorExtension.ToStringPath"/> to easily format this path.
        /// </summary>
        public IReadOnlyList<PathElement> LastWarnOrErrorPath
        {
            get { return _warnSnaphot; }
        }

        /// <summary>
        /// Clears current <see cref="LastWarnOrErrorPath"/> (sets it to null), and
        /// optionally clears <see cref="LastErrorPath"/>.
        /// </summary>
        public void ClearLastWarnPath( bool clearLastErrorPath = false )
        {
            _warnSnaphot = null;
            if( clearLastErrorPath ) _errorSnaphot = null;
        }

        /// <summary>
        /// Appends or updates the last <see cref="PathElement"/> of <see cref="DynamicPath"/>
        /// and handles errors or warning.
        /// </summary>
        /// <param name="data">Log data. Never null.</param>
        protected override void OnUnfilteredLog( ActivityMonitorLogData data )
        {
            if( data.Text != ActivityMonitor.ParkLevel )
            {
                if( _currentIsGroupClosed ) HandleCurrentGroupIsClosed();
                if( _currentIsGroup || _current == null )
                {
                    _current = new PathElement();
                    _path.Add( _current );
                    _currentIsGroup = false;
                }
                _current.Tags = data.Tags;
                _current.MaskedLevel = data.Level&LogLevel.Mask;
                _current.Text = data.Text;
                CheckSnapshot();
            }
        }

        /// <summary>
        /// Appends or updates the last <see cref="PathElement"/> of <see cref="DynamicPath"/>
        /// and handles errors or warning.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected override void OnOpenGroup( IActivityLogGroup group )
        {
            if( _currentIsGroupClosed ) HandleCurrentGroupIsClosed();
            if( _currentIsGroup || _current == null )
            {
                _current = new PathElement();
                _path.Add( _current );
            }
            _currentIsGroup = true;
            _current.Tags = group.GroupTags;
            _current.MaskedLevel = group.MaskedGroupLevel;
            _current.Text = group.GroupText;
            CheckSnapshot();
        }

        /// <summary>
        /// Removes one or two last <see cref="PathElement"/> of <see cref="DynamicPath"/>.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Texts that conclude the group. Never null but can be empty.</param>
        protected override void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _currentIsGroupClosed ) HandleCurrentGroupIsClosed();
            if( _path.Count > 0 )
            {
                if( !_currentIsGroup ) _path.RemoveAt( _path.Count - 1 );
                
                _currentIsGroupClosed = false;
                _current = null;
                if( _path.Count > 0 )
                {
                    _current = _path[_path.Count - 1];
                    _current.GroupConclusion = conclusions;
                    _currentIsGroup = true;
                    _currentIsGroupClosed = _path.Count > 0;
                }
                else _currentIsGroup = false;
            }
        }

        void HandleCurrentGroupIsClosed()
        {
            Debug.Assert( _currentIsGroupClosed && _path.Count > 0 );
            _current = null;
            _path.RemoveAt( _path.Count - 1 );
            if( _path.Count > 0 ) _current = _path[_path.Count - 1];
            _currentIsGroup = _current != null;
            _currentIsGroupClosed = false;
        }

        void CheckSnapshot()
        {
            Debug.Assert( _current != null );
            if( _current.MaskedLevel >= LogLevel.Warn )
            {
                // Clone the last element if it is not a group: since it is updated
                // with levels, it has to be snapshotted.
                _warnSnaphot = _path.Select( ( e, idx ) => _currentIsGroup || idx < _path.Count-1 ? e : new PathElement() { Tags = e.Tags, MaskedLevel = e.MaskedLevel, Text = e.Text } ).ToArray();
                if( _current.MaskedLevel >= LogLevel.Error )
                {
                    _errorSnaphot = _warnSnaphot;
                }
            }
        }

    }
}
