#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorOutput.cs) is part of CiviKey. 
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
using System.Threading;

namespace CK.Core.Impl
{
    /// <summary>
    /// Implementation of <see cref="IActivityMonitorOutput"/> for <see cref="IActivityMonitor.Output"/>.
    /// </summary>
    public class ActivityMonitorOutput : IActivityMonitorOutput
    {
        readonly IActivityMonitorImpl _monitor;
        readonly ActivityMonitorBridgeTarget _externalInput;
        IActivityMonitorClient[] _clients;

        //static object _clientLockTemp = new object();

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorOutput"/> bound to a <see cref="IActivityMonitor"/>.
        /// </summary>
        /// <param name="monitor"></param>
        public ActivityMonitorOutput( IActivityMonitorImpl monitor )
        {
            if( monitor == null ) throw new ArgumentNullException();
            _monitor = monitor;
            _clients = Util.EmptyArray<IActivityMonitorClient>.Empty; 
            _externalInput = new ActivityMonitorBridgeTarget( monitor, true );
        }

        /// <summary>
        /// Gets an entry point for other monitors: by registering <see cref="ActivityMonitorBridge"/> in other <see cref="IActivityMonitor.Output"/>
        /// bound to this <see cref="ActivityMonitorBridgeTarget"/>, log streams can easily be merged.
        /// </summary>
        public ActivityMonitorBridgeTarget ExternalInput
        {
            get { return _externalInput; }
        }

        /// <summary>
        /// Gets the associated <see cref="IActivityMonitor"/>.
        /// </summary>
        protected IActivityMonitorImpl Monitor { get { return _monitor; } }

        /// <summary>
        /// Registers an <see cref="IActivityMonitorClient"/> to the <see cref="Clients"/> list.
        /// Duplicate IActivityMonitorClient are silently ignored.
        /// </summary>
        /// <param name="client">An <see cref="IActivityMonitorClient"/> implementation.</param>
        /// <returns>The registered client.</returns>
        public IActivityMonitorClient RegisterClient( IActivityMonitorClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            //lock( _clientLockTemp )
            //{
            //    Console.WriteLine( "Thread={2}, RegisterClient client '{0}'. idx={1}.", client, Array.IndexOf( _clients, client ), Thread.CurrentThread.ManagedThreadId );
            //    if( Array.IndexOf( _clients, client ) < 0 )
            //    {
            //        IActivityMonitorBoundClient bound = client as IActivityMonitorBoundClient;
            //        if( bound != null ) bound.SetMonitor( _monitor, false );
            //    }
            //    _clients = new CKReadOnlyListMono<IActivityMonitorClient>( client ).Concat( _clients ).ToArray();
            //}

            if( Array.IndexOf( _clients, client ) < 0 )
            {
                // Has the same Client instance a chance to be registered at the same time by two threads?
                //
                // For non-bound client may be... But here we are talking of a bound client that is "associated" 
                // to one monitor. Since multithreading in this context is quite impossible, we consider that
                // protecting SetMonitor call here is useless.
                // (Other option: create a _setMonitorLock and serialize all calls to all SetMonitor in the AppDomain.)
                IActivityMonitorBoundClient bound = client as IActivityMonitorBoundClient;
                if( bound != null ) bound.SetMonitor( _monitor, false );
                Util.InterlockedPrepend( ref _clients, client );
            }
            return client;
        }

        /// <summary>
        /// Registers a typed <see cref="IActivityMonitorClient"/>.
        /// </summary>
        /// <typeparam name="T">Any type that specializes <see cref="IActivityMonitorClient"/>.</typeparam>
        /// <param name="this">This <see cref="IActivityMonitorOutput"/> object.</param>
        /// <param name="client">Multiple clients to register.</param>
        /// <returns>The registered client.</returns>
        public T RegisterClient<T>( T client ) where T : IActivityMonitorClient
        {
            return (T)RegisterClient( (IActivityMonitorClient)client );
        }

