using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace CK.Core.Tests.Monitoring
{
    public class ActivityMonitorFilterPropagation
    {
        [Fact]
        public void ThreadSafeOnClientMinimalFilterChanged()
        {
            var monitor = new ActivityMonitor(applyAutoConfigurations:false);
            var c = monitor.Output.RegisterClient(new ActivityMonitorClientTester());
            Parallel.For(0, 20, i => c.AsyncSetMinimalFilterBlock(new LogFilter(LogLevelFilter.Info, (LogLevelFilter)(i % 5 + 1)), 1));

        }

        [Fact]
        public void ClientFilterPropagatesToMonitor()
        {
            var monitor = new ActivityMonitor( applyAutoConfigurations: false);
            var client = new ActivityMonitorConsoleClient();
            monitor.Output.RegisterClient(client);

            monitor.MinimalFilter.Should().Be(LogFilter.Undefined);

            client.Filter = LogFilter.Release;

            client.Filter.Should().Be(LogFilter.Release);
            monitor.ActualFilter.Should().Be(LogFilter.Release);
        }

    }
}
