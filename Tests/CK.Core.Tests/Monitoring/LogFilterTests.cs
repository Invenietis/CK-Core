#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\LogFilterTests.cs) is part of CiviKey. 
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
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class LogFilterTests
    {
        [Test]
        public void CombineLevelTests()
        {
            Assert.That( LogFilter.Combine( LogLevelFilter.Error, LogLevelFilter.Fatal ), Is.EqualTo( LogLevelFilter.Error ) );
            Assert.That( LogFilter.Combine( LogLevelFilter.None, LogLevelFilter.Fatal ), Is.EqualTo( LogLevelFilter.Fatal ) );
            Assert.That( LogFilter.Combine( LogLevelFilter.Error, LogLevelFilter.None ), Is.EqualTo( LogLevelFilter.Error ) );
            Assert.That( LogFilter.Combine( LogLevelFilter.None, LogLevelFilter.None ), Is.EqualTo( LogLevelFilter.None ) );
            Assert.That( LogFilter.Combine( LogLevelFilter.Info, LogLevelFilter.Error ), Is.EqualTo( LogLevelFilter.Info ) );
        }

        [Test]
        public void CombineLogTests()
        {
            LogFilter f = new LogFilter( LogLevelFilter.None, LogLevelFilter.Error );
            LogFilter f2 = f.SetGroup( LogLevelFilter.Info );
            Assert.That( f2.Line == LogLevelFilter.Error && f2.Group == LogLevelFilter.Info );
            LogFilter f3 = new LogFilter( LogLevelFilter.Trace, LogLevelFilter.Info );
            LogFilter f4 = f2.Combine( f3 );
            Assert.That( f4.Equals( f3 ) );
            Assert.That( f4 == f3 );
        }

        [Test]
        public void ToStringTests()
        {
            Assert.That( LogFilter.Undefined.ToString(), Is.EqualTo( "Undefined" ) );
            Assert.That( LogFilter.Terse.ToString(), Is.EqualTo( "Terse" ) );
            Assert.That( LogFilter.Off.ToString(), Is.EqualTo( "Off" ) );
            Assert.That( LogFilter.Debug.ToString(), Is.EqualTo( "Debug" ) );
            Assert.That( LogFilter.Invalid.ToString(), Is.EqualTo( "Invalid" ) );
            Assert.That( new LogFilter( LogLevelFilter.Warn, LogLevelFilter.Error ).ToString(), Is.EqualTo( "{Warn,Error}" ) );
        }

        [Test]
        public void ParseTests()
        {
            Assert.That( LogFilter.Parse( "Undefined" ), Is.EqualTo( LogFilter.Undefined ) );
            Assert.That( LogFilter.Parse( "Debug" ), Is.EqualTo( LogFilter.Debug ) );
            Assert.That( LogFilter.Parse( "Verbose" ), Is.EqualTo( LogFilter.Verbose ) );
            Assert.That( LogFilter.Parse( "Monitor" ), Is.EqualTo( LogFilter.Monitor ) );
            Assert.That( LogFilter.Parse( "Terse" ), Is.EqualTo( LogFilter.Terse ) );
            Assert.That( LogFilter.Parse( "Release" ), Is.EqualTo( LogFilter.Release ) );
            Assert.That( LogFilter.Parse( "Off" ), Is.EqualTo( LogFilter.Off ) );

            Assert.That( LogFilter.Parse( "{None,None}" ), Is.EqualTo( LogFilter.Undefined ) );
            Assert.That( LogFilter.Parse( "{Warn,None}" ), Is.EqualTo( new LogFilter( LogLevelFilter.Warn, LogLevelFilter.None ) ) );
            Assert.That( LogFilter.Parse( "{Error,Warn}" ), Is.EqualTo( new LogFilter( LogLevelFilter.Error, LogLevelFilter.Warn ) ) );
            Assert.That( LogFilter.Parse( "{Off,None}" ), Is.EqualTo( new LogFilter( LogLevelFilter.Off, LogLevelFilter.None ) ) );
            Assert.That( LogFilter.Parse( "{Error,Error}" ), Is.EqualTo( LogFilter.Release ) );
            Assert.That( LogFilter.Parse( "{Info,Error}" ), Is.EqualTo( LogFilter.Terse ) );
            Assert.That( LogFilter.Parse( "{Fatal,Invalid}" ), Is.EqualTo( new LogFilter( LogLevelFilter.Fatal, LogLevelFilter.Invalid ) ) );

            Assert.That( LogFilter.Parse( "{ Error , Error }" ), Is.EqualTo( LogFilter.Release ) );
            Assert.That( LogFilter.Parse( "{   Trace    ,    Info   }" ), Is.EqualTo( LogFilter.Verbose ) );

            Assert.Throws<CKException>( () => LogFilter.Parse( " {Error,Error}" ) );
            Assert.Throws<CKException>( () => LogFilter.Parse( "{Error,Error} " ) );
            Assert.Throws<CKException>( () => LogFilter.Parse( "Error,Error}" ) );
            Assert.Throws<CKException>( () => LogFilter.Parse( "{Error,Error" ) );
            Assert.Throws<CKException>( () => LogFilter.Parse( "{Error,,Error}" ) );
            Assert.Throws<CKException>( () => LogFilter.Parse( "{Error,Warn,Trace}" ) );
            Assert.Throws<CKException>( () => LogFilter.Parse( "{}" ) );
        }

        [Test]
        public void SourceLogFilterInternalTests()
        {
            {
                SourceLogFilter f = new SourceLogFilter( LogFilter.Debug, LogFilter.Invalid );
                Assert.That( (short)(f.GroupFilter >> 16) == (short)LogLevelFilter.Trace );
                Assert.That( (short)(f.LineFilter >> 16) == (short)LogLevelFilter.Trace );
                Assert.That( (short)(f.GroupFilter & 0xFFFF) == (short)LogLevelFilter.Invalid );
                Assert.That( (short)(f.LineFilter & 0xFFFF) == (short)LogLevelFilter.Invalid );
            }
            {
                SourceLogFilter f = new SourceLogFilter( new LogFilter( LogLevelFilter.Off, LogLevelFilter.Fatal ), new LogFilter( LogLevelFilter.Error, LogLevelFilter.Warn ) );
                Assert.That( (short)(f.GroupFilter >> 16) == (short)LogLevelFilter.Off );
                Assert.That( (short)(f.LineFilter >> 16) == (short)LogLevelFilter.Fatal );
                Assert.That( (short)(f.GroupFilter & 0xFFFF) == (short)LogLevelFilter.Error );
                Assert.That( (short)(f.LineFilter & 0xFFFF) == (short)LogLevelFilter.Warn );
            }
        }
    }
}
