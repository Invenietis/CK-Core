using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class ActivityMonitorFilterPropagation
    {
        [Test]
        public void ThreadSafeOnClientMinimalFilterChanged()
        {
            var monitor = new ActivityMonitor( false );
            var c = monitor.Output.RegisterClient( new ActivityMonitorClientTester() );

            for( int i = 0; i < 20; ++i )
                Task.Run( () =>
                {
                    c.AsyncSetMinimalFilterBlock( new LogFilter( LogLevelFilter.Info, (LogLevelFilter)(i % 5 + 1) ), 1 );
                } ).Wait();
        }

    }
}
