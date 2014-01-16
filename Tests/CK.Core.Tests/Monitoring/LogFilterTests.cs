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
            Assert.That( LogFilter.Undefined.ToString(), Is.EqualTo( "{None,None}" ) );
            Assert.That( LogFilter.Terse.ToString(), Is.EqualTo( "{Info,Error}" ) );
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
    }
}
