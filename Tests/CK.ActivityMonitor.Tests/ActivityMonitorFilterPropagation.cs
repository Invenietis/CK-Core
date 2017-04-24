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
        public void Client_minimal_filter_changes_is_thread_safe()
        {
            var monitor = new ActivityMonitor(applyAutoConfigurations:false);
            var c = monitor.Output.RegisterClient(new ActivityMonitorClientTester());
            Parallel.For(0, 20, i => c.AsyncSetMinimalFilterBlock(new LogFilter(LogLevelFilter.Info, (LogLevelFilter)(i % 5 + 1)), 1));
        }

        [Fact]
        public void Client_filter_propagates_to_monitor()
        {
            var monitor = new ActivityMonitor(applyAutoConfigurations: false);
            var client = new ActivityMonitorConsoleClient();
            monitor.Output.RegisterClient(client);

            monitor.MinimalFilter.Should().Be(LogFilter.Undefined);

            client.Filter = LogFilter.Release;

            client.Filter.Should().Be(LogFilter.Release);
            monitor.ActualFilter.Should().Be(LogFilter.Release);
        }

        [Fact]
        public void ultimate_default_filter_is_the_static_ActivityMonitor_DefaultFilter_that_is_Trace_by_default()
        {
            var monitor = new ActivityMonitor(applyAutoConfigurations: false);
            var client = new StupidStringClient();
            monitor.Output.RegisterClient(client);

            ActivityMonitor.DefaultFilter.Should().Be(LogFilter.Trace);
            monitor.MinimalFilter.Should().Be(LogFilter.Undefined);
            monitor.ActualFilter.Should().Be(LogFilter.Undefined);
            monitor.Trace().Send("n°1");
            monitor.Debug().Send("NOSHOW 1");

            ActivityMonitor.DefaultFilter = LogFilter.Debug;
            monitor.MinimalFilter.Should().Be(LogFilter.Undefined);
            monitor.ActualFilter.Should().Be(LogFilter.Undefined);
            monitor.Trace().Send("n°2");
            monitor.Debug().Send("Debug works.");
            ActivityMonitor.DefaultFilter = LogFilter.Trace;

            client.ToString().Should().Match("*n°1*n°2*Debug works.*");
            client.ToString().Should().NotMatch("*NOSHOW*");
        }

    }
}
