using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;

namespace CK.Monitoring
{

    /// <summary>
    /// A GrandOutputClient is a <see cref="IActivityMonitorClient"/> that can only be obtained and registered
    /// through <see cref="GrandOutput.Register"/>.
    /// </summary>
    public sealed class GrandOutputClient : IActivityMonitorBoundClient
    {
        readonly GrandOutput _central;
        IActivityMonitorImpl _monitorSource;

        IChannel _channel;
        LogFilter _currentMinimalFilter;
        
        int _currentGroupDepth;
        int _curVersion;
        int _version;

        internal GrandOutputClient( GrandOutput central )
        {
            _central = central;
        }

        /// <summary>
        /// forceBuggyRemove is not used here since this client is not lockable.
        /// </summary>
        void IActivityMonitorBoundClient.SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _monitorSource != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            // Silently ignore null => null or monitor => same monitor.
            if( source != _monitorSource )
            {
                _currentMinimalFilter = LogFilter.Undefined;
                Debug.Assert( (source == null) != (_monitorSource == null) );
                if( (_monitorSource = source) == null )
                {
                    // Releases the channel if any.
                    _channel = null;
                }
                else
                {
                    Interlocked.Increment( ref _version );
                }
            }
        }

        public GrandOutput Central
        {
            get { return _central; }
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            // Next log will obtain a new channel: The Info with the TopiChanged
            // will appear in the new channel.
            Interlocked.Increment( ref _version );
        }
        
        public LogFilter MinimalFilter { get { return _currentMinimalFilter; } }

        internal void OnChannelConfigurationChanged()
        {
            Interlocked.Increment( ref _version );
        }

        IChannel EnsureChannel()
        {
            if( _channel != null ) _channel.PreHandleLock();
            if( _version != _curVersion )
            {
                do
                {
                    _curVersion = _version;
                    if( _channel != null )
                    {
                        _channel.CancelPreHandleLock();
                    }
                    // The Topic can be changed only in the 
                    // activity (and we are here inside the activity),
                    // we can safely call the property when needed.
                    // When ObtainChannel retunrs null, it means that the GrandOutput has been disposed.
                    _channel = _central.ObtainChannel( _monitorSource.Topic );
                    if( _channel == null ) return null;
                }
                while( _version != _curVersion );
            }
            var g = _monitorSource.CurrentGroup;
            _currentGroupDepth = g != null ? g.Depth : 0;
            if( _currentMinimalFilter != _channel.MinimalFilter )
            {
                var prev = _currentMinimalFilter;
                _currentMinimalFilter = _channel.MinimalFilter;
                _monitorSource.OnClientMinimalFilterChanged( prev, _channel.MinimalFilter );
            }
            return _channel;
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            var h = EnsureChannel();
            if( h != null )
            {
                IMulticastLogEntry e = LogEntry.CreateMulticastLog( _monitorSource.UniqueId, _currentGroupDepth, data.Text, data.LogTime, data.Level, data.FileName, data.LineNumber, data.Tags, data.EnsureExceptionData() );
                h.Handle( new GrandOutputEventInfo( e, _monitorSource.Topic ) );
            }
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            var h = EnsureChannel();
            if( h != null )
            {
                ++_currentGroupDepth;
                IMulticastLogEntry e = LogEntry.CreateMulticastOpenGroup( _monitorSource.UniqueId, _currentGroupDepth, group.GroupText, group.LogTime, group.GroupLevel, group.FileName, group.LineNumber, group.GroupTags, group.EnsureExceptionData() );
                h.Handle( new GrandOutputEventInfo( e, _monitorSource.Topic ) );
            }
        }

        public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            var h = EnsureChannel();
            if( h != null )
            {
                IMulticastLogEntry e = LogEntry.CreateMulticastCloseGroup( _monitorSource.UniqueId, _currentGroupDepth, group.CloseLogTime, group.GroupLevel, conclusions );
                h.Handle( new GrandOutputEventInfo( e, _monitorSource.Topic ) );
                --_currentGroupDepth;
            }
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }
    }
}
