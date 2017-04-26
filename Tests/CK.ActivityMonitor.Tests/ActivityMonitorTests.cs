using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using System.Xml.Linq;
using System.Collections.Generic;
using CK.Text;
using FluentAssertions;

namespace CK.Core.Tests.Monitoring
{
    public class ActivityMonitorTests : MutexTest<ActivityMonitor>
    {
        public ActivityMonitorTests()
        {
            TestHelper.ConsoleMonitor.MinimalFilter = LogFilter.Undefined;
            ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
            ActivityMonitor.CriticalErrorCollector.Clear();
        }

        protected override void OnDispose()
        {
            var e = ActivityMonitor.CriticalErrorCollector.ToArray();
            e.Should().BeEmpty();
        }

        [Fact]
        public void automatic_configuration_of_monitors_just_uses_ActivityMonitor_AutoConfiguration_delegate()
        {
            using (LockFact())
            {
                StupidStringClient c = new StupidStringClient();

                ActivityMonitor.AutoConfiguration = null;
                ActivityMonitor.AutoConfiguration += m => m.Output.RegisterClient(c);
                int i = 0;
                ActivityMonitor.AutoConfiguration += m => m.Trace().Send("This monitors has been created at {0:O}, n°{1}", DateTime.UtcNow, ++i);

                ActivityMonitor monitor1 = new ActivityMonitor();
                ActivityMonitor monitor2 = new ActivityMonitor();

                c.ToString().Should().Contain("This monitors has been created at");
                c.ToString().Should().Contain("n°1");
                c.ToString().Should().Contain("n°2");

                ActivityMonitor.AutoConfiguration = null;
            }
        }

        [Fact]
        public void registering_multiple_times_the_same_client_is_an_error()
        {
            using (LockFact())
            {
                ActivityMonitor.AutoConfiguration = null;
                IActivityMonitor monitor = new ActivityMonitor();
                monitor.Output.Clients.Should().HaveCount(0);

                var counter = new ActivityMonitorErrorCounter();
                monitor.Output.RegisterClient(counter);
                monitor.Output.Clients.Should().HaveCount(1);
                Should.Throw<InvalidOperationException>(() => TestHelper.ConsoleMonitor.Output.RegisterClient(counter), "Counter can be registered in one source at a time.");

                var pathCatcher = new ActivityMonitorPathCatcher();
                monitor.Output.RegisterClient(pathCatcher);
                monitor.Output.Clients.Should().HaveCount(2);
                Should.Throw<InvalidOperationException>(() => TestHelper.ConsoleMonitor.Output.RegisterClient(pathCatcher), "PathCatcher can be registered in one source at a time.");

                IActivityMonitor other = new ActivityMonitor(applyAutoConfigurations: false);
                ActivityMonitorBridge bridgeToConsole;
                using (monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    bridgeToConsole = monitor.Output.FindBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);
                    monitor.Output.Clients.Should().HaveCount(3);
                    bridgeToConsole.TargetMonitor.Should().BeSameAs(TestHelper.ConsoleMonitor);

                    Should.Throw<InvalidOperationException>(() => other.Output.RegisterClient(bridgeToConsole), "Bridge can be associated to only one source monitor.");
                }
                monitor.Output.Clients.Should().HaveCount(2);

                other.Output.RegisterClient(bridgeToConsole); // Now we can.

                monitor.Output.UnregisterClient(bridgeToConsole); // Already removed.
                monitor.Output.UnregisterClient(counter);
                monitor.Output.UnregisterClient(pathCatcher);
                monitor.Output.Clients.Should().HaveCount(0);
            }
        }

