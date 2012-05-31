#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\ConsumerPlugin.cs) is part of CiviKey. 
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

namespace CK.Plugin.Host.Tests
{

    public class BugException : Exception
    {
        public BugException()
            : base( "Bug in plugin" )
        {
        }
    }

	class ConsumerPlugin : IPlugin
	{
        TestContext _c;

        public ConsumerPlugin( TestContext c )
        {
            _c = c;
        }

        #region IPlugin Members

        bool IPlugin.Setup( IPluginSetupInfo info )
        {
            return true;
        }

        void IPlugin.Start()
        {
        }

        void IPlugin.Teardown()
        {
        }

        void IPlugin.Stop()
        {
        }

        #endregion

        IChoucrouteService CS { get { return _c.Service.Service; } }

		bool _anEventFired;
		bool _anEventGenFired;

		public void NormalRun()
		{
            Assert.That( _c.Service.Status == RunningStatus.Started, "This method must be run whith a running service." );

            CS.CallFunc();
			Assert.That( CS.Div( 30, 2 ) == 15 );
			Assert.That( CS.Div( 100, 10, 2 ) == 5 );
			Assert.That( CS[0] != null && CS[1] == null );
			Assert.That( CS.Creation > DateTime.MinValue );
			Guid tG = Guid.NewGuid();
			CS.ID = tG;
			Assert.That( CS.ID == tG );
			Assert.That( CS[33, DateTime.MinValue, CS, "a", this, 93, 12].Length > 0 );
			CS[33, DateTime.Now, null, "a", this, 93, 12] = "Not Stored...";

			// Raise AnEvent without any listener to AnEvent.
			CS.RaiseAnEvent();

			_anEventFired = false;
			CS.AnEvent += CS_AnEvent;
			CS.RaiseAnEvent();
			Assert.That( _anEventFired );
			
			_anEventFired = false;
			CS.AnEvent -= CS_AnEvent;
			CS.RaiseAnEvent();
			Assert.That( !_anEventFired );
			
			_anEventGenFired = false;
			CS.AnEventGen += CS_AnEventGen;
			CS.RaiseAnEventGen();
			Assert.That( _anEventGenFired );
			CS.AnEventGen -= CS_AnEventGen;
			
			CS.GenericFunc( 23 );
			CS.GenericFunc( "kilo" );
			Assert.That( CS.GenericFunc( "kilo", 3, CS ) == 3 );
			Assert.That( CS.GenericFunc( CS, 12, "kilo" ) == 12 );
		}

        public void RunWhileNotAvailableService()
        {
            Assert.That( _c.Service.Status == RunningStatus.Disabled, "This method must be run with a disabled service or non existing plugin. Behavior must be exactly the same" );

            // This one has IgnoreServiceRunningStatus
            Assert.Throws<ServiceNotAvailableException>( delegate() { CS.Div( 300, 10 ); } );

            Assert.Throws<ServiceNotAvailableException>( delegate() { CS.Div( 300, 10, 2 ); } );

            Assert.Throws<ServiceNotAvailableException>( delegate() { CS.AnyMethodCalled += ( o, e ) => Assert.Fail( "Never here..." ); } );
            
            Assert.Throws<ServiceNotAvailableException>( delegate() { CS.AnEventGen += ( o, e ) => Assert.Fail( "Never here..." ); } );

            Assert.Throws<ServiceNotAvailableException>( delegate() { CS.RaiseAnEvent(); } );

        }

