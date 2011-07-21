using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using CK.Storage;
using CK.Plugin;

namespace CK.Context.Tests
{
    [TestFixture]
    public class ContextExceptionProtection
    {
        bool _onExitOKCalled;
        bool _onExitKOCalled;
        bool _exceptionLogged;

        [Test]
        public void TestExceptions()
        {
            StandardContextHost host = new StandardContextHost( "TestContexts", null );
            
            IContext c = host.CreateContext();

            c.LogCenter.EventCreated += LogEventListener;
            c.ApplicationExited += OnExitOK;

            c.RaiseExitApplication( false );
            Assert.That( _onExitOKCalled && !_onExitKOCalled, "Just to test that events work." );

            _onExitOKCalled = false;
            c.ApplicationExited += OnExitKO;
            c.RaiseExitApplication( false );
            Assert.That( _onExitOKCalled && _onExitKOCalled && _exceptionLogged, "The exception has been caught." );

            // Removes the handler and adds it back so that it appears after the buggy one.
            c.ApplicationExited -= OnExitOK;
            c.ApplicationExited += OnExitOK;
            _onExitOKCalled = _onExitKOCalled = _exceptionLogged = false;
            c.RaiseExitApplication( false );
            Assert.That( _onExitOKCalled && _onExitKOCalled && _exceptionLogged, "The exception has been caught, the remaining subscribers have been called." );
        
        }

        void OnExitOK( object sender, EventArgs e )
        {
            _onExitOKCalled = true;
        }

        void OnExitKO( object sender, EventArgs e )
        {
            _onExitKOCalled = true;
            throw new Exception( "Buggy handler..." );
        }

        void LogEventListener( object sender, LogEventArgs e )
        {
            if( e.EntryType == LogEntryType.EventError )
            {
                ILogEventError eE = (ILogEventError)e;
                Assert.That( eE.Error.Message, Is.EqualTo( "Buggy handler..." ) );
                _exceptionLogged = true;
            }
        }
    }
}