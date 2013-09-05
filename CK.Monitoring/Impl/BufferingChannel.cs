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

namespace CK.Monitoring.Impl
{

    /// <summary>
    /// This kind of channel is bound to a <see cref="GrandOutputClient"/>. It is returned by <see cref="GrandOutput.ObtainChannel"/>
    /// when a configuration is being applied.
    /// </summary>
    class BufferingChannel : IChannel
    {
        readonly IGrandOutputSink _commonSink;
        readonly CountdownEvent _useLock;
        readonly ConcurrentQueue<GrandOutputEventInfo> _buffer;
        readonly object _flushLock;

        internal BufferingChannel( IGrandOutputSink commonSink )
        {
            _commonSink = commonSink;
            _useLock = new CountdownEvent( 0 );
            _buffer = new ConcurrentQueue<GrandOutputEventInfo>();
            _flushLock = new Object();
        }

        public void Initialize()
        {
        }

        public GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName )
        {
            return new GrandOutputSource( monitor, channelName );
        }

        public void ReleaseInput( GrandOutputSource source )
        {
        }

        public LogLevelFilter MinimalFilter
        {
            get { return LogLevelFilter.None; }
        }

        public void Handle( GrandOutputEventInfo logEvent )
        {
            _commonSink.Handle( logEvent );
            _buffer.Enqueue( logEvent );
            _useLock.Signal();
        }

        public void HandleBuffer( List<GrandOutputEventInfo> list )
        {
            throw new NotSupportedException( "BufferingChannel does not handle buffered events." );
        }
        
        public void PreHandleLock()
        {
            _useLock.AddCount();
        }

        public void CancelPreHandleLock()
        {
            _useLock.Signal();
        }

        internal void EnsureActive()
        {
            Debug.Assert( Monitor.IsEntered( _flushLock ) );
            if( _useLock.CurrentCount == 0 ) _useLock.Reset( 1 );
        }

        internal object FlushLock
        {
            get { return _flushLock; }
        }

        /// <summary>
        /// Flushes all buffered GrandOutputEventInfo into appropriate channels.
        /// This is the only step during wich a lock blocks GrandOutput.ObtainChannel calls.
        /// </summary>
        /// <param name="newChannels">Function that knows how to return the channel to uses based on its name.</param>
        internal void FlushBuffer( Func<string,IChannel> newChannels )
        {
            Debug.Assert( Monitor.IsEntered( _flushLock ) );
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
                Dictionary<IChannel,List<GrandOutputEventInfo>> routedEvents = new Dictionary<IChannel, List<GrandOutputEventInfo>>();
                GrandOutputEventInfo e;
                while( _buffer.TryDequeue( out e ) )
                {
                    IChannel c = newChannels( e.Source.ChannelName );
                    List<GrandOutputEventInfo> events = routedEvents.GetValueWithDefaultFunc( c, channel => new List<GrandOutputEventInfo>() );
                    events.Add( e );
                }
                foreach( var pair in routedEvents )
                {
                    pair.Key.HandleBuffer( pair.Value );
                }
            }
            Debug.Assert( _useLock.CurrentCount == 0 );
        }

    }
}
