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
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// A tap is a <see cref="IActivityLoggerClient"/> that delivers log data 
    /// to multiple <see cref="IActivityLoggerSink"/> implementations.
    /// </summary>
    public class ActivityLoggerTap : ActivityLoggerClient, IActivityLoggerBoundClient
    {
        int _curLevel;
        List<IActivityLoggerSink> _sinks;
        ICKReadOnlyList<IActivityLoggerSink> _sinksEx;
        IActivityLogger _source;
        readonly bool _locked;

        [ExcludeFromCodeCoverage]
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

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
            }

            protected override void OnOpenGroup( IActivityLogGroup group )
            {
            }

            // Security if OnGroupClosing is implemented once on ActivityLoggerTap.
            protected override void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
            {
            }

            protected override void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
            }
        }

        /// <summary>
        /// Empty <see cref="ActivityLoggerTap"/> (null object design pattern).
        /// </summary>
        static public new readonly ActivityLoggerTap Empty = new EmptyTap();

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerTap"/>.
        /// </summary>
        public ActivityLoggerTap()
        {
            _curLevel = -1;
            _sinks = new List<IActivityLoggerSink>();
            _sinksEx = new CKReadOnlyListOnIList<IActivityLoggerSink>( _sinks );
        }

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerTap"/> as the default <see cref="IDefaultActivityLogger.Tap"/>.
        /// It can not be unregistered.
        /// </summary>
        public ActivityLoggerTap( IDefaultActivityLogger logger )
            : this()
        {
            logger.Output.RegisterClient( this );
            _locked = true;
        }

        void IActivityLoggerBoundClient.SetLogger( IActivityLogger source, bool forceBuggyRemove )
        {
            if( !forceBuggyRemove )
            {
                if( _locked ) throw new InvalidOperationException( R.CanNotUnregisterDefaultClient );
                if( source != null && _source != null ) throw new InvalidOperationException( String.Format( R.ActivityLoggerBoundClientMultipleRegister, GetType().FullName ) );
            }
            _source = source;
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
        public ICKReadOnlyList<IActivityLoggerSink> RegisteredSinks
        {
            get { return _sinksEx; }
        }

        /// <summary>
        /// Sends log to sinks (handles level changes).
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityLogger.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            if( text == ActivityLogger.ParkLevel )
            {
                if( _curLevel != -1 )
                {
                    SafeCall( s => s.OnLeaveLevel( (LogLevel)_curLevel ) );
                }
                _curLevel = -1;
            }
            else
            {
                if( _curLevel == (int)level )
                {
                    SafeCall( s => s.OnContinueOnSameLevel( tags, level, text, logTimeUtc ) );
                }
                else
                {
                    if( _curLevel != -1 )
                    {
                        SafeCall( s => s.OnLeaveLevel( (LogLevel)_curLevel ) );
                    }
                    SafeCall( s => s.OnEnterLevel( tags, level, text, logTimeUtc ) );
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
                SafeCall( s => s.OnLeaveLevel( (LogLevel)_curLevel ) );
                _curLevel = -1;
            }
            SafeCall( s => s.OnGroupOpen( group ) );
        }

        /// <summary>
        /// Sends log to sinks (<see cref="IActivityLoggerSink.OnGroupClose"/>.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Texts that conclude the group. Never null but can be empty.</param>
        protected override void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _curLevel != -1 )
            {
                SafeCall( s => s.OnLeaveLevel( (LogLevel)_curLevel ) );
                _curLevel = -1;
            }
            SafeCall( s => s.OnGroupClose( group, conclusions ) );
        }

        void SafeCall( Action<IActivityLoggerSink> a )
        {
            List<IActivityLoggerSink> buggySinks = null;
            foreach( var s in _sinks )
            {
                try
                {
                    a( s );
                }
                catch( Exception exCall )
                {
                    ActivityLogger.LoggingError.Add( exCall, s.GetType().FullName );
                    if( buggySinks == null ) buggySinks = new List<IActivityLoggerSink>();
                    buggySinks.Add( s );
                }
            }
            if( buggySinks != null ) foreach( var s in buggySinks ) _sinks.Remove( s );
        }

    }
}
