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
        LogEntryType _prevLogType;
        DateTimeStamp _prevlogTime;
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
                _prevLogType = LogEntryType.None;
                _prevlogTime = DateTimeStamp.Unknown;
                Debug.Assert( (source == null) != (_monitorSource == null) );
                if( (_monitorSource = source) == null )
                {
                    // Releases the channel if any.
                    _channel = null;
                }
                else
                {
                    var g = _monitorSource.CurrentGroup;
                    _currentGroupDepth = g != null ? g.Depth : 0;
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
            _monitorSource.SetClientMinimalFilterDirty();
        }

        LogFilter IActivityMonitorBoundClient.MinimalFilter 
        { 
            get 
            {
                if( _version != _curVersion ) 
                { 
                    var c = EnsureChannel( false );
                    if( c != null ) c.CancelPreHandleLock();
                }
                return _currentMinimalFilter; 
            } 
        }

        internal bool IsBoundToMonitor
        {
            get { return _monitorSource != null; }
        }

        internal bool OnChannelConfigurationChanged()
        {
            Interlocked.Increment( ref _version );
            var m = _monitorSource;
            if( m == null ) return false;
            m.SetClientMinimalFilterDirty();
            return true;
        }

        IChannel EnsureChannel( bool callOnClientMinimalFilterChanged = true )
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
                    // When ObtainChannel returns null, it means that the GrandOutput has been disposed.
                    _channel = _central.ObtainChannel( _monitorSource.Topic );
                    if( _channel == null )
                    {
                        CheckFilter( callOnClientMinimalFilterChanged, LogFilter.Undefined );
                        return null;
                    }
                }
                while( _version != _curVersion );
                CheckFilter( callOnClientMinimalFilterChanged, _channel.MinimalFilter );
            }
            return _channel;
        }

        void CheckFilter( bool callOnClientMinimalFilterChanged, LogFilter f )
        {
            if( _currentMinimalFilter != f )
            {
                var prev = _currentMinimalFilter;
                _currentMinimalFilter = f;
                if( callOnClientMinimalFilterChanged ) _monitorSource.OnClientMinimalFilterChanged( prev, f );
            }
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            var h = EnsureChannel();
            if( h != null )
            {
                IMulticastLogEntry e = LogEntry.CreateMulticastLog( _monitorSource.UniqueId, _prevLogType, _prevlogTime, _currentGroupDepth, data.Text, data.LogTime, data.Level, data.FileName, data.LineNumber, data.Tags, data.EnsureExceptionData() );
                h.Handle( new GrandOutputEventInfo( e, _monitorSource.Topic ) );
                _prevlogTime = data.LogTime;
                _prevLogType = LogEntryType.Line;
            }
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            var h = EnsureChannel();
            if( h != null )
            {
                IMulticastLogEntry e = LogEntry.CreateMulticastOpenGroup( _monitorSource.UniqueId, _prevLogType, _prevlogTime, _currentGroupDepth, group.GroupText, group.LogTime, group.GroupLevel, group.FileName, group.LineNumber, group.GroupTags, group.EnsureExceptionData() );
                h.Handle( new GrandOutputEventInfo( e, _monitorSource.Topic ) );
                ++_currentGroupDepth;
                _prevlogTime = group.LogTime;
                _prevLogType = LogEntryType.OpenGroup;
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
                IMulticastLogEntry e = LogEntry.CreateMulticastCloseGroup( _monitorSource.UniqueId, _prevLogType, _prevlogTime, _currentGroupDepth, group.CloseLogTime, group.GroupLevel, conclusions );
                h.Handle( new GrandOutputEventInfo( e, _monitorSource.Topic ) );
                --_currentGroupDepth;
                _prevlogTime = group.CloseLogTime;
                _prevLogType = LogEntryType.CloseGroup;
            }
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }
    }
}
