#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerTap.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// A tap is both a <see cref="IMuxActivityLoggerClient"/> and a <see cref="IActivityLoggerClient"/> that delivers log data 
    /// to multiple <see cref="IActivityLoggerSink"/> implementations.
    /// </summary>
    public class ActivityLoggerTap : ActivityLoggerHybridClient
    {
        int _curLevel;
        List<IActivityLoggerSink> _sinks;
        IReadOnlyList<IActivityLoggerSink> _sinksEx;

        class EmptyTap : ActivityLoggerTap
        {
            public override ActivityLoggerTap Register( IActivityLoggerSink l )
            {
                return this;
            }
            
            // Security if OnFilterChanged is implemented once on ActivityLoggerTap.
            protected override void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
            {
            }

            protected override void OnUnfilteredLog( LogLevel level, string text )
            {
            }

            protected override void OnOpenGroup( IActivityLogGroup group )
            {
            }

            // Security if OnGroupClosing is implemented once on ActivityLoggerTap.
            protected override void OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
            {
            }

            protected override void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
            }
        }

        /// <summary>
        /// Empty <see cref="ActivityLoggerTap"/> (null object design pattern).
        /// </summary>
        static public new readonly ActivityLoggerTap Empty = new EmptyTap();

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerTap"/> bound to a <see cref="IMuxActivityLoggerClientRegistrar"/>.
        /// </summary>
        public ActivityLoggerTap( )
        {
            _curLevel = -1;
            _sinks = new List<IActivityLoggerSink>();
            _sinksEx = new ReadOnlyListOnIList<IActivityLoggerSink>( _sinks );
        }

        /// <summary>
        /// Adds an <see cref="IActivityLoggerSink"/> to the <see cref="RegisteredSinks"/>.
        /// Duplicate <see cref="IActivityLoggerSink"/> are silently ignored.
        /// </summary>
        /// <param name="l">An activity logger sink implementation.</param>
        /// <returns>This tap to enable fluent syntax.</returns>
        public virtual ActivityLoggerTap Register( IActivityLoggerSink l )
        {
            if( !_sinks.Contains( l ) ) _sinks.Add( l );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerSink"/> from the collection of loggers.
        /// Silently ignored unregistered logger.
        /// </summary>
        /// <param name="l">An activity logger sink implementation.</param>
        /// <returns>This tap to enable fluent syntax.</returns>
        public virtual ActivityLoggerTap Unregister( IActivityLoggerSink l )
        {
            _sinks.Remove( l );
            return this;
        }

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerSink"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerSink> RegisteredSinks
        {
            get { return _sinksEx; }
        }

        /// <summary>
        /// Sends log to sinks (handles level changes).
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        protected override void OnUnfilteredLog( LogLevel level, string text )
        {
            if( text == ActivityLogger.ParkLevel )
            {
                if( _curLevel != -1 )
                {
                    foreach( var s in RegisteredSinks ) s.OnLeaveLevel( (LogLevel)_curLevel );
                }
                _curLevel = -1;
            }
            else
            {
                if( _curLevel == (int)level )
                {
                    foreach( var s in RegisteredSinks ) s.OnContinueOnSameLevel( level, text );
                }
                else
                {
                    if( _curLevel != -1 )
                    {
                        foreach( var s in RegisteredSinks ) s.OnLeaveLevel( (LogLevel)_curLevel );
                    }
                    foreach( var s in RegisteredSinks ) s.OnEnterLevel( level, text );
                    _curLevel = (int)level;
                }
            }
        }

        /// <summary>
        /// Sends log to sinks (<see cref="IActivityLoggerSink.OnGroupOpen"/>.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected override void OnOpenGroup( IActivityLogGroup group )
        {
            if( _curLevel != -1 )
            {
                foreach( var s in RegisteredSinks ) s.OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }
            foreach( var s in RegisteredSinks ) s.OnGroupOpen( group );
        }

        /// <summary>
        /// Sends log to sinks (<see cref="IActivityLoggerSink.OnGroupClose"/>.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Texts that conclude the group. Never null but can be empty.</param>
        protected override void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _curLevel != -1 )
            {
                foreach( var s in RegisteredSinks ) s.OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }
            foreach( var s in RegisteredSinks ) s.OnGroupClose( group, conclusions );
        }

    }
}
