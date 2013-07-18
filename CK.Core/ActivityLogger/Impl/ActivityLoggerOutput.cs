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
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// Implementation of <see cref="IActivityLoggerOutput"/> for <see cref="IActivityLogger.Output"/>.
    /// </summary>
    public class ActivityLoggerOutput : IActivityLoggerOutput
    {
#if net40
        internal class OutList<T> : List<T>, IReadOnlyList<T>
        {
            internal OutList()
                : base()
            {
            }

            internal OutList(IEnumerable<T> basedOn)
                : base( basedOn )
            {
            }
        }
        OutList<IActivityLoggerClient> Clients
        {
            get { return _list as OutList<IActivityLoggerClient>; }
        }

        private OutList<T> CreateNewList<T>( List<T> list = null )
        {
            if( list == null ) return new OutList<T>();
            else return new OutList<T>( list );
        }
        OutList<IActivityLoggerClient> _list;
#endif
#if net45
        List<IActivityLoggerClient> Clients
        {
            get { return _list as List<IActivityLoggerClient>; }
        }
        private List<T> CreateNewList<T>( List<T> list )
        {
            return new List<T>( list );
        }
        List<IActivityLoggerClient> _list;
#endif



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
#if net40
            _list = new OutList<IActivityLoggerClient>();
#endif
#if net45
            _list = new List<IActivityLoggerClient>();
#endif
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

            var localClients = Clients;

            if( !localClients.Contains( client ) )
            {
                IActivityLoggerBoundClient bound = client as IActivityLoggerBoundClient;
                if( bound != null ) bound.SetLogger( Logger, false );

                Morph( ref _list, client, ( current, arg ) =>
                {
                    var loggers = CreateNewList<IActivityLoggerClient>( current.ToList() );
                    if( !loggers.Contains( arg ) ) { loggers.Insert( 0, arg ); }
                    else return current;
                    return loggers;
                } );
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

            IList<IActivityLoggerClient> localClients = Clients;

            if( localClients.Contains( client ) )
            {

                IActivityLoggerBoundClient bound = client as IActivityLoggerBoundClient;
                if( bound != null ) bound.SetLogger( null, false );

                Morph( ref _list, client, ( current, arg ) =>
                {
                    var loggers = CreateNewList<IActivityLoggerClient>( current.ToList() );
                    var idx = loggers.IndexOf( arg );
                    if( idx >= 0 ) loggers.RemoveAt( idx );
                    else return current;
                    return loggers;
                } );
            }

            return this;
        }

        internal void ForceRemoveBuggyClient( IActivityLoggerClient client )
        {
            Debug.Assert( client != null && Clients.Contains( client ) );
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

            Morph( ref _list, client, (current, arg) =>
            {
                var loggers = CreateNewList<IActivityLoggerClient>( current.ToList() );
                var idx = loggers.IndexOf( arg );
                if( idx >= 0 ) loggers.RemoveAt( idx );
                else return current;
                return loggers;
            } );
        }

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerClient"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerClient> RegisteredClients
        {
            get { return _list; }
        }

        static void Morph<T, TArg>( ref T target, TArg arg, Func<T, TArg, T> morpher )
            where T : class
        {
            T desiredVal;
            T startVal = target;
            do
            {
                startVal = target;
                desiredVal = morpher( startVal, arg );
            }
            while( Interlocked.CompareExchange( ref target, desiredVal, startVal ) != startVal );
        }
    }
}
