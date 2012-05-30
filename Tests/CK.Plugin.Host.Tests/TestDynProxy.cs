#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\TestDynProxy.cs) is part of CiviKey. 
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
using NUnit.Framework;
using CK.Plugin;
using CK.Plugin.Hosting;
using System.Reflection;
using System.Reflection.Emit;
using CK.Core;

namespace CK.Plugin.Host.Tests
{

	[TestFixture]
	public class TestDynProxy
	{

		static int _countEvent;

		[Test]
		public void HandMadeProxy()
		{
            TestContext c = new TestContext( true, true );
            TestNormalRun( c );
		}

		[Test]
		public void RealProxy()
		{
            TestContext c = new TestContext( false, true );
            TestNormalRun( c );
        }

        [Test]
        public void HandMadeProxyServiceStoppedException()
        {
            TestContext c = new TestContext( true, false );
            TestRunWhileStoppedService( c );
        }

        [Test]
        public void RealProxyServiceStoppedException()
        {
            TestContext c = new TestContext( false, false );
            TestRunWhileStoppedService( c );
        }
        
        [Test]
        public void HandMadeProxyServiceNotAvailableException()
        {
            TestRunWhileDisabledService( true );
        }

        [Test]
        public void RealProxyServiceNotAvailableException()
        {
            TestRunWhileDisabledService( false );
        }

        [Test]
        public void HandMadeProxyRunAnEventBuggyHandlerWithNoProtection()
        {
            TestContext c = new TestContext( true, true );
            c.SetLogOptions( "AnEvent", ServiceLogEventOptions.None ); 
            c.ConsumerPlugin.RunAnEventBuggyHandlerWithNoProtection();
        }

        [Test]
        public void RealProxyRunAnEventBuggyHandlerWithNoProtection()
        {
            {
                TestContext c = new TestContext( false, true );
                c.SetLogOptions( "AnEvent", ServiceLogEventOptions.None );
                c.ConsumerPlugin.RunAnEventBuggyHandlerWithNoProtection();
            }
            // Same test with CatchExceptionGeneration.Never: since no catch is generated, the behavior
            // is the same as with SilentEventError = false.
            {
                TestContext c = new TestContext( false, true, CatchExceptionGeneration.Never );
                c.SetLogOptions( "AnEvent", ServiceLogEventOptions.SilentEventError );
                c.ConsumerPlugin.RunAnEventBuggyHandlerWithNoProtection();
            }
        }

        [Test]
        public void RealProxyRunWithOrWithoutProtection()
        {
            {
                TestContext c = new TestContext( false, true, CatchExceptionGeneration.Always );
                Assert.Throws<DivideByZeroException>( () => c.ConsumerPlugin.RunDivideByZero() );
            }
            {
                TestContext c = new TestContext( false, true, CatchExceptionGeneration.Never );
                Assert.Throws<DivideByZeroException>( () => c.ConsumerPlugin.RunDivideByZero() );
            }
        }

        [Test]
        public void HandMadeProxyRunAnEventProtectedBuggyHandler()
        {
            TestContext c = new TestContext( true, true );
            TestRunAnEventProtectedBuggyHandler( c );
        }

        [Test]
        public void RealProxyRunAnEventProtectedBuggyHandler()
        {
            TestContext c = new TestContext( false, true );
            TestRunAnEventProtectedBuggyHandler( c );
        }

