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
    public class ActivityLoggerTap : IActivityLoggerClient, IMuxActivityLoggerClient
    {
        int _curLevel;
        List<IActivityLoggerSink> _sinks;
        IReadOnlyList<IActivityLoggerSink> _sinksEx;

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerTap"/> bound to a <see cref="IMuxActivityLoggerClientRegistrar"/>.
        /// </summary>
        /// <param name="server">The <see cref="IMuxActivityLoggerClientRegistrar"/> to listen to.</param>
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

        void IActivityLoggerClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
        }

        void IActivityLoggerClient.OnUnfilteredLog( LogLevel level, string text )
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

        void IActivityLoggerClient.OnOpenGroup( IActivityLogGroup group )
        {
            if( _curLevel != -1 )
            {
                foreach( var s in RegisteredSinks ) s.OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }
            foreach( var s in RegisteredSinks ) s.OnGroupOpen( group );
        }

        string IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, string conclusion )
        {
            return null;
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, string conclusion )
        {
            if( _curLevel != -1 )
            {
                foreach( var s in RegisteredSinks ) s.OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }
            foreach( var s in RegisteredSinks ) s.OnGroupClose( group, conclusion );
        }

        #region IMuxActivityLoggerClient relayed to IActivityLoggerClient

        void IMuxActivityLoggerClient.OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue )
        {
            ((IActivityLoggerClient)this).OnFilterChanged( current, newValue );
        }

        void IMuxActivityLoggerClient.OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text )
        {
            ((IActivityLoggerClient)this).OnUnfilteredLog( level, text );
        }

        void IMuxActivityLoggerClient.OnOpenGroup( IActivityLogger sender, IActivityLogGroup group )
        {
            ((IActivityLoggerClient)this).OnOpenGroup( group );
        }

        string IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            return ((IActivityLoggerClient)this).OnGroupClosing( group, conclusion );
        }

        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            ((IActivityLoggerClient)this).OnGroupClosed( group, conclusion );
        }

        #endregion
    }
}
