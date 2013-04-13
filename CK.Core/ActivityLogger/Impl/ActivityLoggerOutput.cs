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
    public class ActivityLoggerOutput : IActivityLoggerOutput, IActivityLoggerClient
    {
        List<IActivityLoggerClient> _clients;
        IReadOnlyList<IActivityLoggerClient> _clientsEx;
        List<IActivityLoggerClient> _nonRemoveableClients;

        internal class EmptyOutput : IActivityLoggerOutput
        {
            public IActivityLoggerClient ExternalInput
            {
                get { return ActivityLoggerClient.Empty; }
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
                get { return CKReadOnlyListEmpty<IActivityLoggerClient>.Empty; }
            }

            public IList<IActivityLoggerClient> NonRemoveableClients 
            {
                get { return (IList<IActivityLoggerClient>)CKReadOnlyListEmpty<IActivityLoggerClient>.Empty; } 
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
            _clientsEx = new CKReadOnlyListOnIList<IActivityLoggerClient>( _clients );
            _nonRemoveableClients = new List<IActivityLoggerClient>();
        }

        /// <summary>
        /// Gets an entry point for other loggers: by registering this <see cref="IActivityLoggerClient"/> in other <see cref="IActivityLogger.Output"/>,
        /// log data easily be merged.
        /// </summary>
        public IActivityLoggerClient ExternalInput 
        {
            get { return this; } 
        }

        /// <summary>
        /// Gets a modifiable list of <see cref="IActivityLoggerClient"/> that can not be unregistered.
        /// </summary>
        /// <para>
        /// This list simply guaranties that an <see cref="InvalidOperationException"/> will be thrown 
        /// if a call to <see cref="IActivityLoggerClientRegistrar.UnregisterClient"/> is done on a non removeable client.
        /// </para>
        /// </remarks>
        public IList<IActivityLoggerClient> NonRemoveableClients 
        { 
            get { return _nonRemoveableClients; } 
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
            if( client == null ) throw new ArgumentNullException( "client" );
            if( !_clients.Contains( client ) )
            {
                _clients.Insert( 0, client );
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
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerClient> RegisteredClients
        {
            get { return _clientsEx; }
        }

        void IActivityLoggerClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            foreach( var l in _clients ) l.OnFilterChanged( current, newValue );
        }

        void IActivityLoggerClient.OnUnfilteredLog( LogLevel level, string text )
        {
            foreach( var l in _clients ) l.OnUnfilteredLog( level, text );
        }

        void IActivityLoggerClient.OnOpenGroup( IActivityLogGroup group )
        {
            foreach( var l in _clients ) l.OnOpenGroup( group );
        }

        void IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            foreach( var l in _clients ) l.OnGroupClosing( group, conclusions );
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            foreach( var l in _clients ) l.OnGroupClosed( group, conclusions );
        }
    }
}
