using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;

namespace CK.Monitoring.Impl
{
    internal class EventDispatcher : IDisposable
    {
        public class FinalReceiver
        {
            public readonly IGrandOutputSink CommonSink;
            public readonly HandlerBase[] Handlers;
            public readonly IRouteConfigurationLock ConfigLock;

            public FinalReceiver( IGrandOutputSink common, HandlerBase[] handlers, IRouteConfigurationLock configLock )
            {
                Debug.Assert( handlers != null );
                CommonSink = common;
                Handlers = handlers;
                ConfigLock = configLock;
            }

            internal void Dispatch( GrandOutputEventInfo e )
            {
                if( CommonSink != null )
                {
                    try
                    {
                        CommonSink.Handle( e, false );
                        foreach( var h in Handlers ) h.Handle( e, false );
                    }
                    catch( Exception ex )
                    {
                        ActivityMonitor.MonitoringError.Add( ex, "While logging event into Global sinks." );
                    }
                }
                try
                {
                    foreach( var h in Handlers ) h.Handle( e, false );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.MonitoringError.Add( ex, "While logging event." );
                }
                finally
                {
                    if( ConfigLock != null ) ConfigLock.Unlock();
                }
            }
        }

        struct EventItem
        {
            public readonly GrandOutputEventInfo EventInfo;
            public readonly FinalReceiver Receiver;

            public EventItem( GrandOutputEventInfo e, FinalReceiver receiver )
            {
                EventInfo = e;
                Receiver = receiver;
            }

            public bool MustStop { get { return Receiver == null; } }
        }

        static readonly TimeSpan _delayBetweenCapacityError = TimeSpan.FromMinutes( 1 );

        readonly ConcurrentQueue<EventItem> _queue;
        readonly object _dispatchLock;
        readonly int _maxCapacity;
        int _eventLostCount;
        DateTime _nextCapacityError;
        bool _disposed;

        public EventDispatcher()
        {
            _queue = new ConcurrentQueue<EventItem>();
            _dispatchLock = new object();
            _maxCapacity = 10000;
            new Thread( Run ).Start();
        }

        ~EventDispatcher()
        {
            if( !_disposed ) Add( new GrandOutputEventInfo(), null );
        }

        public void Add( GrandOutputEventInfo e, FinalReceiver receiver )
        {
            Debug.Assert( e.Entry != null || receiver == null, "Only the MustStop item has null everywhere." );
            if( _queue.Count > _maxCapacity )
            {
                int nbLost = Interlocked.Increment( ref _eventLostCount );
                var now = DateTime.UtcNow;
                if( now > _nextCapacityError )
                {
                    ActivityMonitor.MonitoringError.Add( new CKException( "GrandOutput dispatcher overload. Lost {0} total events.", nbLost ), null );
                    _nextCapacityError = now.Add( _delayBetweenCapacityError );
                }
            }
            else
            {
                _queue.Enqueue( new EventItem( e, receiver ) );
                lock( _dispatchLock ) Monitor.Pulse( _dispatchLock );
            }
        }

        void Run()
        {
            for(;;)
            {
                EventItem e;
                while( _queue.TryDequeue( out e ) )
                {
                    if( e.MustStop ) return;
                    e.Receiver.Dispatch( e.EventInfo );
                }
                lock( _dispatchLock )
                    while( _queue.Count == 0 )
                        Monitor.Wait( _dispatchLock );
            }
        }

        public bool IsDisposed { get { return _disposed; } }

        public void Dispose()
        {
            if( !_disposed )
            {
                _disposed = true;
                GC.SuppressFinalize( this );
                Add( new GrandOutputEventInfo(), null );
            }
        }

    }
}
