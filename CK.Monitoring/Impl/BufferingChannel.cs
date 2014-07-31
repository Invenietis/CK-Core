using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.GrandOutputHandlers;

namespace CK.Monitoring.Impl
{

    /// <summary>
    /// This kind of channel is bound to a <see cref="GrandOutputClient"/>. It is returned by <see cref="GrandOutput.ObtainChannel"/>
    /// when a configuration is being applied.
    /// </summary>
    internal sealed class BufferingChannel : IChannel, IDisposable
    {
        readonly EventDispatcher _dispatcher;
        readonly EventDispatcher.FinalReceiver _receiver;
        readonly CountdownEvent _useLock;
        readonly ConcurrentQueue<GrandOutputEventInfo> _buffer;
        readonly object _flushLock;

        internal BufferingChannel( EventDispatcher dispatcher, EventDispatcher.FinalReceiver commonSinkOnly )
        {
            _dispatcher = dispatcher;
            _receiver = commonSinkOnly;
            _useLock = new CountdownEvent( 0 );
            _buffer = new ConcurrentQueue<GrandOutputEventInfo>();
            _flushLock = new Object();
        }

        public void Initialize()
        {
        }

        public LogFilter MinimalFilter
        {
            get { return LogFilter.Undefined; }
        }

        public void Handle( GrandOutputEventInfo logEvent, bool sendToCommonSink )
        {
            Debug.Assert( sendToCommonSink == true );
            try
            {
                _dispatcher.Add( logEvent, _receiver );
                _buffer.Enqueue( logEvent );
            }
            finally
            {
                _useLock.Signal();
            }
        }
        
        public void PreHandleLock()
        {
            _useLock.AddCount();
        }

        public void CancelPreHandleLock()
        {
            _useLock.Signal();
        }

        /// <summary>
        /// Can be called either from GrandOutput.OnConfigurationClosing or from GrandOutput.ObtainChannel when the FlushLock is acquired.
        /// This sets the CountdownEvent to 1. 
        /// </summary>
        internal void EnsureActive()
        {
            #if !net40
            Debug.Assert( Monitor.IsEntered( _flushLock ) );
            #endif
            if( _useLock.CurrentCount == 0 ) _useLock.Reset( 1 );
        }

        internal object FlushLock
        {
            get { return _flushLock; }
        }

        /// <summary>
        /// Flushes all buffered GrandOutputEventInfo into appropriate channels.
        /// It is called by the GrandOutput.OnConfigurationReady method to transfer buffered log events
        /// into the appropriate new routes.
        /// This is the only step during which a lock blocks GrandOutput.ObtainChannel calls.
        /// </summary>
        /// <param name="newChannels">Function that knows how to return the channel to uses based on the topic string.</param>
internal void FlushBuffer( Func<string,IChannel> newChannels )
{
    #if !net40
    Debug.Assert( Monitor.IsEntered( _flushLock ) );
    #endif
    Debug.Assert( _useLock.CurrentCount >= 1 );

    _useLock.Signal();
    _useLock.Wait();
    if( newChannels == null )
    {
        GrandOutputEventInfo e;
        while( _buffer.TryDequeue( out e ) );
    }
    else
    {
        GrandOutputEventInfo e;
        while( _buffer.TryDequeue( out e ) )
        {
            IChannel c = newChannels( e.Topic );
            c.Handle( e, false );
        }
    }
    Debug.Assert( _useLock.CurrentCount == 0 );
}


        public void Dispose()
        {
            _useLock.Dispose();
        }
    }
}
