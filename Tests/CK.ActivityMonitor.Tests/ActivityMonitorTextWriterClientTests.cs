using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Xml.Linq;
using System.Collections.Generic;
using CK.Text;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    [Category( "ActivityMonitor" )]
    public class ActivityMonitorTextWriterClientTests
    {
        [Test]
        [Category( "Console" )]
        public void logging_multiple_lines()
        {
            TestHelper.LogsToConsole = true;
            var m = new ActivityMonitor( false );
            m.MinimalFilter = LogFilter.Debug;
            StringBuilder b = new StringBuilder();
            var client = new ActivityMonitorTextWriterClient( s => b.Append( s ) );
            m.Output.RegisterClient( client );
            using( TestHelper.ConsoleMonitor.SetMinimalFilter( LogFilter.Debug ) )
            using( m.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                using( m.OpenInfo().Send( "IL1" + Environment.NewLine + "IL2" + Environment.NewLine + "IL3" ) )
                {
                    using( m.OpenTrace().Send( "TL1" + Environment.NewLine + "TL2" + Environment.NewLine + "TL3" ) )
                    {
                        m.Warn().Send( "WL1" + Environment.NewLine + "WL2" + Environment.NewLine + "WL3" );
                        m.CloseGroup( new[] 
                        {
                            new ActivityLogGroupConclusion("c1"),
                            new ActivityLogGroupConclusion("c2"),
                            new ActivityLogGroupConclusion("Multi"+Environment.NewLine+"Line"+Environment.NewLine),
                            new ActivityLogGroupConclusion("Another"+Environment.NewLine+"Multi"+Environment.NewLine+"Line"+Environment.NewLine)
                        } );
                    }
                }
            }
            string result = b.ToString();
            Assert.That( result, Is.EqualTo(
@"> Info: IL1
|       IL2
|       IL3
|  > Trace: TL1
|  |        TL2
|  |        TL3
|  |  - Warn: WL1
|  |          WL2
|  |          WL3
|  < c1 - c2
|  < Multi
|    Line
|  < Another
|    Multi
|    Line
".NormalizeEOL() ) );
        }
    }
}