        [Fact]
        public void registering_a_null_client_is_an_error()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            Should.Throw<ArgumentNullException>( () => monitor.Output.RegisterClient( null ) );
            Should.Throw<ArgumentNullException>( () => monitor.Output.UnregisterClient( null ) );
        }

        [Fact]
        public void registering_a_null_bridge_or_a_bridge_to_an_already_briged_target_is_an_error()
        {
            using (LockFact())
            {
                IActivityMonitor monitor = new ActivityMonitor();
                Should.Throw<ArgumentNullException>(() => monitor.Output.CreateBridgeTo(null));
                Should.Throw<ArgumentNullException>(() => monitor.Output.UnbridgeTo(null));

                monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);
                Should.Throw<InvalidOperationException>(() => monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget));
                monitor.Output.UnbridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);

                IActivityMonitorOutput output = null;
                Should.Throw<NullReferenceException>(() => output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget));
                Should.Throw<NullReferenceException>(() => output.UnbridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget));
                Should.Throw<ArgumentNullException>(() => new ActivityMonitorBridge(null, false, false), "Null guards.");

            }
        }

        [Fact]
        public void when_bridging_unbalanced_close_groups_are_automatically_handled()
        {
            using (LockFact())
            {
                //Main app monitor
                IActivityMonitor mainMonitor = new ActivityMonitor();
                var mainDump = mainMonitor.Output.RegisterClient(new StupidStringClient());
                using (mainMonitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    //Domain monitor
                    IActivityMonitor domainMonitor = new ActivityMonitor();
                    var domainDump = domainMonitor.Output.RegisterClient(new StupidStringClient());

                    int i = 0;
                    for (; i < 10; i++) mainMonitor.OpenInfo().Send("NOT Bridged n°{0}", i);

                    using (domainMonitor.Output.CreateBridgeTo(mainMonitor.Output.BridgeTarget))
                    {
                        domainMonitor.OpenInfo().Send("Bridged n°10");
                        domainMonitor.OpenInfo().Send("Bridged n°20");
                        domainMonitor.CloseGroup("Bridged close n°10");
                        domainMonitor.CloseGroup("Bridged close n°20");

                        using (domainMonitor.OpenInfo().Send("Bridged n°50"))
                        {
                            using (domainMonitor.OpenInfo().Send("Bridged n°60"))
                            {
                                using (domainMonitor.OpenInfo().Send("Bridged n°70"))
                                {
                                    // Number of Prematurely closed by Bridge removed depends on the max level of groups open
                                }
                            }
                        }
                    }

                    int j = 0;
                    for (; j < 10; j++) mainMonitor.CloseGroup(String.Format("NOT Bridge close n°{0}", j));
                }

                string allText = mainDump.ToString();
                Regex.Matches(allText, Impl.ActivityMonitorResources.ClosedByBridgeRemoved).Should().HaveCount(0, "All Info groups are closed, no need to automatically close other groups");
            }
        }

        [Fact]
        public void when_bridging_unbalanced_close_groups_are_automatically_handled_more_tests()
        {
            using (LockFact())
            {
                IActivityMonitor monitor = new ActivityMonitor();
                var allDump = monitor.Output.RegisterClient(new StupidStringClient());

                LogFilter InfoInfo = new LogFilter(LogLevelFilter.Info, LogLevelFilter.Info);

                // The pseudoConsole is a string dump of the console.
                // Both the console and the pseudoConsole accepts at most Info level.
                IActivityMonitor pseudoConsole = new ActivityMonitor();
                var consoleDump = pseudoConsole.Output.RegisterClient(new StupidStringClient());
                pseudoConsole.MinimalFilter = InfoInfo;
                TestHelper.ConsoleMonitor.MinimalFilter = InfoInfo;
                // The monitor that is bridged to the Console accepts everything.
                monitor.MinimalFilter = LogFilter.Trace;

                int i = 0;
                for (; i < 60; i++) monitor.OpenInfo().Send("Not Bridged n°{0}", i);
                int j = 0;
                using (monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                using (monitor.Output.CreateBridgeTo(pseudoConsole.Output.BridgeTarget))
                {
                    for (; i < 62; i++) monitor.OpenInfo().Send("Bridged n°{0} (appear in Console)", i);
                    for (; i < 64; i++) monitor.OpenTrace().Send("Bridged n°{0} (#NOT appear# in Console since level is Trace)", i);
                    for (; i < 66; i++) monitor.OpenWarn().Send("Bridged n°{0} (appear in Console)", i);

                    // Now close the groups, but not completely.
                    for (; j < 2; j++) monitor.CloseGroup(String.Format("Close n°{0} (Close Warn appear in Console)", j));
                    monitor.CloseGroup(String.Format("Close n°{0} (Close Trace does #NOT appear# in Console)", j++));

                    // Disposing: This removes the bridge to the console: the Trace is not closed (not opened because of Trace level), but the 2 Info are automatically closed.
                }
                string consoleText = consoleDump.ToString();
                consoleText.Should().NotContain("#NOT appear#");
                Regex.Matches(consoleText, "Close Warn appear").Should().HaveCount(2);
                Regex.Matches(consoleText, Impl.ActivityMonitorResources.ClosedByBridgeRemoved).Should().HaveCount(2, "The 2 Info groups have been automatically closed, but not the Warn nor the 60 first groups.");

                for (; j < 66; j++) monitor.CloseGroup(String.Format("CLOSE NOT BRIDGED - {0}", j));
                monitor.CloseGroup("NEVER OPENED Group");

                string allText = allDump.ToString();
                allText.Should().NotContain(Impl.ActivityMonitorResources.ClosedByBridgeRemoved);
                Regex.Matches(allText, "#NOT appear#").Should().HaveCount(3, "The 2 opened Warn + the only explicit close.");
                Regex.Matches(allText, "CLOSE NOT BRIDGED").Should().HaveCount(63, "The 60 opened groups at the beginning + the last Trace and the 2 Info.");
                allText.Should().NotContain("NEVER OPENED");
            }
        }

        [Fact]
        public void closing_group_restores_previous_AutoTags_and_MinimalFilter()
        {
            using (LockFact())
            {
                ActivityMonitor monitor = new ActivityMonitor();
                using (monitor.OpenTrace().Send("G1"))
                {
                    monitor.AutoTags = ActivityMonitor.Tags.Register("Tag");
                    monitor.MinimalFilter = LogFilter.Monitor;
                    using (monitor.OpenWarn().Send("G2"))
                    {
                        monitor.AutoTags = ActivityMonitor.Tags.Register("A|B|C");
                        monitor.MinimalFilter = LogFilter.Release;
                        monitor.AutoTags.ToString().Should().Be("A|B|C");
                        monitor.MinimalFilter.Should().Be(LogFilter.Release);
                    }
                    monitor.AutoTags.ToString().Should().Be("Tag");
                    monitor.MinimalFilter.Should().Be(LogFilter.Monitor);
                }
                monitor.AutoTags.Should().BeSameAs(ActivityMonitor.Tags.Empty);
                monitor.MinimalFilter.Should().Be(LogFilter.Undefined);
            }
        }

        [Fact]
        public void Off_FilterLevel_prevents_all_logs_even_UnfilteredLogs()
        {
            using (LockFact())
            {
                var m = new ActivityMonitor(false);
                var c = m.Output.RegisterClient(new StupidStringClient());
                m.Trace().Send("Trace1");
                m.MinimalFilter = LogFilter.Off;
                m.UnfilteredLog(ActivityMonitor.Tags.Empty, LogLevel.Fatal, "NOSHOW-1", m.NextLogTime(), null);
                m.UnfilteredOpenGroup(ActivityMonitor.Tags.Empty, LogLevel.Fatal, null, "NOSHOW-2", m.NextLogTime(), null);
                m.UnfilteredLog(ActivityMonitor.Tags.Empty, LogLevel.Error, "NOSHOW-3", m.NextLogTime(), null);
                // Off will be restored by the group closing.
                m.MinimalFilter = LogFilter.Trace;
                m.CloseGroup("NOSHOW-4");
                m.MinimalFilter = LogFilter.Trace;
                m.Trace().Send("Trace2");

                var s = c.ToString();
                s.Should().Contain("Trace1").And.Contain("Trace2");
                s.Should().NotContain("NOSHOW");
            }
        }

        [Fact]
        public void sending_a_null_or_empty_text_is_transformed_into_no_log_text()
        {
            using (LockFact())
            {
                var m = new ActivityMonitor(false);
                var c = m.Output.RegisterClient(new StupidStringClient());
                m.Trace().Send("");
                m.UnfilteredLog(null, LogLevel.Error, null, m.NextLogTime(), null);
                m.OpenTrace().Send(ActivityMonitor.Tags.Empty, null, this);
                m.OpenInfo().Send("");

                c.Entries.All(e => e.Text == ActivityMonitor.NoLogText);
            }
        }

        [Fact]
        public void display_conclusions()
        {
            using (LockFact())
            {
                IActivityMonitor monitor = new ActivityMonitor(false);
                using (monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    monitor.Output.RegisterClients(new StupidStringClient(), new StupidXmlClient(new StringWriter()));
                    monitor.Output.Clients.Should().HaveCount(3);

                    var tag1 = ActivityMonitor.Tags.Register("Product");
                    var tag2 = ActivityMonitor.Tags.Register("Sql");
                    var tag3 = ActivityMonitor.Tags.Register("Combined Tag|Sql|Engine V2|Product");

                    using (monitor.OpenError().Send("MainGroupError").ConcludeWith(() => "EndMainGroupError"))
                    {
                        using (monitor.OpenTrace().Send("MainGroup").ConcludeWith(() => "EndMainGroup"))
                        {
                            monitor.Trace().Send(tag1, "First");
                            using (monitor.TemporarilySetAutoTags(tag1))
                            {
                                monitor.Trace().Send("Second");
                                monitor.Trace().Send(tag3, "Third");
                                using (monitor.TemporarilySetAutoTags(tag2))
                                {
                                    monitor.Info().Send("First");
                                }
                            }
                            using (monitor.OpenInfo().Send("InfoGroup").ConcludeWith(() => "Conclusion of Info Group (no newline)."))
                            {
                                monitor.Info().Send("Second");
                                monitor.Trace().Send("Fourth");

                                string warnConclusion = "Conclusion of Warn Group" + Environment.NewLine + "with more than one line int it.";
                                using (monitor.OpenWarn().Send("WarnGroup {0} - Now = {1}", 4, DateTime.UtcNow).ConcludeWith(() => warnConclusion))
                                {
                                    monitor.Info().Send("Warn!");
                                    monitor.CloseGroup("User conclusion with multiple lines."
                                        + Environment.NewLine + "It will be displayed on "
                                        + Environment.NewLine + "multiple lines.");
                                }
                                monitor.CloseGroup("Conclusions on one line are displayed separated by dash.");
                            }
                        }
                    }


                    if (TestHelper.LogsToConsole)
                    {
                        Console.WriteLine(monitor.Output.Clients.OfType<StupidStringClient>().Single().Writer);
                        Console.WriteLine(monitor.Output.Clients.OfType<StupidXmlClient>().Single().InnerWriter);
                    }

                    IReadOnlyList<XElement> elements = monitor.Output.Clients.OfType<StupidXmlClient>().Single().XElements;

                    elements.Descendants("Info").Should().HaveCount(3);
                    elements.Descendants("Trace").Should().HaveCount(2);
                }
            }
        }

        [Fact]
        public void exceptions_are_deeply_dumped()
        {
            using (LockFact())
            {
                IActivityMonitor l = new ActivityMonitor(applyAutoConfigurations: false);
                var wLogLovely = new StringBuilder();
                var rawLog = new StupidStringClient();
                l.Output.RegisterClient(rawLog);
                using (l.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    var logLovely = new ActivityMonitorTextWriterClient((s) => wLogLovely.Append(s));
                    l.Output.RegisterClient(logLovely);

                    l.Error().Send(new Exception("EXERROR-1"));
                    using (l.OpenFatal().Send(new Exception("EXERROR-2"), "EXERROR-TEXT2"))
                    {
                        try
                        {
                            throw new Exception("EXERROR-3");
                        }
                        catch (Exception ex)
                        {
                            l.Trace().Send(ex, "EXERROR-TEXT3");
                        }
                    }
                }
                rawLog.ToString().Should().Contain("EXERROR-1");
                rawLog.ToString().Should().Contain("EXERROR-2").And.Contain("EXERROR-TEXT2");
                rawLog.ToString().Should().Contain("EXERROR-3").And.Contain("EXERROR-TEXT3");

                string text = wLogLovely.ToString();
                text.Should().Contain("EXERROR-1");
                text.Should().Contain("EXERROR-2").And.Contain("EXERROR-TEXT2");
                text.Should().Contain("EXERROR-3").And.Contain("EXERROR-TEXT3");
                text.Should().Contain("Stack:");
            }
        }

        [Fact]
        public void ending_a_monitor_send_an_unfilitered_MonitorEnd_tagged_info()
        {
            using (LockFact())
            {
                IActivityMonitor m = new ActivityMonitor(applyAutoConfigurations: false);
                var rawLog = new StupidStringClient();
                m.Output.RegisterClient(rawLog);
                m.OpenFatal().Send("a group");
                // OpenFatal or OpenError sets their scoped filter to Debug.
                m.MinimalFilter = LogFilter.Release;
                m.OpenInfo().Send("a (filtered) group");
                m.Fatal().Send("a line");
                m.Info().Send("a (filtered) line");
                m.MonitorEnd();
                m.CloseGroup().Should().BeFalse();
                string logs = rawLog.ToString();
                logs.Should().NotContain("(filtered)");
                logs.Should().Match("*a group*a line*Done.*", "We used the default 'Done.' end text.");
            }
        }

        [Fact]
        public void AggregatedException_are_handled_specifically()
        {
            using (LockFact())
            {
                IActivityMonitor l = new ActivityMonitor(applyAutoConfigurations: false);
                var wLogLovely = new StringBuilder();
                using (l.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {

                    var logLovely = new ActivityMonitorTextWriterClient((s) => wLogLovely.Append(s));
                    l.Output.RegisterClient(logLovely);


                    l.Error().Send(new Exception("EXERROR-1"));
                    using (l.OpenFatal().Send(new Exception("EXERROR-2"), "EXERROR-TEXT2"))
                    {
                        try
                        {
                            throw new AggregateException(
                                new Exception("EXERROR-Aggreg-1"),
                                new AggregateException(
                                    new Exception("EXERROR-Aggreg-2-1"),
                                    new Exception("EXERROR-Aggreg-2-2")
                                ),
                                new Exception("EXERROR-Aggreg-3"));
                        }
                        catch (Exception ex)
                        {
                            l.Error().Send(ex, "EXERROR-TEXT3");
                        }
                    }
                }
                string text = wLogLovely.ToString();
                text.Should().Contain("EXERROR-Aggreg-1");
                text.Should().Contain("EXERROR-Aggreg-2-1");
                text.Should().Contain("EXERROR-Aggreg-2-2");
                text.Should().Contain("EXERROR-Aggreg-3");
            }
        }

        [Fact]
        public void closing_a_group_when_no_group_is_opened_is_ignored()
        {
            using (LockFact())
            {
                IActivityMonitor monitor = new ActivityMonitor(applyAutoConfigurations: false);
                using (monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    var log1 = monitor.Output.RegisterClient(new StupidStringClient());
                    monitor.Output.Clients.Should().HaveCount(2);

                    using (monitor.OpenTrace().Send("First").ConcludeWith(() => "End First"))
                    {
                        monitor.CloseGroup("Pouf");
                        using (monitor.OpenWarn().Send("A group at level 0!"))
                        {
                            monitor.CloseGroup("Close it.");
                            monitor.CloseGroup("Close it again. (not seen)");
                        }
                    }
                    string logged = log1.Writer.ToString();
                    logged.Should().Contain("Pouf").And.Contain("End First", "Multiple conclusions.");
                    logged.Should().NotContain("Close it again", "Close forgets other closes...");
                }
            }
        }

        [Fact]
        public void testing_filtering_levels()
        {
            using (LockFact())
            {
                LogFilter FatalFatal = new LogFilter(LogLevelFilter.Fatal, LogLevelFilter.Fatal);
                LogFilter WarnWarn = new LogFilter(LogLevelFilter.Warn, LogLevelFilter.Warn);

                IActivityMonitor l = new ActivityMonitor(false);
                using (l.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    var log = l.Output.RegisterClient(new StupidStringClient());
                    using (l.TemporarilySetMinimalFilter(LogLevelFilter.Error, LogLevelFilter.Error))
                    {
                        l.Debug().Send("NO SHOW");
                        l.Trace().Send("NO SHOW");
                        l.Info().Send("NO SHOW");
                        l.Warn().Send("NO SHOW");
                        l.Error().Send("Error n°1.");
                        using (l.TemporarilySetMinimalFilter(WarnWarn))
                        {
                            l.Debug().Send("NO SHOW");
                            l.Trace().Send("NO SHOW");
                            l.Info().Send("NO SHOW");
                            l.Warn().Send("Warn n°1.");
                            l.Error().Send("Error n°2.");
                            using (l.OpenWarn().Send("GroupWarn: this appears."))
                            {
                                l.MinimalFilter.Should().Be(WarnWarn, "Groups does not change the current filter level.");
                                l.Debug().Send("NO SHOW");
                                l.Trace().Send("NO SHOW");
                                l.Info().Send("NO SHOW");
                                l.Warn().Send("Warn n°2.");
                                l.Error().Send("Error n°3.");
                                // Changing the level inside a Group.
                                l.MinimalFilter = FatalFatal;
                                l.Error().Send("NO SHOW");
                                l.Fatal().Send("Fatal n°1.");
                            }
                            using (l.OpenInfo().Send("GroupInfo: NO SHOW."))
                            {
                                l.MinimalFilter.Should().Be(WarnWarn, "Groups does not change the current filter level.");
                                l.Debug().Send("NO SHOW");
                                l.Trace().Send("NO SHOW");
                                l.Info().Send("NO SHOW");
                                l.Warn().Send("Warn n°2-bis.");
                                l.Error().Send("Error n°3-bis.");
                                // Changing the level inside a Group.
                                l.MinimalFilter = FatalFatal;
                                l.Error().Send("NO SHOW");
                                l.Fatal().Send("Fatal n°1.");
                                using (l.OpenError().Send("GroupError: NO SHOW."))
                                {
                                }
                            }
                            l.MinimalFilter.Should().Be(WarnWarn, "But Groups restores the original filter level when closed.");
                            l.Debug().Send("NO SHOW");
                            l.Trace().Send("NO SHOW");
                            l.Info().Send("NO SHOW");
                            l.Warn().Send("Warn n°3.");
                            l.Error().Send("Error n°4.");
                            l.Fatal().Send("Fatal n°2.");
                        }
                        l.Debug().Send("NO SHOW");
                        l.Trace().Send("NO SHOW");
                        l.Info().Send("NO SHOW");
                        l.Warn().Send("NO SHOW");
                        l.Error().Send("Error n°5.");
                    }
                    string result = log.Writer.ToString();
                    result.Should().NotContain("NO SHOW");
                    result.Should().Contain("Error n°1.")
                                .And.Contain("Error n°2.")
                                .And.Contain("Error n°3.")
                                .And.Contain("Error n°3-bis.")
                                .And.Contain("Error n°4.")
                                .And.Contain("Error n°5.");
                    result.Should().Contain("Warn n°1.")
                                .And.Contain("Warn n°2.")
                                .And.Contain("Warn n°2-bis.")
                                .And.Contain("Warn n°3.");
                    result.Should().Contain("Fatal n°1.")
                                .And.Contain("Fatal n°2.");
                }
            }
        }

        [Fact]
        public void mismatch_of_explicit_Group_disposing_is_handled()
        {
            using (LockFact())
            {
                IActivityMonitor l = new ActivityMonitor();
                var log = l.Output.RegisterClient(new StupidStringClient());
                {
                    IDisposable g0 = l.OpenTrace().Send("First");
                    IDisposable g1 = l.OpenTrace().Send("Second");
                    IDisposable g2 = l.OpenTrace().Send("Third");

                    g1.Dispose();
                    l.Trace().Send("Inside First");
                    g0.Dispose();
                    l.Trace().Send("At root");

                    var end = "Trace: Inside First" + Environment.NewLine + "-" + Environment.NewLine + "Trace: At root";
                    log.Writer.ToString().Should().EndWith(end);
                }
                {
                    // g2 is closed after g1.
                    IDisposable g0 = l.OpenTrace().Send("First");
                    IDisposable g1 = l.OpenTrace().Send("Second");
                    IDisposable g2 = l.OpenTrace().Send("Third");
                    log.Writer.GetStringBuilder().Clear();
                    g1.Dispose();
                    // g2 has already been disposed by g1. 
                    // Nothing changed.
                    g2.Dispose();
                    l.Trace().Send("Inside First");
                    g0.Dispose();
                    l.Trace().Send("At root");

                    var end = "Trace: Inside First" + Environment.NewLine + "-" + Environment.NewLine + "Trace: At root";
                    log.Writer.ToString().Should().EndWith(end);
                }
            }
        }

        class ObjectAsConclusion
        {
            public override string ToString()
            {
                return "Explicit User Conclusion";
            }
        }

        [Fact]
        public void appending_multiple_conclusions_to_a_group_is_possible()
        {
            using (LockFact())
            {
                IActivityMonitor l = new ActivityMonitor();
                using (l.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    l.Output.RegisterClient(new ActivityMonitorErrorCounter(true));
                    var log = l.Output.RegisterClient(new StupidStringClient());

                    // No explicit close conclusion: Success!
                    using (l.OpenTrace().Send("G").ConcludeWith(() => "From Opener"))
                    {
                        l.Error().Send("Pouf");
                        l.CloseGroup(new ObjectAsConclusion());
                    }
                    log.Writer.ToString().Should().Contain("Explicit User Conclusion, From Opener, 1 Error");
                }
            }
        }

        [Fact]
        public void ActivityMonitorPathCatcher_is_aClient_that_maintains_the_current_Group_path()
        {
            using (LockFact())
            {
                var monitor = new ActivityMonitor();
                ActivityMonitorPathCatcher p = monitor.Output.RegisterClient(new ActivityMonitorPathCatcher());
                monitor.MinimalFilter = LogFilter.Debug;

                using (monitor.OpenDebug().Send("!D"))
                using (monitor.OpenTrace().Send("!T"))
                using (monitor.OpenInfo().Send("!I"))
                using (monitor.OpenWarn().Send("!W"))
                using (monitor.OpenError().Send("!E"))
                using (monitor.OpenFatal().Send("!F"))
                {
                    p.DynamicPath.ToStringPath()
                       .Should().Contain("!D").And.Contain("!T").And.Contain("!I").And.Contain("!W").And.Contain("!E").And.Contain("!F");
                }
                var path = p.DynamicPath;
                path = null;
                path.ToStringPath().Should().BeEmpty();
            }
        }

        [Fact]
        public void ActivityMonitorPathCatcher_tests()
        {
            using (LockFact())
            {

                var monitor = new ActivityMonitor(applyAutoConfigurations: false);
                using (monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {
                    ActivityMonitorPathCatcher p = new ActivityMonitorPathCatcher();
                    monitor.Output.RegisterClient(p);

                    monitor.Trace().Send("Trace n°1");
                    p.DynamicPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Trace|Trace n°1");
                    p.LastErrorPath.Should().BeNull();
                    p.LastWarnOrErrorPath.Should().BeNull();

                    monitor.Trace().Send("Trace n°2");
                    p.DynamicPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Trace|Trace n°2");
                    p.LastErrorPath.Should().BeNull();
                    p.LastWarnOrErrorPath.Should().BeNull();

                    monitor.Warn().Send("W1");
                    p.DynamicPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Warn|W1");
                    p.LastErrorPath.Should().BeNull();
                    p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Warn|W1");

                    monitor.Error().Send("E2");
                    monitor.Warn().Send("W1bis");
                    p.DynamicPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Warn|W1bis");
                    p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Error|E2");
                    p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text).Single().Should().Be("Warn|W1bis");

                    p.ClearLastWarnPath();
                    p.LastErrorPath.Should().NotBeNull();
                    p.LastWarnOrErrorPath.Should().BeNull();

                    p.ClearLastErrorPath();
                    p.LastErrorPath.Should().BeNull();

                    using (monitor.OpenTrace().Send("G1"))
                    {
                        using (monitor.OpenInfo().Send("G2"))
                        {
                            String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2");
                            p.LastErrorPath.Should().BeNull();
                            using (monitor.OpenTrace().Send("G3"))
                            {
                                using (monitor.OpenInfo().Send("G4"))
                                {
                                    monitor.Warn().Send("W1");

                                    String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2>G3>G4>W1");

                                    monitor.Info().Send(
                                        new Exception("An exception logged as an Info.",
                                            new Exception("With an inner exception. Since these exceptions have not been thrown, there is no stack trace.")),
                                        "Test With an exception: a Group is created. Since the text of the log is given, the Exception.Message must be displayed explicitely.");

                                    string.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2>G3>G4>Test With an exception: a Group is created. Since the text of the log is given, the Exception.Message must be displayed explicitely.");

                                    try
                                    {
                                        try
                                        {
                                            try
                                            {
                                                try
                                                {
                                                    throw new Exception("Deepest exception.");
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception("Yet another inner with inner Exception.", ex);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception("Exception with inner Exception.", ex);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception("Log without log text: the text of the entry is the Exception.Message.", ex);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        monitor.Trace().Send(ex);
                                        p.DynamicPath.ToStringPath().Length.Should().BeGreaterThan(0);
                                    }

                                    p.LastErrorPath.Should().BeNull();
                                    string.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1");
                                }
                                String.Join(">", p.DynamicPath.Select(e => e.ToString())).Should().Be("G1>G2>G3>G4");
                                p.LastErrorPath.Should().BeNull();
                                String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1");

                                monitor.Error().Send("E1");
                                String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2>G3>E1");
                                String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");
                                String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");
                            }
                            String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2>G3");
                            String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");
                            String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");
                        }
                        String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2");
                        using (monitor.OpenTrace().Send("G2Bis"))
                        {
                            String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2Bis");
                            String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");
                            String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");

                            monitor.Warn().Send("W2");
                            String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>G2Bis>W2");
                            String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Trace|G2Bis>Warn|W2");
                            String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Info|G2>Trace|G3>Error|E1");
                        }
                        monitor.Fatal().Send("F1");
                        String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("G1>F1");
                        String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Fatal|F1");
                        String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Fatal|F1");
                    }

                    // Extraneous closing are ignored.
                    monitor.CloseGroup(null);

                    monitor.Warn().Send("W3");
                    String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("W3");
                    String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Warn|W3");
                    String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Fatal|F1");

                    // Extraneous closing are ignored.
                    monitor.CloseGroup(null);

                    monitor.Warn().Send("W4");
                    String.Join(">", p.DynamicPath.Select(e => e.Text)).Should().Be("W4");
                    String.Join(">", p.LastWarnOrErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Warn|W4");
                    String.Join(">", p.LastErrorPath.Select(e => e.MaskedLevel.ToString() + '|' + e.Text)).Should().Be("Trace|G1>Fatal|F1");

                    p.ClearLastWarnPath(true);
                    p.LastErrorPath.Should().BeNull();
                    p.LastWarnOrErrorPath.Should().BeNull();
                }
            }
        }

        [Fact]
        public void ActivityMonitorErrorCounter_and_ActivityMonitorPathCatcher_Clients_work_together()
        {
            using (LockFact())
            {
                var monitor = new ActivityMonitor(applyAutoConfigurations: false);
                using (monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget))
                {

                    // Registers the ErrorCounter first: it will be the last one to be called, but
                    // this does not prevent the PathCatcher to work: the path elements reference the group
                    // so that any conclusion arriving after PathCatcher.OnClosing are available.
                    ActivityMonitorErrorCounter c = new ActivityMonitorErrorCounter();
                    monitor.Output.RegisterClient(c);

                    // Registers the PathCatcher now: it will be called BEFORE the ErrorCounter.
                    ActivityMonitorPathCatcher p = new ActivityMonitorPathCatcher();
                    monitor.Output.RegisterClient(p);

                    c.GenerateConclusion.Should().BeFalse("False by default.");
                    c.GenerateConclusion = true;
                    c.Root.MaxLogLevel.Should().Be(LogLevel.None);

                    monitor.Trace().Send("T1");
                    c.Root.HasWarnOrError.Should().BeFalse();
                    c.Root.HasError.Should().BeFalse();
                    c.Root.MaxLogLevel.Should().Be(LogLevel.Trace);
                    c.Root.ToString().Should().BeNull();

                    monitor.Warn().Send("W1");
                    c.Root.HasWarnOrError.Should().BeTrue();
                    c.Root.HasError.Should().BeFalse();
                    c.Root.MaxLogLevel.Should().Be(LogLevel.Warn);
                    c.Root.ToString().Should().NotBeNullOrEmpty();

                    monitor.Error().Send("E2");
                    c.Root.HasWarnOrError.Should().BeTrue();
                    c.Root.HasError.Should().BeTrue();
                    c.Root.ErrorCount.Should().Be(1);
                    c.Root.MaxLogLevel.Should().Be(LogLevel.Error);
                    c.Root.ToString().Should().NotBeNullOrEmpty();

                    c.Root.ClearError();
                    c.Root.HasWarnOrError.Should().BeTrue();
                    c.Root.HasError.Should().BeFalse();
                    c.Root.ErrorCount.Should().Be(0);
                    c.Root.MaxLogLevel.Should().Be(LogLevel.Warn);
                    c.Root.ToString().Should().NotBeNull();

                    c.Root.ClearWarn();
                    c.Root.HasWarnOrError.Should().BeFalse();
                    c.Root.HasError.Should().BeFalse();
                    c.Root.MaxLogLevel.Should().Be(LogLevel.Info);
                    c.Root.ToString().Should().BeNull();

                    using (monitor.OpenTrace().Send("G1"))
                    {
                        string errorMessage;
                        using (monitor.OpenInfo().Send("G2"))
                        {
                            monitor.Error().Send("E1");
                            monitor.Fatal().Send("F1");
                            c.Root.HasWarnOrError.Should().BeTrue();
                            c.Root.HasError.Should().BeTrue();
                            c.Root.ErrorCount.Should().Be(1);
                            c.Root.FatalCount.Should().Be(1);
                            c.Root.WarnCount.Should().Be(0);

                            using (monitor.OpenInfo().Send("G3"))
                            {
                                c.Current.HasWarnOrError.Should().BeFalse();
                                c.Current.HasError.Should().BeFalse();
                                c.Current.ErrorCount.Should().Be(0);
                                c.Current.FatalCount.Should().Be(0);
                                c.Current.WarnCount.Should().Be(0);

                                monitor.Error().Send("An error...");

                                c.Current.HasWarnOrError.Should().BeTrue();
                                c.Current.HasError.Should().BeTrue();
                                c.Current.ErrorCount.Should().Be(1);
                                c.Current.FatalCount.Should().Be(0);
                                c.Current.WarnCount.Should().Be(0);

                                errorMessage = String.Join("|", p.LastErrorPath.Select(e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion()));
                                errorMessage.Should().Be("G1-|G2-|G3-|An error...-", "Groups are not closed: no conclusion exist yet.");
                            }
                            errorMessage = String.Join("|", p.LastErrorPath.Select(e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion()));
                            errorMessage.Should().Be("G1-|G2-|G3-1 Error|An error...-", "G3 is closed: its conclusion is available.");
                        }
                        errorMessage = String.Join("|", p.LastErrorPath.Select(e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion()));
                        errorMessage.Should().Be("G1-|G2-1 Fatal error, 2 Errors|G3-1 Error|An error...-");
                        monitor.Error().Send("E3");
                        monitor.Fatal().Send("F2");
                        monitor.Warn().Send("W2");
                        c.Root.HasWarnOrError.Should().BeTrue();
                        c.Root.HasError.Should().BeTrue();
                        c.Root.FatalCount.Should().Be(2);
                        c.Root.ErrorCount.Should().Be(3);
                        c.Root.MaxLogLevel.Should().Be(LogLevel.Fatal);
                    }
                    String.Join(">", p.LastErrorPath.Select(e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion())).Should().Be("G1-2 Fatal errors, 3 Errors, 1 Warning>F2-");
                    String.Join(">", p.LastWarnOrErrorPath.Select(e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion())).Should().Be("G1-2 Fatal errors, 3 Errors, 1 Warning>W2-");
                }
            }
        }

        [Fact]
        public void ActivityMonitorSimpleCollector_is_a_Client_that_filters_and_stores_its_Capacity_count_of_last_log_entries()
        {
            using (LockFact())
            {
                IActivityMonitor d = new ActivityMonitor( applyAutoConfigurations: false );
                var c = new ActivityMonitorSimpleCollector();
                d.Output.RegisterClient(c);
                d.Warn().Send("1");
                d.Error().Send("2");
                d.Fatal().Send("3");
                d.Trace().Send("4");
                d.Info().Send("5");
                d.Warn().Send("6");
                String.Join(",", c.Entries.Select(e => e.Text)).Should().Be("2,3");

                c.MinimalFilter = LogLevelFilter.Fatal;
                String.Join(",", c.Entries.Select(e => e.Text)).Should().Be("3");

                c.MinimalFilter = LogLevelFilter.Off;
                String.Join(",", c.Entries.Select(e => e.Text)).Should().Be("");

                c.MinimalFilter = LogLevelFilter.Warn;
                using (d.OpenWarn().Send("1"))
                {
                    d.Error().Send("2");
                    using (d.OpenFatal().Send("3"))
                    {
                        d.Trace().Send("4");
                        d.Info().Send("5");
                    }
                }
                d.Warn().Send("6");
                String.Join(",", c.Entries.Select(e => e.Text)).Should().Be("1,2,3,6");

                c.MinimalFilter = LogLevelFilter.Fatal;
                String.Join(",", c.Entries.Select(e => e.Text)).Should().Be("3");

                c.MinimalFilter = LogLevelFilter.Debug;
                d.MinimalFilter = LogFilter.Debug;
                using (d.OpenDebug().Send("d1"))
                {
                    d.Debug().Send("d2");
                    using (d.OpenFatal().Send("f1"))
                    {
                        d.Debug().Send("d3");
                        d.Info().Send("i1");
                    }
                }
                String.Join(",", c.Entries.Select(e => e.Text)).Should().Be("3,d1,d2,f1,d3,i1");

            }
        }

        [Fact]
        public void ActivityMonitorTextWriterClient_has_its_own_LogFilter()
        {
            using (LockFact())
            {
                StringBuilder sb = new StringBuilder();

                IActivityMonitor d = new ActivityMonitor();
                d.MinimalFilter = LogFilter.Trace;

                var c = new ActivityMonitorTextWriterClient(s => sb.Append(s), LogFilter.Release);
                d.Output.RegisterClient(c);

                d.Trace().Send("NO SHOW");
                d.Trace().Send("NO SHOW");
                using (d.OpenTrace().Send("NO SHOW"))
                {
                    d.Info().Send("NO SHOW");
                    d.Info().Send("NO SHOW");
                }

                d.Error().Send("Error line at root");
                using (d.OpenInfo().Send("NO SHOW"))
                {
                    d.Warn().Send("NO SHOW");
                    d.Error().Send("Send error line inside group");
                    using (d.OpenError().Send("Open error group"))
                    {
                        d.Error().Send("Send error line inside sub group");
                    }
                }

                sb.ToString().Should().NotContain("NO SHOW");
                sb.ToString().Should().Contain("Error line at root");
                sb.ToString().Should().Contain("Send error line inside group");
                sb.ToString().Should().Contain("Open error group");
                sb.ToString().Should().Contain("Send error line inside sub group");
            }
        }

        [Fact]
        public void OnError_fires_synchronously()
        {
            using (LockFact())
            {
                var m = new ActivityMonitor(false);
                bool hasError = false;
                using (m.OnError(() => hasError = true))
                using (m.OpenInfo().Send("Handling StObj objects."))
                {
                    m.Fatal().Send("Oops!");
                    hasError.Should().BeTrue();
                    hasError = false;
                    m.OpenFatal().Send("Oops! (Group)").Dispose();
                    hasError.Should().BeTrue();
                    hasError = false;
                }
                hasError = false;
                m.Fatal().Send("Oops!");
                hasError.Should().BeFalse();

                bool hasFatal = false;
                using (m.OnError(() => hasFatal = true, () => hasError = true))
                {
                    m.Fatal().Send("Big Oops!");
                    hasFatal.Should().BeTrue();
                    hasError.Should().BeFalse();
                    m.Error().Send("Oops!");
                    hasFatal.Should().BeTrue();
                    hasError.Should().BeTrue();
                    hasFatal = hasError = false;
                    m.OpenError().Send("Oops! (Group)").Dispose();
                    hasFatal.Should().BeFalse();
                    hasError.Should().BeTrue();
                    m.OpenFatal().Send("Oops! (Group)").Dispose();
                    hasFatal.Should().BeTrue(); hasError.Should().BeTrue();
                    hasFatal = hasError = false;
                }
                m.Fatal().Send("Oops!");
                hasFatal.Should().BeFalse();
                hasError.Should().BeFalse();
            }
        }

        [Fact]
        public void setting_the_MininimalFilter_of_a_bound_Client_is_thread_safe()
        {
            using (LockFact())
            {
                ActivityMonitor m = new ActivityMonitor(false);
                var tester = m.Output.RegisterClient(new ActivityMonitorClientTester());

                m.ActualFilter.Should().Be(LogFilter.Undefined);
                tester.AsyncSetMinimalFilterBlock(LogFilter.Monitor);
                m.ActualFilter.Should().Be(LogFilter.Monitor);
            }
        }

        class CheckAlwaysFilteredClient : ActivityMonitorClient
        {
            protected override void OnOpenGroup( IActivityLogGroup group )
            {
                 (group.GroupLevel & LogLevel.IsFiltered).Should().NotBe( 0 );
            }

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                 data.IsFilteredLog.Should().BeTrue( );
            }
        }

        [Fact]
        public void testing_all_the_overloads_of_StandardSender()
        {
            using (LockFact())
            {
                Exception ex = new Exception("EXCEPTION");
                string fmt0 = "fmt", fmt1 = "fmt{0}", fmt2 = "fmt{0}{1}", fmt3 = "fmt{0}{1}{2}", fmt4 = "fmt{0}{1}{2}{3}", fmt5 = "fmt{0}{1}{2}{3}{4}", fmt6 = "fmt{0}{1}{2}{3}{4}{5}";
                string p1 = "p1", p2 = "p2", p3 = "p3", p4 = "p4", p5 = "p5", p6 = "p6";
                Func<string> onDemandText = () => "onDemand";
                Func<int, string> onDemandTextP1 = (i) => "onDemand" + i.ToString();
                Func<int, int, string> onDemandTextP2 = (i, j) => "onDemand" + i.ToString() + j.ToString();

                IActivityMonitor d = new ActivityMonitor();
                var collector = new ActivityMonitorSimpleCollector() { MinimalFilter = LogLevelFilter.Trace, Capacity = 1 };
                d.Output.RegisterClients(collector, new CheckAlwaysFilteredClient());

                // d.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Trace, "CheckAlwaysFilteredClient works", DateTime.UtcNow, null );

                d.Trace().Send(fmt0); collector.Entries.Last().Text.Should().Be("fmt");
                d.Trace().Send(fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1");
                Should.Throw<ArgumentException>(() => d.Trace().Send(fmt1, ex));
                d.Trace().Send(onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Trace().Send(onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Trace().Send(onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Trace().Send(fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2");
                d.Trace().Send(fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3");
                d.Trace().Send(fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4");
                d.Trace().Send(fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5");
                d.Trace().Send(fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6");

                d.Info().Send(fmt0); collector.Entries.Last().Text.Should().Be("fmt");
                d.Info().Send(fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1");
                Should.Throw<ArgumentException>(() => d.Info().Send(fmt1, ex));
                d.Info().Send(onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Info().Send(onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Info().Send(onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Info().Send(fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2");
                d.Info().Send(fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3");
                d.Info().Send(fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4");
                d.Info().Send(fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5");
                d.Info().Send(fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6");

                d.Warn().Send(fmt0); collector.Entries.Last().Text.Should().Be("fmt");
                d.Warn().Send(fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1");
                Should.Throw<ArgumentException>(() => d.Warn().Send(fmt1, ex));
                d.Warn().Send(onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Warn().Send(onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Warn().Send(onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Warn().Send(fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2");
                d.Warn().Send(fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3");
                d.Warn().Send(fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4");
                d.Warn().Send(fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5");
                d.Warn().Send(fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6");

                d.Error().Send(fmt0); collector.Entries.Last().Text.Should().Be("fmt");
                d.Error().Send(fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1");
                Should.Throw<ArgumentException>(() => d.Error().Send(fmt1, ex));
                d.Error().Send(onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Error().Send(onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Error().Send(onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Error().Send(fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2");
                d.Error().Send(fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3");
                d.Error().Send(fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4");
                d.Error().Send(fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5");
                d.Error().Send(fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6");

                d.Fatal().Send(fmt0); collector.Entries.Last().Text.Should().Be("fmt");
                d.Fatal().Send(fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1");
                Should.Throw<ArgumentException>(() => d.Fatal().Send(fmt1, ex));
                d.Fatal().Send(onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Fatal().Send(onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Fatal().Send(onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Fatal().Send(fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2");
                d.Fatal().Send(fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3");
                d.Fatal().Send(fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4");
                d.Fatal().Send(fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5");
                d.Fatal().Send(fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6");

                d.OpenTrace().Send(fmt0); collector.Entries.Last().Text.Should().Be("fmt");
                d.OpenTrace().Send(fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1");
                d.OpenTrace().Send(fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2");
                d.OpenTrace().Send(fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3");
                d.OpenTrace().Send(fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4");
                d.OpenTrace().Send(fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5");
                d.OpenTrace().Send(fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6");

                d.Trace().Send(ex); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Trace().Send(ex, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex);

                d.Info().Send(ex); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Info().Send(ex, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex);

                d.Warn().Send(ex); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Warn().Send(ex, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex);

                d.Error().Send(ex); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Error().Send(ex, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex);

                d.Fatal().Send(ex); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.Fatal().Send(ex, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex);

                d.OpenTrace().Send(ex); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
                d.OpenTrace().Send(ex, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex);
            }
        }


        [Fact]
        public void testing_all_the_overloads_of_StandardSender_with_Traits()
        {
            using (LockFact())
            {

                Exception ex = new Exception("EXCEPTION");
                string fmt0 = "fmt", fmt1 = "fmt{0}", fmt2 = "fmt{0}{1}", fmt3 = "fmt{0}{1}{2}", fmt4 = "fmt{0}{1}{2}{3}", fmt5 = "fmt{0}{1}{2}{3}{4}", fmt6 = "fmt{0}{1}{2}{3}{4}{5}";
                string p1 = "p1", p2 = "p2", p3 = "p3", p4 = "p4", p5 = "p5", p6 = "p6";
                Func<string> onDemandText = () => "onDemand";
                Func<int, string> onDemandTextP1 = (i) => "onDemand" + i.ToString();
                Func<int, int, string> onDemandTextP2 = (i, j) => "onDemand" + i.ToString() + j.ToString();

                IActivityMonitor d = new ActivityMonitor();
                var collector = new ActivityMonitorSimpleCollector() { MinimalFilter = LogLevelFilter.Trace, Capacity = 1 };
                d.Output.RegisterClients(collector, new CheckAlwaysFilteredClient());

                CKTrait tag = ActivityMonitor.Tags.Register("TAG");

                d.Trace().Send(tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                Should.Throw<ArgumentException>(() => d.Trace().Send(tag, fmt1, ex));
                d.Trace().Send(tag, onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Trace().Send(tag, onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Trace().Send(tag, onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Trace().Send(tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Info().Send(tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                Should.Throw<ArgumentException>(() => d.Info().Send(tag, fmt1, ex));
                d.Info().Send(tag, onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Info().Send(tag, onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Info().Send(tag, onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Info().Send(tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Warn().Send(tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                Should.Throw<ArgumentException>(() => d.Warn().Send(tag, fmt1, ex));
                d.Warn().Send(tag, onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Warn().Send(tag, onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Warn().Send(tag, onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Warn().Send(tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Error().Send(tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                Should.Throw<ArgumentException>(() => d.Error().Send(tag, fmt1, ex));
                d.Error().Send(tag, onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Error().Send(tag, onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Error().Send(tag, onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Error().Send(tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Fatal().Send(tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                Should.Throw<ArgumentException>(() => d.Fatal().Send(tag, fmt1, ex));
                d.Fatal().Send(tag, onDemandText); collector.Entries.Last().Text.Should().Be("onDemand");
                d.Fatal().Send(tag, onDemandTextP1, 1); collector.Entries.Last().Text.Should().Be("onDemand1");
                d.Fatal().Send(tag, onDemandTextP2, 1, 2); collector.Entries.Last().Text.Should().Be("onDemand12");
                d.Fatal().Send(tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.OpenTrace().Send(tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Trace().Send(ex, tag); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Trace().Send(ex, tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Info().Send(ex, tag); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Info().Send(ex, tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Warn().Send(ex, tag); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Warn().Send(ex, tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Error().Send(ex, tag); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Error().Send(ex, tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.Fatal().Send(ex, tag); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.Fatal().Send(ex, tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);

                d.OpenTrace().Send(ex, tag); collector.Entries.Last().Text.Should().Be("EXCEPTION"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt0); collector.Entries.Last().Text.Should().Be("fmt"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt1, p1); collector.Entries.Last().Text.Should().Be("fmtp1"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt2, p1, p2); collector.Entries.Last().Text.Should().Be("fmtp1p2"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt3, p1, p2, p3); collector.Entries.Last().Text.Should().Be("fmtp1p2p3"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt4, p1, p2, p3, p4); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt5, p1, p2, p3, p4, p5); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);
                d.OpenTrace().Send(ex, tag, fmt6, p1, p2, p3, p4, p5, p6); collector.Entries.Last().Text.Should().Be("fmtp1p2p3p4p5p6"); collector.Entries.Last().Exception.Should().BeSameAs(ex); collector.Entries.Last().Tags.Should().BeSameAs(tag);

            }
        }
    }
}
