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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Implementation of <see cref="IActivityLoggerOutput"/> for <see cref="IActivityLogger.Output"/>.
    /// </summary>
    public class ActivityLoggerOutput : IActivityLoggerOutput
    {
        readonly List<IActivityLoggerClient> _clients;
        readonly IReadOnlyList<IActivityLoggerClient> _clientsEx;
        readonly ActivityLoggerBridgeTarget _externalInput;

        internal class EmptyOutput : IActivityLoggerOutput
        {
            ActivityLoggerBridgeTarget _empty = new ActivityLoggerBridgeTarget();

            public IActivityLoggerClient ExternalInput
            {
                get { return ActivityLoggerClient.Empty; }
            }

            public IActivityLoggerOutput RegisterClient( IActivityLoggerClient client )
            {
                return this;
            }

            public IActivityLoggerOutput UnregisterClient( IActivityLoggerClient client )
            {
                return this;
            }

            public IReadOnlyList<IActivityLoggerClient> RegisteredClients
            {
                get { return CKReadOnlyListEmpty<IActivityLoggerClient>.Empty; }
            }

            ActivityLoggerBridgeTarget IActivityLoggerOutput.ExternalInput
            {
                get { throw new NotImplementedException(); }
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
            _clients = new List<IActivityLoggerClient>();
            _clientsEx = new CKReadOnlyListOnIList<IActivityLoggerClient>( _clients );
            _externalInput = new ActivityLoggerBridgeTarget( logger, true );
        }

        /// <summary>
        /// Gets an entry point for other loggers: by registering <see cref="ActivityLoggerBridge"/> in other <see cref="IActivityLogger.Output"/>
        /// bound to this <see cref="ActivityLoggerBridgeTarget"/>, log streams can easily be merged.
        /// </summary>
        public ActivityLoggerBridgeTarget ExternalInput 
        {
            get { return _externalInput; } 
        }

        /// <summary>
        /// Gets the associated <see cref="IActivityLogger"/>.
        /// </summary>
        protected IActivityLogger Logger { get { return _externalInput.FinalLogger; } }

        /// <summary>
        /// Registers an <see cref="IActivityLoggerClient"/> to the <see cref="RegisteredClients"/> list.
        /// Duplicate IActivityLoggerClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IActivityLoggerOutput RegisterClient( IActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            if( !_clients.Contains( client ) )
            {
                IActivityLoggerBoundClient bound = client as IActivityLoggerBoundClient;
                if( bound != null ) bound.SetLogger( Logger, false );
                _clients.Insert( 0, client );
            }
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerClient"/> from the <see cref="RegisteredClients"/> list.
        /// Silently ignores unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IActivityLoggerClient"/> implementation.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        public IActivityLoggerOutput UnregisterClient( IActivityLoggerClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            int idx = _clients.IndexOf( client );
            if( idx >= 0 )
            {
                IActivityLoggerBoundClient bound = client as IActivityLoggerBoundClient;
                if( bound != null ) bound.SetLogger( null, false );
                _clients.RemoveAt( idx );
            }
            return this;
        }

        internal void ForceRemoveBuggyClient( IActivityLoggerClient client )
        {
            Debug.Assert( client != null && _clients.Contains( client ) );
            if( client == null ) throw new ArgumentNullException( "client" );
            IActivityLoggerBoundClient bound = client as IActivityLoggerBoundClient;
            if( bound != null )
            {
                try
                {
                    bound.SetLogger( null, true );
                }
                catch( Exception ex )
                {
                    ActivityLogger.LoggingError.Add( ex, "While removing the buggy client." );
                }
            }
            _clients.Remove( client );
        }



        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerClient> RegisteredClients
        {
            get { return _clientsEx; }
        }
    }
}