        [Test]
        public void GetServiceType()
        {
            {
                // Ask for IChoucrouteService and IService<IChoucrouteService> when plugin is running:
                // we obtain both of them.
                TestContext c = new TestContext( false, true );

                object oDirect =  c.IServiceHost.GetProxy( typeof( IChoucrouteService ) );
                Assert.That( oDirect != null
                    && typeof( IChoucrouteService ).IsAssignableFrom( oDirect.GetType() ) );

                object oWrapped =  c.IServiceHost.GetProxy( typeof( IService<IChoucrouteService> ) );
                
                Assert.That( oWrapped != null
                    && typeof( IChoucrouteService ).IsAssignableFrom( oWrapped.GetType() )
                    && typeof( IService<IChoucrouteService> ).IsAssignableFrom( oWrapped.GetType() ) );

                IService<IChoucrouteService> s = (IService<IChoucrouteService>)oWrapped;
                Assert.That( s.Status == RunningStatus.Started );
                Assert.That( s.Service.Div( 12, 4 ) == 3 );
            }
            {
                // Ask for IChoucrouteService and IService<IChoucrouteService> when plugin is NOT running:
                // we obtain the IService<> in an unavailable state and we obtain null for the mere IChoucrouteService.
                TestContext c = new TestContext( false, false );

                object oDirect =  c.IServiceHost.GetProxy( typeof( IChoucrouteService ) );
                Assert.That( oDirect == null );

                object oWrapped =  c.IServiceHost.GetProxy( typeof( IService<IChoucrouteService> ) );
                Assert.That( oWrapped != null
                    && typeof( IChoucrouteService ).IsAssignableFrom( oWrapped.GetType() )
                    && typeof( IService<IChoucrouteService> ).IsAssignableFrom( oWrapped.GetType() ) );
                IService<IChoucrouteService> s = (IService<IChoucrouteService>)oWrapped;
                Assert.That( s.Status == RunningStatus.Disabled );
                
                Assert.Throws<ServiceNotAvailableException>( () => s.Service.Div( 12, 4 ) );
            }

        }

        void TestNormalRun( TestContext c )
        {
            Assert.That( c.RealPlugin.CalledMethodsCount == 0, "No call yet." );

            _countEvent = 0;
            c.Service.Service.AnyMethodCalled += CS_AnyMethodCalled;

            Assert.That( c.RealPlugin.CalledMethodsCount == 1, "We registered the call to AnyMethodCalled add." );
            c.ConsumerPlugin.NormalRun();
            c.Service.Service.AnyMethodCalled -= CS_AnyMethodCalled;

            Assert.That( c.RealPlugin.AllMethodsHaveBeenCalled(), "Methods not called: " + String.Join( ", ", c.RealPlugin.MethodsNotCalled() ) );

            Assert.That( _countEvent == c.RealPlugin.CalledMethodsCount - 2, "We did not count the += (handler was not yet registered) and the -= (handler have been removed) on AnyMethodCalled." );
        }

        private static void TestRunWhileDisabledService( bool handMadeProxy )
        {
            {
                // Test 1: Plugin nerver started.
                TestContext c = new TestContext( handMadeProxy, false );
                Assert.That( c.ServiceProxyBase.Implementation == null, "Since we never started anything, the service is not bound to its plugin." );
                c.ConsumerPlugin.RunWhileNotAvailableService();
            }
            {
                // Test 2: Plugin started and then disabled.
                TestContext c = new TestContext( handMadeProxy, false );
                c.EnsureStoppedService();
                Assert.That( c.Service.Status == RunningStatus.Stopped );
                Assert.That( c.ServiceProxyBase.Implementation != null );
                c.PluginHost.Execute( new[] { TestContext.PluginPluginId }, ReadOnlyListEmpty<IPluginInfo>.Empty, ReadOnlyListEmpty<IPluginInfo>.Empty );
                Assert.That( c.Service.Status == RunningStatus.Disabled );
                c.ConsumerPlugin.RunWhileNotAvailableService();
            }
        }

        void TestRunWhileStoppedService( TestContext c )
        {
            _countEvent = 0;
            c.EnsureStoppedService();
            c.Service.Service.AnyMethodCalled += CS_AnyMethodCalled;

            Assert.That( c.RealPlugin.CalledMethodsCount == 1, "We registered the call to AnyMethodCalled add." );
            c.ConsumerPlugin.RunWhileStoppedService();

            c.Service.Service.AnyMethodCalled -= CS_AnyMethodCalled;
            Assert.That( !c.RealPlugin.AllMethodsHaveBeenCalled() );
            Console.WriteLine( "Methods not called: " + String.Join( ", ", c.RealPlugin.MethodsNotCalled() ) );
        }

        void TestRunAnEventProtectedBuggyHandler( TestContext c )
        {
            c.SetLogOptions( "AnEvent", ServiceLogEventOptions.SilentEventError );
            c.ConsumerPlugin.RunAnEventProtectedBuggyHandler();
        }

        void CS_AnyMethodCalled( object sender, EventArgs e )
		{
			Assert.That( sender is ServiceProxyBase, "Sender is the Proxy object (event must have been relayed and parameter must have been mapped)." );
			++_countEvent;
		}

	}
}
