#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerOutput.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        List<IActivityLoggerClientBase> _nonRemoveableClients;

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

            public IList<IActivityLoggerClientBase> NonRemoveableClients 
            {
                get { return (IList<IActivityLoggerClientBase>)ReadOnlyListEmpty<IActivityLoggerClientBase>.Empty; } 
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
            _nonRemoveableClients = new List<IActivityLoggerClientBase>();
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
        /// Gets a modifiable list of either <see cref="IMuxActivityLoggerClient"/> or <see cref="IActivityLoggerClient"/>
        /// that can not be removed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Already registered hybrid clients (that support both <see cref="IMuxActivityLoggerClient"/> and <see cref="IActivityLoggerClient"/>)
        /// can be added at any time in <see cref="IActivityLoggerClientRegistrar.RegisteredClients"/> or <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients"/>:
        /// they are automatically removed from the other registrar.
        /// </para>
        /// <para>
        /// This behavior (that avoids stuterring: logs sent twice since the same client is registered in both registrar), also applies to clients that are 
        /// registered in this NonRemoveableClients list. This list simply guraranty that an <see cref="InvalidOperationException"/> will be thrown 
        /// if a call to <see cref="IActivityLoggerClientRegistrar.UnregisterClient"/> or <see cref="IMuxActivityLoggerClientRegistrar.UnregisterMuxClient"/> is 
        /// done on a non removeable client.
        /// </para>
        /// </remarks>
        public IList<IActivityLoggerClientBase> NonRemoveableClients 
        { 
            get { return _nonRemoveableClients; } 
        }

        /// <summary>
        /// Gets the associated <see cref="IActivityLogger"/>.
        /// </summary>
        protected IActivityLogger Logger { get; private set; }

        /// <summary>
        /// Registers an <see cref="IActivityLoggerClient"/> to the <see cref="RegisteredClients"/> list.
        /// Removes the <paramref name="client"/> from <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients">RegisteredMuxClients</see> if
        /// it is also a <see cref="IMuxActivityLoggerClient"/> to avoid stuttering.
        /// Duplicate IActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IActivityLoggerClientRegistrar RegisterClient( IActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            if( !_clients.Contains( client ) )
            {
                IMuxActivityLoggerClient mux = client as IMuxActivityLoggerClient;
                if( mux != null ) DoRemove( mux );
                _clients.Insert( 0, client );
            }
            return this;
        }

        /// <summary>
        /// Registers an <see cref="IMuxActivityLoggerClient"/> to the <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients">RegisteredMuxClients</see> list.
        /// Removes the <paramref name="client"/> from <see cref="RegisteredClients"/> if
        /// it is also a <see cref="IActivityLoggerClient"/> to avoid stuttering.
        /// Duplicate IMuxActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public override IMuxActivityLoggerClientRegistrar RegisterMuxClient( IMuxActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            if( !RegisteredMuxClients.Contains( client ) )
            {
                IActivityLoggerClient c = client as IActivityLoggerClient;
                if( c != null ) _clients.Remove( c );
                DoAdd( client );
            }
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerClient"/> from the <see cref="RegisteredClients"/> list.
        /// Silently ignores unregistered client but throws an <see cref="InvalidOperationException"/> if it belongs to <see cref="NonRemoveableClients"/> list.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IActivityLoggerClientRegistrar UnregisterClient( IActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            if( _nonRemoveableClients.Contains( client ) ) throw new InvalidOperationException( R.ActivityLoggerNonRemoveableClient );
            _clients.Remove( client );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IMuxActivityLoggerClient"/> from the <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients">RegisteredMuxClients</see> list.
        /// Silently ignores unregistered client but throws an <see cref="InvalidOperationException"/> if it belongs to <see cref="NonRemoveableClients"/> list.
        /// </summary>
        /// <param name="client">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public override IMuxActivityLoggerClientRegistrar UnregisterMuxClient( IMuxActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            if( _nonRemoveableClients.Contains( client ) ) throw new InvalidOperationException( R.ActivityLoggerNonRemoveableClient );
            DoRemove( client );
            return this;
        }

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerClient> RegisteredClients
        {
            get { return _clientsEx; }
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

        internal void OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            foreach( var l in _clients ) l.OnGroupClosing( group, conclusions );
            ((IMuxActivityLoggerClient)this).OnGroupClosing( Logger, group, conclusions );
        }

        internal void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            foreach( var l in _clients ) l.OnGroupClosed( group, conclusions );
            ((IMuxActivityLoggerClient)this).OnGroupClosed( Logger, group, conclusions );
        }
    }
}
