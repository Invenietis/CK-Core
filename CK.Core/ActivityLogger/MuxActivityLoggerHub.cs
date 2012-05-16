using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Implementation of a <see cref="IMuxActivityLoggerClientRegistrar"/> that can register multiple <see cref="IMuxActivityLoggerClient"/> 
    /// and that is itself a <see cref="IMuxActivityLoggerClient"/>.
    /// </summary>
    public class MuxActivityLoggerHub : IMuxActivityLoggerClient, IMuxActivityLoggerClientRegistrar
    {
        List<IMuxActivityLoggerClient> _clients;
        IReadOnlyList<IMuxActivityLoggerClient> _clientsEx;

        /// <summary>
        /// Initializes a new <see cref="MuxActivityLoggerHub"/>.
        /// </summary>
        public MuxActivityLoggerHub()
        {
            _clients = new List<IMuxActivityLoggerClient>();
            _clientsEx = new ReadOnlyListOnIList<IMuxActivityLoggerClient>( _clients );
        }

        /// <summary>
        /// Registers an <see cref="IMuxActivityLoggerClient"/> to the <see cref="RegisteredMuxClients"/> list.
        /// Duplicate IActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="l">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IMuxActivityLoggerClientRegistrar RegisterMuxClient( IMuxActivityLoggerClient client )
        {
            if( !_clients.Contains( client ) && OnBeforeAdd( client ) ) _clients.Add( client );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IMuxActivityLoggerClient"/> from the <see cref="RegisteredMuxClients"/> list.
        /// Silently ignored unregistered client.
        /// </summary>
        /// <param name="l">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IMuxActivityLoggerClientRegistrar UnregisterMuxClient( IMuxActivityLoggerClient client )
        {
            if( _clients.Remove( client ) ) OnAfterRemoved( client );
            return this;
        }

        /// <summary>
        /// Gets the list of registered <see cref="IMuxActivityLoggerClient"/>.
        /// </summary>
        public IReadOnlyList<IMuxActivityLoggerClient> RegisteredMuxClients
        {
            get { return _clientsEx; }
        }

        /// <summary>
        /// Overriddable method to validate any new client.
        /// </summary>
        /// <param name="client">The client that will be added.</param>
        /// <returns>True to add the client, false to reject it.</returns>
        protected virtual bool OnBeforeAdd( IMuxActivityLoggerClient client )
        {
            return true;
        }

        /// <summary>
        /// Overriddable method to validate any remove of client.
        /// </summary>
        /// <param name="client">The remove client. Can be added back if necessary.</param>
        protected virtual void OnAfterRemoved( IMuxActivityLoggerClient client )
        {
        }

        void IMuxActivityLoggerClient.OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue )
        {
            foreach( var l in RegisteredMuxClients ) l.OnFilterChanged( sender, current, newValue );
        }

        void IMuxActivityLoggerClient.OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text )
        {
            foreach( var l in RegisteredMuxClients ) l.OnUnfilteredLog( sender, level, text );
        }

        void IMuxActivityLoggerClient.OnOpenGroup( IActivityLogger sender, IActivityLogGroup group )
        {
            foreach( var l in RegisteredMuxClients ) l.OnOpenGroup( sender, group );
        }

        string IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            foreach( var l in RegisteredMuxClients ) conclusion = l.OnGroupClosing( sender, group, conclusion ) ?? conclusion;
            return conclusion;
        }

        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            foreach( var l in RegisteredMuxClients ) l.OnGroupClosed( sender, group, conclusion );
        }

    }

}
