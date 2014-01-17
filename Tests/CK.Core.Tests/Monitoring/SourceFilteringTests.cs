using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class SourceFilteringTests
    {
        [Test]
        public void FileNamesAreInternedString()
        {
            ThisFile();
        }

        string ThisFile( [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            Assert.That( String.IsInterned( fileName ) != null );
            Assert.That( lineNumber > 0 );
            return fileName;
        }

        [Test]
        public void SourceFileFilterTest()
        {
            var m = new ActivityMonitor( applyAutoConfigurations: false );
            var c = m.Output.RegisterClient( new StupidStringClient() );
            m.Trace().Send( "This file DOPASS: {0}", ThisFile() );
            ActivityMonitor.SourceFilter.SetFileFilter( LogFilter.Release );
            m.Trace().Send( "This one DONOTPASS: {0}", ThisFile() );
            ActivityMonitor.SourceFilter.SetFileFilter( LogFilter.Undefined );
            m.Trace().Send( "This one PASSAGAIN: {0}", ThisFile() );

            Assert.That( c.Entries[0].Text.Contains( "DOPASS" ) );
            Assert.That( c.Entries.Any( e => e.Text.Contains( "DONOTPASS" ) ), Is.False );
            Assert.That( c.Entries[1].Text.Contains( "PASSAGAIN" ) );
        }

    }
}
