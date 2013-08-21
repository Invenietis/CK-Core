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
    public class GrandOutputClient : IActivityMonitorBoundClient
    {
        readonly GrandOutput _central;
        IActivityMonitorImpl _monitorSource;
        string _channelName;

        GrandOutputSource _source;
        IChannel _channel;
        LogLevelFilter _currentMinimalFilter;
        int _relativeDepth;
        int _curVersion;
        int _version;

        internal GrandOutputClient( GrandOutput central )
        {
            _central = central;
            _channelName = String.Empty;
        }

        /// <summary>
        /// forceBuggyRemove is not used here since this client is not lockable.
        /// </summary>
        LogLevelFilter IActivityMonitorBoundClient.SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _monitorSource != null ) throw ActivityMonitorClient.NewMultipleRegisterOnBoundClientException( this );
            // Silently ignore null => null or monitor => same monitor.
            if( source != _monitorSource )
            {
                _currentMinimalFilter = LogLevelFilter.None;
                Debug.Assert( (source == null) != (_monitorSource == null) );
                if( (_monitorSource = source) == null )
                {
                    Thread.MemoryBarrier();
                    // Releases the channel if any.
                    if( _channel != null )
                    {
                        _channel.ReleaseInput( _source );
                        _source = null;
                        _channel = null;
                    }
                }
                else
                {
                    Interlocked.Increment( ref _version );
                }
            }
            return _currentMinimalFilter;
        }

        public GrandOutput Central
        {
            get { return _central; }
        }

        public string ChannelName
        {
            get { return _channelName; }
            set
            {
                if( value == null ) value = String.Empty;
                if( _channelName != value )
                {
                    _channelName = value;
                    Interlocked.Increment( ref _version );
                }
            }
        }

        internal void OnChannelConfigurationChanged()
        {
            Interlocked.Increment( ref _version );
        }

        IChannel EnsureChannel( IActivityMonitorImpl monitorSource )
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
                        if( _source != null )
                        {
                            _channel.ReleaseInput( _source );
                            _source = null;
                        }
                    }
                    _channel = _central.ObtainChannel( monitorSource.UniqueId, _channelName );
                }
                while( _version != _curVersion );

            }
            _source = _channel.CreateInput( monitorSource, _channelName );
            _relativeDepth = 0;
            //_currentMinimalFilter = _channel.MinimalFilter;
            return _channel;
        }

        void IActivityMonitorClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            Thread.MemoryBarrier();
            var monitorSource = _monitorSource;
            if( monitorSource != null )
            {
                ILogEntry e = Impl.LogEntry.CreateLog( text, logTimeUtc, level, tags );
                EnsureChannel( monitorSource ).Handle( new GrandOutputEventInfo( _source, e, _relativeDepth )  );
            }
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            Thread.MemoryBarrier();
            var monitorSource = _monitorSource;
            if( monitorSource != null )
            {
                ++_relativeDepth;
                ILogEntry e = Impl.LogEntry.CreateOpenGroup( group.GroupText, group.LogTimeUtc, group.GroupLevel, group.GroupTags, group.Exception );
                EnsureChannel( monitorSource ).Handle( new GrandOutputEventInfo( _source, e, _relativeDepth ) );
            }
        }

        public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            Thread.MemoryBarrier();
            var monitorSource = _monitorSource;
            if( monitorSource != null )
            {
                ILogEntry e = Impl.LogEntry.CreateCloseGroup( group.CloseLogTimeUtc, group.GroupLevel, conclusions );
                EnsureChannel( monitorSource ).Handle( new GrandOutputEventInfo( _source, e, _relativeDepth ) );
                --_relativeDepth;
            }
        }
    }
}
