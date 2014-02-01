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
        public void SourceFileOverrideFilterTest()
        {
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                Assert.That( m.ActualFilter, Is.EqualTo( LogFilter.Undefined ) );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Release );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Undefined );
                m.Trace().Send( "Trace2" );
                m.OpenTrace().Send( "OTrace2" );

                CollectionAssert.AreEqual( new[] { "Trace1", "OTrace1", "Trace2", "OTrace2" }, c.Entries.Select( e => e.Text ).ToArray(), StringComparer.OrdinalIgnoreCase );
            }
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                m.MinimalFilter = LogFilter.Terse;
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Debug );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Undefined );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );

                CollectionAssert.AreEqual( new[] { "Trace1", "OTrace1" }, c.Entries.Select( e => e.Text ).ToArray(), StringComparer.OrdinalIgnoreCase );
            }
        }

        [Test]
        public void SourceFileMinimalFilterTest()
        {
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                Assert.That( m.ActualFilter, Is.EqualTo( LogFilter.Undefined ) );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Release );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Undefined );
                m.Trace().Send( "Trace2" );
                m.OpenTrace().Send( "OTrace2" );

                CollectionAssert.AreEqual( new[] { "Trace1", "OTrace1", "Trace2", "OTrace2" }, c.Entries.Select( e => e.Text ).ToArray(), StringComparer.OrdinalIgnoreCase );
            }
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                m.MinimalFilter = LogFilter.Terse;
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Debug );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Undefined );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );

                CollectionAssert.AreEqual( new[] { "Trace1", "OTrace1" }, c.Entries.Select( e => e.Text ).ToArray(), StringComparer.OrdinalIgnoreCase );
            }
        }

    }
}
