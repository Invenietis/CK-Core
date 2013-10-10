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
    public class GrandOutputClient : IActivityMonitorBoundClient
    {
        readonly GrandOutput _central;
        IActivityMonitorImpl _monitorSource;
        string _channelName;

        GrandOutputSource _source;
        IChannel _channel;
        LogFilter _currentMinimalFilter;
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
                        if( _source != null )
                        {
                            _channel.ReleaseInput( _source );
                            _source = null;
                        }
                    }
                    _channel = _central.ObtainChannel( _channelName );
                }
                while( _version != _curVersion );
            }
            _source = _channel.CreateInput( _monitorSource, _channelName );
            _relativeDepth = 0;
            if( _currentMinimalFilter != _channel.MinimalFilter )
            {
                var prev = _currentMinimalFilter;
                _currentMinimalFilter = _channel.MinimalFilter;
                _monitorSource.OnClientMinimalFilterChanged( prev, _channel.MinimalFilter );
            }
            return _channel;
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            ILogEntry e = Impl.LogEntry.CreateLog( text, logTimeUtc, level, tags );
            EnsureChannel().Handle( new GrandOutputEventInfo( _source, e, _relativeDepth )  );
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            ++_relativeDepth;
            ILogEntry e = Impl.LogEntry.CreateOpenGroup( group.GroupText, group.LogTimeUtc, group.GroupLevel, group.GroupTags, group.EnsureExceptionData() );
            EnsureChannel().Handle( new GrandOutputEventInfo( _source, e, _relativeDepth ) );
        }

        public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            ILogEntry e = Impl.LogEntry.CreateCloseGroup( group.CloseLogTimeUtc, group.GroupLevel, conclusions );
            EnsureChannel().Handle( new GrandOutputEventInfo( _source, e, _relativeDepth ) );
            --_relativeDepth;
        }
    }
}
