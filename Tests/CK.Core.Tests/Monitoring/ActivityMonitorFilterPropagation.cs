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
            Parallel.For( 0, 20, i => c.AsyncSetMinimalFilterBlock( (LogLevelFilter)(i%5 + 1), 1 ) );

        }

    }
}
