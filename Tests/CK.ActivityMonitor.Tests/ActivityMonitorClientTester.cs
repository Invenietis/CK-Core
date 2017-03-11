using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core.Impl;
using Xunit;
using FluentAssertions;

namespace CK.Core.Tests.Monitoring
{
    class ActivityMonitorClientTester : IActivityMonitorBoundClient
    {
        IActivityMonitorImpl _source;
        LogFilter _minimalFilter;
        int _depth;
        string[] _text;

        public LogFilter MinimalFilter
        {
            get { return _minimalFilter; }
            set
            {
                var prev = _minimalFilter;
                if( prev != value )
                {
                    _minimalFilter = value;
                    if( _source != null ) _source.SetClientMinimalFilterDirty();
                }
            }
        }

        public void AsyncSetMinimalFilter( LogLevelFilter filter, int delayMilliSeconds = 0 )
        {
            ThreadPool.QueueUserWorkItem( DoAsyncSetMinimalFilter, Tuple.Create( TimeSpan.FromMilliseconds( delayMilliSeconds ), filter ) );
        }

        class Flag { public bool Set; }

        public void AsyncSetMinimalFilterBlock( LogFilter filter, int delayMilliSeconds = 0 )
        {
            var state = Tuple.Create( TimeSpan.FromMilliseconds( delayMilliSeconds ), filter, new Flag() );
            ThreadPool.QueueUserWorkItem( DoAsyncSetMinimalFilterBlock, state );
            lock( state ) 
                while( !state.Item3.Set )
                    Monitor.Wait( state );
        }

        void DoAsyncSetMinimalFilter( object state )
        {
            var o = (Tuple<TimeSpan, LogFilter>)state;
            if( o.Item1 != TimeSpan.Zero ) Thread.Sleep( o.Item1 );
            MinimalFilter = o.Item2;
        }

        void DoAsyncSetMinimalFilterBlock( object state )
        {
            var o = (Tuple<TimeSpan, LogFilter,Flag>)state;
            if( o.Item1 != TimeSpan.Zero ) Thread.Sleep( o.Item1 );
            MinimalFilter = o.Item2;
            lock( o )
            {
                o.Item3.Set = true;
                Monitor.Pulse( o );
            }
        }

        void IActivityMonitorBoundClient.SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _source != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            if( source != null )
            {
                Interlocked.Exchange( ref _text, Util.Array.Empty<string>() );
                _source = source;
            }
            else _source = null;
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            data.FileName.Should().NotBeNullOrEmpty();
            Util.InterlockedAdd( ref _text, String.Format( "{0} {1} - {2} -[{3}]", new String( '>', _depth ), data.Level, data.Text, data.Tags ) ); 
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
             group.FileName.Should().NotBeNullOrEmpty();
            int d = Interlocked.Increment( ref _depth );
            Util.InterlockedAdd( ref _text, String.Format( "{0} {1} - {2} -[{3}]", new String( '>', d ), group.GroupLevel, group.GroupText, group.GroupTags ) );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
             fileName.Should().NotBeNullOrEmpty();
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }
    }
}
