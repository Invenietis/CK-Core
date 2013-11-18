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
    internal sealed class EventDispatcher : IDisposable
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

        static readonly TimeSpan _delayBetweenCapacityError = TimeSpan.FromMinutes( 2 );

        readonly ConcurrentQueue<EventItem> _queue;
        readonly object _dispatchLock;
        readonly Thread _thread;
        IGrandOutputDispatcherStrategy  _strat;
        int _maxQueuedCount;
        int _eventLostCount;
        DateTime _nextCapacityError;
        object _overloadLock;
        bool _overloadedErrorWaiting;

        public EventDispatcher( IGrandOutputDispatcherStrategy strategy )
        {
            Debug.Assert( strategy != null );
            _queue = new ConcurrentQueue<EventItem>();
            _dispatchLock = new object();
            _strat = strategy;
            _overloadLock = new object();
            _thread = new Thread( Run );
            _strat.Initialize( () => _queue.Count, _thread );
            _thread.Start();
        }

        ~EventDispatcher()
        {
            // Since the Queue is a managed object, we can not use it
            // to send the MustStop message.
            // The only thing to do here is to abort the thread.
            _thread.Abort();
        }

        public int LostEventCount { get { return _eventLostCount; } }

        public int MaxQueuedCount { get { return _maxQueuedCount; } }

        public bool Add( GrandOutputEventInfo e, FinalReceiver receiver )
        {
            if( receiver == null ) throw new ArgumentNullException();
            return DoAdd( e, receiver );
        }

        bool DoAdd( GrandOutputEventInfo e, FinalReceiver receiver )
        {
            bool result = true;
            Debug.Assert( e.Entry != null || receiver == null, "Only the MustStop item has null everywhere." );
            if( receiver == null )
            {
                // This is the MustStop message.
                _queue.Enqueue( new EventItem( e, null ) );
                lock( _dispatchLock ) Monitor.Pulse( _dispatchLock );
                // Ensures that if _overloadedErrorWaiting is true, a final "Lost Event" monitoring error is sent.
                _nextCapacityError = DateTime.MinValue;
                Thread.MemoryBarrier();
            }
            else
            {
                // Normal message.
                Thread.MemoryBarrier();
                var strat = _strat;
                if( strat == null ) return false;
                if( strat.IsOpened( ref _maxQueuedCount ) )
                {
                    // Normal message and no queue overload detected.
                    _queue.Enqueue( new EventItem( e, receiver ) );
                    lock( _dispatchLock ) Monitor.Pulse( _dispatchLock );
                }
                else
                {
                    // Overload has been detected.
                    // Unlock the configuration: the message will not be handled.
                    if( receiver.ConfigLock != null ) receiver.ConfigLock.Unlock();
                    Interlocked.Increment( ref _eventLostCount );
                    // A new "Lost Event" monitoring error must be sent once.
                    _overloadedErrorWaiting = true;
                    result = false;
                }
                Thread.MemoryBarrier();
            }
            // Whatever happens, if a "Lost Event" monitoring error must be send once, 
            // checks to see if we must send it now.
            Thread.MemoryBarrier();
            if( _overloadedErrorWaiting )
            {
                var now = receiver != null ? e.Entry.LogTimeUtc : DateTime.MaxValue;
                if( now > _nextCapacityError )
                {
                    // Double check locking.
                    lock( _overloadLock )
                    {
                        if( _overloadedErrorWaiting && now > _nextCapacityError )
                        {
                            ActivityMonitor.MonitoringError.Add( new CKException( "GrandOutput dispatcher overload. Lost {0} total events.", _eventLostCount ), null );
                            if( receiver != null ) _nextCapacityError = now.Add( _delayBetweenCapacityError );
                            _overloadedErrorWaiting = false;
                        }
                    }
                }
            }
            return result;
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
                    while( _queue.IsEmpty )
                        Monitor.Wait( _dispatchLock );
            }
        }

        public bool IsDisposed { get { return _strat == null; } }

        public void Dispose()
        {
            Thread.MemoryBarrier();
            var strat = _strat;
            if( strat != null )
            {
                _strat = null;
                Thread.MemoryBarrier();
                DoAdd( new GrandOutputEventInfo(), null );
                GC.SuppressFinalize( this );
            }
        }

        /// <summary>
        /// Gets the count of concurrent sampling: each time <see cref="IsOpened"/> has been
        /// called while it was already called by another thread.
        /// </summary>
        public int IgnoredConcurrentCallCount
        {
            get { return _strat.IgnoredConcurrentCallCount; }
        }
    }
}