		public void RunWhileStoppedService()
		{
            Assert.That( _c.Service.Status == RunningStatus.Stopped, "This method must be run with a stopped service." );

			Assert.Throws<ServiceStoppedException>( delegate(){ CS.CallFunc(); } );

			// IgnoreServiceRunningStatus
			Assert.That( CS.Div( 300, 10 ) == 30 );

            Assert.Throws<ServiceStoppedException>( delegate() { CS.Div( 300, 10, 2 ); } );
			
			// Setter on ID IgnoreServiceRunningStatus
			Guid tG = Guid.NewGuid();
			CS.ID = tG;
			// But not on the getter!
            Assert.Throws<ServiceStoppedException>( delegate() { Guid g = CS.ID; } );
			
			// Creation property has IgnoreServiceRunningStatus
			Assert.That( CS.Creation > DateTime.MinValue );

            Assert.Throws<ServiceStoppedException>( delegate() { object o = CS[0]; } );
            Assert.Throws<ServiceStoppedException>( delegate() { string o = CS[33, DateTime.MinValue, CS, "a", this, 93, 12]; } );
            Assert.Throws<ServiceStoppedException>( delegate() { CS[33, DateTime.Now, null, "a", this, 93, 12] = "Not Stored..."; } );
            Assert.Throws<ServiceStoppedException>( delegate() { CS.GenericFunc( 23 ); } );
            Assert.Throws<ServiceStoppedException>( delegate() { CS.GenericFunc( "kilo" ); } );
            Assert.Throws<ServiceStoppedException>( delegate() { int i = CS.GenericFunc( "kilo", 3, CS ); } );

			// We have not registered any listener to AnEvent: it will not be fired.
			CS.RaiseAnEvent();

			CS.AnEvent += CS_AnEventNotRunning;

            // Here, we MUST have an exception because even if RaiseAnEvent is [IgnoreServiceRunningStatus], the AnEvent event itself is NOT:
            // the plugin is not allowed to raise this event since the service is not started.
            Assert.Throws<ServiceStoppedException>( delegate() { CS.RaiseAnEvent(); } );

			CS.AnEvent -= CS_AnEventNotRunning;

		}

        public void RunAnEventBuggyHandlerWithNoProtection()
        {
            Assert.That( _c.Service.Status == RunningStatus.Started, "This method must be run whith a running service." );

            _anEventFired = false;
            CS.AnEvent += CS_AnEvent;
            CS.AnEvent += CS_AnEventBuggyHandler;
            Assert.Throws<BugException>( () => CS.RaiseAnEvent() );
            Assert.That( _anEventFired, "First event has been dispatched." );

            // Invert subscription to event.
            _anEventFired = false;
            CS.AnEvent -= CS_AnEvent;
            CS.AnEvent += CS_AnEvent;

            Assert.Throws<BugException>( () => CS.RaiseAnEvent() );
            Assert.That( !_anEventFired, "The first dispatching thrown the exception." );
        }

        public void RunAnEventProtectedBuggyHandler()
        {
            Assert.That( _c.Service.Status == RunningStatus.Started, "This method must be run whith a running service." );
            Assert.That( (_c.GetEventLogOptions( "AnEvent") & ServiceLogEventOptions.SilentEventError) != 0, "Dispatch protection is on." );

            _anEventFired = false;
            CS.AnEvent += CS_AnEvent;
            CS.AnEvent += CS_AnEventBuggyHandler;
            CS.RaiseAnEvent();
            Assert.That( _anEventFired, "First event has been dispatched." );

            // Invert subscription to event.
            _anEventFired = false;
            CS.AnEvent -= CS_AnEvent;
            CS.AnEvent += CS_AnEvent;
            CS.RaiseAnEvent();
            Assert.That( _anEventFired, "Even if the first dispatch thrown the exception, the second one has been called." );

        }

        public void RunDivideByZero()
        {
            CS.Div( 20, 0 );
        }

        void CS_AnEvent( int i, bool b, string s, object source )
		{
			Assert.That( source is ServiceProxyBase, "Source is the Proxy object (event must have been relayed and parameter must have been mapped)." );
			_anEventFired = true;
		}
		
		void CS_AnEventGen( object source, SpecEventArgs args )
		{
			Assert.That( source is ServiceProxyBase, "Source is the Proxy object (event must have been relayed and parameter must have been mapped)." );
			_anEventGenFired = true;
		}
		
		void CS_AnEventNotRunning( int i, bool b, string s, object source )
		{
			Assert.Fail( "We should not be here since a NotRunningException must have been fired before." );
		}

		void CS_AnEventBuggyHandler( int i, bool b, string s, object source )
		{
            throw new BugException();
        }

	}
}
