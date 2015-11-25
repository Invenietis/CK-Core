using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

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
                        ActivityMonitor.CriticalErrorCollector.Add( ex, "While logging event into Global sinks." );
                    }
                }
                try
                {
                    foreach( var h in Handlers ) h.Handle( e, false );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.CriticalErrorCollector.Add( ex, "While logging event." );
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
        readonly Func<int,int> _idleManager;
        readonly Action<TimeSpan> _onIdle;
        int _currentIdleCount;
        int _nonBlockingCount;
        IGrandOutputDispatcherStrategy  _strat;
        int _maxQueuedCount;
        int _eventLostCount;
        DateTime _nextCapacityError;
        object _overloadLock;
        bool _overloadedErrorWaiting;

        public EventDispatcher( IGrandOutputDispatcherStrategy strategy, Action<TimeSpan> onIdle = null )
        {
            Debug.Assert( strategy != null );
            _queue = new ConcurrentQueue<EventItem>();
            _dispatchLock = new object();
            _strat = strategy;
            _onIdle = onIdle;
            _overloadLock = new object();
            _thread = new Thread( Run );
            _thread.IsBackground = true;
            _strat.Initialize( () => _nonBlockingCount, _thread, out _idleManager );
            _thread.Start();
        }

        ~EventDispatcher()
        {
            // Since the Queue is a managed object, we can not use it
            // to send the MustStop message.
            // The only thing to do here is to abort the thread.
            #if NET451 || NET46
            _thread.Abort();
            #endif
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
                Interlocked.MemoryBarrier();
            }
            else
            {
                // Normal message.
                Interlocked.MemoryBarrier();
                var strat = _strat;
                if( strat == null ) return false;
                if( strat.IsOpened( ref _maxQueuedCount ) )
                {
                    // Normal message and no queue overload detected.
                    Interlocked.Increment( ref _nonBlockingCount );
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
                Interlocked.MemoryBarrier();
            }
            // Whatever happens, if a "Lost Event" monitoring error must be send once, 
            // checks to see if we must send it now.
            Interlocked.MemoryBarrier();
            if( _overloadedErrorWaiting )
            {
                var now = receiver != null ? e.Entry.LogTime.TimeUtc : DateTime.MaxValue;
                if( now > _nextCapacityError )
                {
                    // Double check locking.
                    lock( _overloadLock )
                    {
                        if( _overloadedErrorWaiting && now > _nextCapacityError )
                        {
                            ActivityMonitor.CriticalErrorCollector.Add( new CKException( "GrandOutput dispatcher overload. Lost {0} total events.", _eventLostCount ), null );
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
            DateTime startIdleTime;
            for(;;)
            {
                EventItem e;
                while( _queue.TryDequeue( out e ) )
                {
                    _currentIdleCount = 0;
                    Interlocked.Decrement( ref _nonBlockingCount );
                    if( e.MustStop ) return;
                    e.Receiver.Dispatch( e.EventInfo );
                }
                startIdleTime = DateTime.UtcNow;
                for(;;)
                {
                    bool hasEvent = true;
                    lock( _dispatchLock )
                        while( _queue.IsEmpty )
                            hasEvent = Monitor.Wait( _dispatchLock, _idleManager( _currentIdleCount++ ) );
                    if( hasEvent ) break;
                    if( _onIdle != null ) _onIdle( DateTime.UtcNow - startIdleTime );
                }
            }
        }

        public bool IsDisposed { get { return _strat == null; } }

        public void Dispose()
        {
            Interlocked.MemoryBarrier();
            var strat = _strat;
            if( strat != null )
            {
                _strat = null;
                Interlocked.MemoryBarrier();
                DoAdd( new GrandOutputEventInfo(), null );
                GC.SuppressFinalize( this );
                _thread.Join();
            }
        }
    }
}