        /// <summary>
        /// Enables atomic registration of a <see cref="IActivityMonitorClient"/> that must be unique in a sense.
        /// </summary>
        /// <param name="tester">Predicate that must be satisfied for at least one registered client.</param>
        /// <param name="factory">Factory that will be called if no existing client satisfies <paramref name="tester"/>.</param>
        /// <returns>This object to enable fluent syntax.</returns>
        /// <remarks>
        /// The factory function MUST return a client that satisfies the tester function otherwise a <see cref="InvalidOperationException"/> is thrown.
        /// </remarks>
        public T AtomicRegisterClient<T>( Func<T, bool> tester, Func<T> factory ) where T : IActivityMonitorClient
        {
            Func<T> reg = () => 
            { 
                var c = factory();
                IActivityMonitorBoundClient bound = c as IActivityMonitorBoundClient;
                if( bound != null ) bound.SetMonitor( _monitor, false );
                return c; 
            };

            //lock( _clientLockTemp )
            //{
            //    T e = _clients.OfType<T>().FirstOrDefault( tester );
            //    if( e == null )
            //    {
            //        e = reg();
            //        Console.WriteLine( "Thread={2}, AtomicRegisterClient client '{0}'. idx={1}.", e, Array.IndexOf( _clients, e ), Thread.CurrentThread.ManagedThreadId );
            //        _clients = new CKReadOnlyListMono<IActivityMonitorClient>( e ).Concat( _clients ).ToArray();
            //    }
            //    return e;
            //}
            
            return (T)Util.InterlockedAdd( ref _clients, tester,  reg, true )[0];
        }

        /// <summary>
        /// Unregisters the given <see cref="IActivityMonitorClient"/> from the <see cref="Clients"/> list.
        /// Silently ignores unregistered client.
        /// </summary>
        /// <param name="client">An <see cref="IActivityMonitorClient"/> implementation.</param>
        /// <returns>The unregistered client or null if it has not been found.</returns>
        public IActivityMonitorClient UnregisterClient( IActivityMonitorClient client )
        {
            if( client == null ) throw new ArgumentNullException( "client" );
            //lock( _clientLockTemp )
            //{
            //    Console.WriteLine( "Thread={2}, Unregistering client '{0}'. idx={1}.", client, Array.IndexOf( _clients, client ), Thread.CurrentThread.ManagedThreadId );
            //    if( Array.IndexOf( _clients, client ) >= 0 )
            //    {
            //        IActivityMonitorBoundClient bound = client as IActivityMonitorBoundClient;
            //        if( bound != null ) bound.SetMonitor( null, false );

            //        var cc = _clients.ToList();
            //        cc.Remove( client );
            //        _clients = cc.ToArray();
            //        return client;
            //    }
            //    return null;
            //}

            if( Array.IndexOf( _clients, client ) >= 0 )
            {
                IActivityMonitorBoundClient bound = client as IActivityMonitorBoundClient;
                if( bound != null ) bound.SetMonitor( null, false );

                Util.InterlockedRemove( ref _clients, client );
                return client;
            }
            return null;
        }

        internal void ForceRemoveBuggyClient( IActivityMonitorClient client )
        {
            Debug.Assert( client != null && _clients.Contains( client ) );
            IActivityMonitorBoundClient bound = client as IActivityMonitorBoundClient;
            if( bound != null )
            {
                try
                {
                    bound.SetMonitor( null, true );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.LoggingError.Add( ex, "While removing the buggy client." );
                }
            }
            Util.InterlockedRemove( ref _clients, client );
        }

        /// <summary>
        /// Gets the list of registered <see cref="IActivityMonitorClient"/>.
        /// </summary>
        public IReadOnlyList<IActivityMonitorClient> Clients
        {
            get { return _clients.AsReadOnlyList(); }
        }

    }
}
