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
            Parallel.For( 0, 20, i => c.AsyncSetMinimalFilterBlock( new LogFilter( LogLevelFilter.Info, (LogLevelFilter)(i % 5 + 1) ), 1 ) );

        }

        [Test]
        public void ClientFilterPropagatesToMonitor()
        {
            var monitor = new ActivityMonitor( false );
            var client = new ActivityMonitorConsoleClient();
            monitor.Output.RegisterClient( client );

            Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Undefined ) );

            client.Filter = LogFilter.Release;

            Assert.That( client.Filter, Is.EqualTo( LogFilter.Release ) );
            Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Release ) );
        }

    }
}
