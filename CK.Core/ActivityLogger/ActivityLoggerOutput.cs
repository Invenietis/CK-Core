using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Base implementation of <see cref="IActivityLoggerOutput"/> for <see cref="IActivityLogger.Output"/>.
    /// </summary>
    public class ActivityLoggerOutput : MuxActivityLoggerHub, IActivityLoggerOutput
    {
        List<IActivityLoggerClient> _clients;
        IReadOnlyList<IActivityLoggerClient> _clientsEx;

        internal class EmptyOutput : IActivityLoggerOutput
        {
            public IMuxActivityLoggerClient ExternalInput
            {
                get { return ActivityLoggerHybridClient.Empty; }
            }

            public IActivityLoggerClientRegistrar RegisterClient( IActivityLoggerClient client )
            {
                return this;
            }

            public IActivityLoggerClientRegistrar UnregisterClient( IActivityLoggerClient client )
            {
                return this;
            }

            public IReadOnlyList<IActivityLoggerClient> RegisteredClients
            {
                get { return ReadOnlyListEmpty<IActivityLoggerClient>.Empty; }
            }

            public IMuxActivityLoggerClientRegistrar RegisterMuxClient( IMuxActivityLoggerClient client )
            {
                return this;
            }

            public IMuxActivityLoggerClientRegistrar UnregisterMuxClient( IMuxActivityLoggerClient client )
            {
                return this;
            }

            public IReadOnlyList<IMuxActivityLoggerClient> RegisteredMuxClients
            {
                get { return ReadOnlyListEmpty<IMuxActivityLoggerClient>.Empty; }
            }
        }

        /// <summary>
        /// Empty <see cref="IActivityLoggerOutput"/> (null object design pattern).
        /// </summary>
        static public readonly IActivityLoggerOutput Empty = new EmptyOutput();

        /// <summary>
        /// Initializes a new <see cref="ActivityLoggerOutput"/> bound to a <see cref="IActivityLogger"/>.
        /// </summary>
        /// <param name="logger"></param>
        public ActivityLoggerOutput( IActivityLogger logger )
        {
            Logger = logger;
            _clients = new List<IActivityLoggerClient>();
            _clientsEx = new ReadOnlyListOnIList<IActivityLoggerClient>( _clients );
        }

        /// <summary>
        /// Gets an entry point for other loggers: by registering this <see cref="IMuxActivityLoggerClient"/> in other <see cref="IActivityLogger.Output"/>,
        /// log data easily be merged.
        /// </summary>
        public IMuxActivityLoggerClient ExternalInput 
        {
            get { return this; } 
        }
        
        /// <summary>
        /// Gets the associated <see cref="IActivityLogger"/>.
        /// </summary>
        protected IActivityLogger Logger { get; private set; }

        /// <summary>
        /// Registers an <see cref="IActivityLoggerClient"/> to the <see cref="RegisteredClients"/> list.
        /// Duplicate IActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IActivityLoggerClientRegistrar RegisterClient( IActivityLoggerClient client )
        {
            if( !_clients.Contains( client ) && OnBeforeAdd( client ) ) _clients.Add( client );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerClient"/> from the <see cref="RegisteredClients"/> list.
        /// Silently ignored unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IActivityLoggerClientRegistrar UnregisterClient( IActivityLoggerClient client )
        {
            if( _clients.Remove( client ) ) OnAfterRemoved( client );
            return this;
        }

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerClient> RegisteredClients
        {
            get { return _clientsEx; }
        }

        /// <summary>
        /// Removes from <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients">RegisteredMuxClients</see> if
        /// the <paramref name="client"/> is also a <see cref="IMuxActivityLoggerClient"/> to avoid stuttering.
        /// </summary>
        /// <param name="client">The client that will be added.</param>
        /// <returns>True to add the client, false to reject it.</returns>
        protected virtual bool OnBeforeAdd( IActivityLoggerClient client )
        {
            IMuxActivityLoggerClient mux = client as IMuxActivityLoggerClient;
            if( mux != null ) UnregisterMuxClient( mux );
            return true;
        }

        /// <summary>
        /// Removes from <see cref="RegisteredClients" /> if the <paramref name="client"/> is 
        /// also a <see cref="IActivityLoggerClient"/> to avoid stuttering.
        /// </summary>
        /// <param name="client">The client that will be added.</param>
        /// <returns>True to add the client, false to reject it.</returns>
        protected override bool OnBeforeAdd( IMuxActivityLoggerClient client )
        {
            IActivityLoggerClient c = client as IActivityLoggerClient;
            if( c != null ) UnregisterClient( c );
            return true;
        }

        /// <summary>
        /// Overriddable method to validate any remove of client.
        /// </summary>
        /// <param name="client">The remove client. Can be added back if necessary.</param>
        protected virtual void OnAfterRemoved( IActivityLoggerClient client )
        {
        }

        internal void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            foreach( var l in _clients ) l.OnFilterChanged( current, newValue );
            ((IMuxActivityLoggerClient)this).OnFilterChanged( Logger, current, newValue );
        }

        internal void OnUnfilteredLog( LogLevel level, string text )
        {
            foreach( var l in _clients ) l.OnUnfilteredLog( level, text );
            ((IMuxActivityLoggerClient)this).OnUnfilteredLog( Logger, level, text );
        }

        internal void OnOpenGroup( IActivityLogGroup group )
        {
            foreach( var l in _clients ) l.OnOpenGroup( group );
            ((IMuxActivityLoggerClient)this).OnOpenGroup( Logger, group );
        }

        internal string OnGroupClosing( IActivityLogGroup group, string conclusion )
        {
            foreach( var l in _clients ) conclusion = l.OnGroupClosing( group, conclusion ) ?? conclusion;
            return ((IMuxActivityLoggerClient)this).OnGroupClosing( Logger, group, conclusion );
        }

        internal void OnGroupClosed( IActivityLogGroup group, string conclusion )
        {
            foreach( var l in _clients ) l.OnGroupClosed( group, conclusion );
            ((IMuxActivityLoggerClient)this).OnGroupClosed( Logger, group, conclusion );
        }
    }
}
