#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Context.Tests\ContextExceptionProtection.cs) is part of CiviKey. 
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
            TestContextHost host = new TestContextHost("TestContexts");
            
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