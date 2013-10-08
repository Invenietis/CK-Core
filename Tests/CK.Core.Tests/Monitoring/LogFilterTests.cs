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
            LogFilter f = new LogFilter( LogLevelFilter.Error, LogLevelFilter.None );
            LogFilter f2 = f.SetGroup( LogLevelFilter.Info );
            Assert.That( f2.Line == LogLevelFilter.Error && f2.Group == LogLevelFilter.Info );
            LogFilter f3 = new LogFilter( LogLevelFilter.Info, LogLevelFilter.Trace );
            LogFilter f4 = f2.Combine( f3 );
            Assert.That( f4.Equals( f3 ) ); 
            Assert.That( f4 == f3 ); 
        }
    }
}
