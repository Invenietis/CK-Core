#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\MarshallByRefPlayground.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{
    public class MarshalByRefObjectLifetimeController<T> : MarshalByRefObject, IDisposable, ISponsor
        where T : class
    {
        T _refObject;

        public MarshalByRefObjectLifetimeController( T refObject )
        {
            if( refObject == null ) throw new ArgumentNullException( "refObject" );
            if( !(refObject is MarshalByRefObject) ) throw new ArgumentException( R.MustBeAMarshalByRefObject );
            ILease lease = RemotingServices.GetLifetimeService( (MarshalByRefObject)(object)(_refObject = refObject) ) as ILease;
            if( lease != null ) lease.Register( this );
        }

        /// <summary>
        /// Gets the MarshalByRefObject controlled by this wrapper.
        /// </summary>
        public T RefObject
        {
            get 
            {
                var o = _refObject;
                if( o == null ) throw new ObjectDisposedException( "RefObject" );
                return o; 
            }
        }

        /// <summary>
        /// Renews the lease on the instance with its own <see cref="ILease.InitialLeaseTime"/> 
        /// or <see cref="TimeSpan.Zero"/> if this is disposed.
        /// </summary>
        /// <param name="lease">Lease to renew.</param>
        /// <returns>The new time.</returns>
        TimeSpan ISponsor.Renewal( ILease lease )
        {
            return _refObject != null ? lease.InitialLeaseTime : TimeSpan.Zero;
        }

        public void Dispose()
        {
            T o = _refObject;
            if( o != null && Interlocked.CompareExchange( ref _refObject, null, o ) == o )
            {
                ILease lease = RemotingServices.GetLifetimeService( (MarshalByRefObject)(object)o ) as ILease;
                if( lease != null ) lease.Unregister( this );
            }
        }
    }

    class TheServer : MarshalByRefObject
    {
        TimeSpan _initialLeaseTime;
        
        static public int LeaseCreationCount;

        public TheServer( TimeSpan initialLeaseTime )
        {
            _initialLeaseTime = initialLeaseTime;
        }

        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            if( lease.CurrentState == LeaseState.Initial )
            {
                Console.WriteLine( "Lease created (AppDomain={0}) for Server {1}.", AppDomain.CurrentDomain.FriendlyName, GetHashCode() );
                ++LeaseCreationCount;
                lease.InitialLeaseTime = _initialLeaseTime;
                lease.RenewOnCallTime = TimeSpan.FromMilliseconds( 10 );
                lease.SponsorshipTimeout = TimeSpan.FromMilliseconds( 10 );
            }
            return lease;
        }

        internal void ClientSays( string message )
        {
            Console.WriteLine( "Server {0} received: {1}", GetHashCode(), message );
        }
        
    }

    class TheClient : MarshalByRefObject
    {
        TheServer _s;

        public TheClient( TheServer s )
        {
            _s = s;
        }

        public void CallServer( string message )
        {
            _s.ClientSays( message );
        }
    }

    class AppDomainSource : MarshalByRefObject
    {
        public readonly WeakRef<TheServer> Server = new WeakRef<TheServer>( null );

        public TimeSpan InitialLeaseTimeForCreatedServer { get; set; }

        public TheServer CreateServer()
        {
            var s = new TheServer( InitialLeaseTimeForCreatedServer );
            Server.Target = s;
            return s;
        }

    }

    [TestFixtureAttribute]
    public class MarshallByRefPlayground
    {

        [Test]
        public void MultiDomain()
        {
            TestHelper.SetRemotingLeaseManagerVeryShortPollTime();
            TheServer.LeaseCreationCount = 0;

            var origin = new AppDomainSource() { InitialLeaseTimeForCreatedServer = TimeSpan.FromMilliseconds( 100 ) };
            CreateAndUnloadTwoDomainsWithTwoServers( origin );
            Assert.That( origin.Server.IsAlive, Is.True );
            
            Assert.That( TheServer.LeaseCreationCount, Is.EqualTo( 4 ), "Server created 2 ILease objects for 2 AppDomains." );

            Thread.Sleep( 150 );
            TestHelper.ForceGCFullCollect();
            Assert.That( origin.Server.IsAlive, Is.False, "Server has been GCed." );
        }

        private static void CreateAndUnloadTwoDomainsWithTwoServers( AppDomainSource origin )
        {
            AppDomainSetup setup = new AppDomainSetup() 
            { 
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath
            };

            var ad1 = AppDomain.CreateDomain( "Child1", null, setup );
            ad1.SetData( "external-AppDomainSource", origin );
            ad1.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateServer ) );
            Assert.That( origin.Server.IsAlive );
            ad1.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateServer ) );
            Assert.That( origin.Server.IsAlive );

            var ad2 = AppDomain.CreateDomain( "Child2", null, setup );
            ad2.SetData( "external-AppDomainSource", origin );
            ad2.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateServer ) );
            Assert.That( origin.Server.IsAlive );
            ad2.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateServer ) );
            Assert.That( origin.Server.IsAlive );

            AppDomain.Unload( ad2 );
            AppDomain.Unload( ad1 );
        }


        [Test]
        [Explicit]
        public void SimpleWeakReferenceTest()
        {
            // Demonstrates that triggerring a GC.Collect works as it should.
            {
                WeakReference o;
                {
                    var so = new object();
                    o = new WeakReference( so );
                    Assert.That( o.IsAlive );
                    so = null;
                }
                TestHelper.ForceGCFullCollect();
                Assert.That( o.IsAlive, Is.False, "GC just works..." );
            }
            // A MarshalByRefObject without proxies is an object like the others. 
            {
                var origin = new AppDomainSource() { InitialLeaseTimeForCreatedServer = TimeSpan.FromMinutes( 1 ) };
                var s = origin.CreateServer();
                Assert.That( origin.Server.IsAlive );
                s = null;
                TestHelper.ForceGCFullCollect();
                Assert.That( origin.Server.IsAlive, Is.False, "A MBR is an object like the others (when there is no proxies)." );
            }
            TestHelper.SetRemotingLeaseManagerVeryShortPollTime();
            // A MarshalByRefObject whose proxies are GCed can be GCed. 
            {
                var origin = new AppDomainSource() { InitialLeaseTimeForCreatedServer = TimeSpan.FromMilliseconds( 100 ) };
                AppDomainSetup setup = new AppDomainSetup()
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath
                };
                var appDomain = AppDomain.CreateDomain( "ClientAppDomain", null, setup );
                appDomain.SetData( "external-AppDomainSource", origin );
                appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateServer ) );
                Assert.That( _serverInClientAppDomain, Is.Null, "The not null static _serverInClientAppDomain is in the Client AppDomain, here in the Server's one, it is null." );
                Assert.That( origin.Server.IsAlive );

                TestHelper.ForceGCFullCollect();
                Assert.That( origin.Server.IsAlive, "The static _serverInClientAppDomain still references it." );

                // Without client sponsorship.
                {
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateServer ) );
                    Assert.That( _serverInClientAppDomain, Is.Null, "The not null static _serverInClientAppDomain is in the Client AppDomain, here in the Server's one, it is null." );
                    Assert.That( origin.Server.IsAlive );
                    // Creates a client and registers it on TheServer.
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateClientObject ) );
                    // And tell the Client to call TheServer: this call must be successful since the Server is still here.
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainClientCallsServer ) );

                    // Sleeping twice the InitialLeaseTime for TheServer: the server proxy must not be available anymore.
                    Thread.Sleep( (int)origin.InitialLeaseTimeForCreatedServer.TotalMilliseconds * 2 );
                    Assert.Throws<RemotingException>( () => appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainClientCallsServer ) ) );
                    
                    // Server is dead.
                    TestHelper.ForceGCFullCollect();
                    GC.WaitForPendingFinalizers();
                    Assert.That( origin.Server.IsAlive, Is.False, "No more strong references to the proxy." );

                    // We free the non secure references to TheServer and TheClient: only the secured client is now alive in Client app domain.
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainFreeClientAndServerRef ) );
                }

                // With client sponsorship: The client drives the lifetime of the server proxy.
                {
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateSecuredServer ) );
                    // Creates a client and registers it on TheServer.
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainCreateClientObject ) );
                    // And tell the Client to call TheServer: this call must be successful since the Server is still here.
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainClientCallsServer ) );

                    // Sleeping twice the InitialLeaseTime for TheServer: the server proxy is still available.
                    Thread.Sleep( (int)origin.InitialLeaseTimeForCreatedServer.TotalMilliseconds * 2 );
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainClientCallsServer ) );
                    
                    // Server is NOT dead.
                    TestHelper.ForceGCFullCollect();
                    GC.WaitForPendingFinalizers();
                    Assert.That( origin.Server.IsAlive );
                    
                    // Just to be sure:
                    Thread.Sleep( (int)origin.InitialLeaseTimeForCreatedServer.TotalMilliseconds * 2 );
                    Assert.DoesNotThrow( () => appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainClientCallsServer ) ) );
                    
                    // Server is NOT dead.
                    TestHelper.ForceGCFullCollect();
                    GC.WaitForPendingFinalizers();
                    Assert.That( origin.Server.IsAlive );

                    // Now, release the client: this must make the proxy to TheServer no more callable.
                    appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainReleaseSecuredServer ) );
                    Thread.Sleep( (int)origin.InitialLeaseTimeForCreatedServer.TotalMilliseconds * 2 );
                    Assert.Throws<RemotingException>( () => appDomain.DoCallBack( new CrossAppDomainDelegate( ClientAppDomainClientCallsServer ) ) );

                    // Server is dead.
                    TestHelper.ForceGCFullCollect();
                    GC.WaitForPendingFinalizers();
                    Assert.That( origin.Server.IsAlive, Is.False, "No more strong references to the proxy." );
                }
                AppDomain.Unload( appDomain );
            }
        }

        static TheServer _serverInClientAppDomain;
        static TheClient _clientInClientAppDomain;
        static MarshalByRefObjectLifetimeController<TheServer> _securedServerWrapperInClientAppDomain;

        static void ClientAppDomainCreateServer()
        {
            TestHelper.SetRemotingLeaseManagerVeryShortPollTime();
            AppDomainSource origin = (AppDomainSource)AppDomain.CurrentDomain.GetData( "external-AppDomainSource" );
            var s = origin.CreateServer();
            Assert.That( RemotingServices.IsTransparentProxy( s ) );
            _serverInClientAppDomain = s;
            
            origin.Server.Target.ClientSays( String.Format( "From the '{0}'.", Thread.GetDomain().FriendlyName ) );
        }
        
        static void ClientAppDomainCreateSecuredServer()
        {
            TestHelper.SetRemotingLeaseManagerVeryShortPollTime();
            AppDomainSource origin = (AppDomainSource)AppDomain.CurrentDomain.GetData( "external-AppDomainSource" );
            _securedServerWrapperInClientAppDomain = new MarshalByRefObjectLifetimeController<TheServer>( origin.CreateServer() );
            _serverInClientAppDomain = _securedServerWrapperInClientAppDomain.RefObject;
            Assert.That( RemotingServices.IsTransparentProxy( _serverInClientAppDomain ) );
        }

        static void ClientAppDomainReleaseSecuredServer()
        {
            _securedServerWrapperInClientAppDomain.Dispose();
            Assert.Throws<ObjectDisposedException>( () => { var o = _securedServerWrapperInClientAppDomain.RefObject; } );
            _securedServerWrapperInClientAppDomain = null;
        }

        static void ClientAppDomainFreeClientAndServerRef()
        {
            Assert.That( _serverInClientAppDomain, Is.Not.Null );
            _serverInClientAppDomain = null;
            _clientInClientAppDomain = null;
        }

        static void ClientAppDomainCreateClientObject()
        {
            _clientInClientAppDomain = new TheClient( _serverInClientAppDomain );
        }

        static void ClientAppDomainClientCallsServer()
        {
            Assert.That( _clientInClientAppDomain, Is.Not.Null );
            _clientInClientAppDomain.CallServer( "Hello" );
        }

    }
}
