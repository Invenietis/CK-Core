#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\MuxActivityLoggerHub.cs) is part of CiviKey. 
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
        /// Duplicate IMuxActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public virtual IMuxActivityLoggerClientRegistrar RegisterMuxClient( IMuxActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            if( !_clients.Contains( client ) ) DoAdd( client );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IMuxActivityLoggerClient"/> from the <see cref="RegisteredMuxClients"/> list.
        /// Silently ignores unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IMuxActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public virtual IMuxActivityLoggerClientRegistrar UnregisterMuxClient( IMuxActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            DoRemove( client );
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
        /// Directly removes the client from the list.
        /// </summary>
        /// <param name="client">The client to remove.</param>
        /// <returns>True if has been found and removed.</returns>
        protected bool DoRemove( IMuxActivityLoggerClient client )
        {
            return _clients.Remove( client );
        }

        /// <summary>
        /// Directly adds the client into the list. No check is done.
        /// </summary>
        /// <param name="client">The client to add.</param>
        protected void DoAdd( IMuxActivityLoggerClient client )
        {
            _clients.Insert( 0, client );
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

        void IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            foreach( var l in RegisteredMuxClients ) l.OnGroupClosing( sender, group, conclusions );
        }

        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            foreach( var l in RegisteredMuxClients ) l.OnGroupClosed( sender, group, conclusions );
        }

    }

}
