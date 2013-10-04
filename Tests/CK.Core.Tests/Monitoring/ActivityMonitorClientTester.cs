using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core.Impl;

namespace CK.Core.Tests.Monitoring
{
    class ActivityMonitorClientTester : IActivityMonitorBoundClient
    {
        IActivityMonitorImpl _source;
        LogLevelFilter _minimalFilter;
        int _depth;
        string[] _text;

        public LogLevelFilter MinimalFilter
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

        public void AsyncSetMinimalFilterBlock( LogLevelFilter filter, int delayMilliSeconds = 0 )
        {
            var state = Tuple.Create( TimeSpan.FromMilliseconds( delayMilliSeconds ), filter, new Flag() );
            ThreadPool.QueueUserWorkItem( DoAsyncSetMinimalFilterBlock, state );
            lock( state ) 
                while( !state.Item3.Set )
                    Monitor.Wait( state );
        }

        void DoAsyncSetMinimalFilter( object state )
        {
            var o = (Tuple<TimeSpan, LogLevelFilter>)state;
            if( o.Item1 != TimeSpan.Zero ) Thread.Sleep( o.Item1 );
            MinimalFilter = o.Item2;
        }

        void DoAsyncSetMinimalFilterBlock( object state )
        {
            var o = (Tuple<TimeSpan, LogLevelFilter,Flag>)state;
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
                Interlocked.Exchange( ref _text, Util.EmptyStringArray );
                _source = source;
            }
            else _source = null;
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            Util.InterlockedAdd( ref _text, String.Format( "{0} {1} - {2} -[{3}]", new String( '>', _depth ), level, text, tags ) ); 
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            int d = Interlocked.Increment( ref _depth );
            Util.InterlockedAdd( ref _text, String.Format( "{0} {1} - {2} -[{3}]", new String( '>', d ), group.GroupLevel, group.GroupText, group.GroupTags ) );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
        }
    }
}
