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
    public class ActivityMonitorTests
    {
        [SetUp]
        public void ClearActivityMonitorErrors()
        {
            TestHelper.ConsoleMonitor.MinimalFilter = LogFilter.Undefined;
            ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
            ActivityMonitor.CriticalErrorCollector.Clear();
        }

        [TearDown]
        public void CheckActivityMonitorErrors()
        {
            var e = ActivityMonitor.CriticalErrorCollector.ToArray();
            Assert.That( e, Is.Empty );
        }

        [Test]
        public void automatic_configuration_of_monitors_just_uses_ActivityMonitor_AutoConfiguration_delegate()
        {
            StupidStringClient c = new StupidStringClient();

            ActivityMonitor.AutoConfiguration = null;
            ActivityMonitor.AutoConfiguration += m => m.Output.RegisterClient( c );
            int i = 0;
            ActivityMonitor.AutoConfiguration += m => m.Trace().Send( "This monitors has been created at {0:O}, n°{1}", DateTime.UtcNow, ++i );

            ActivityMonitor monitor1 = new ActivityMonitor();
            ActivityMonitor monitor2 = new ActivityMonitor();

            Assert.That( c.ToString(), Does.Contain( "This monitors has been created at" ) );
            Assert.That( c.ToString(), Does.Contain( "n°1" ) );
            Assert.That( c.ToString(), Does.Contain( "n°2" ) );

            ActivityMonitor.AutoConfiguration = null;
        }

        [Test]
        public void registering_multiple_times_the_same_client_is_an_error()
        {
            ActivityMonitor.AutoConfiguration = null;
            IActivityMonitor monitor = new ActivityMonitor();
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 0 ) );

            var counter = new ActivityMonitorErrorCounter();
            monitor.Output.RegisterClient( counter );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 1 ) );
            Assert.Throws<InvalidOperationException>( () => TestHelper.ConsoleMonitor.Output.RegisterClient( counter ), "Counter can be registered in one source at a time." );

            var pathCatcher = new ActivityMonitorPathCatcher();
            monitor.Output.RegisterClient( pathCatcher );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ) );
            Assert.Throws<InvalidOperationException>( () => TestHelper.ConsoleMonitor.Output.RegisterClient( pathCatcher ), "PathCatcher can be registered in one source at a time." );

            IActivityMonitor other = new ActivityMonitor( applyAutoConfigurations: false );
            ActivityMonitorBridge bridgeToConsole;
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                bridgeToConsole = monitor.Output.FindBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget );
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 3 ) );
                Assert.That( bridgeToConsole.TargetMonitor, Is.SameAs( TestHelper.ConsoleMonitor ) );

                Assert.Throws<InvalidOperationException>( () => other.Output.RegisterClient( bridgeToConsole ), "Bridge can be associated to only one source monitor." );
            }
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ) );

            Assert.DoesNotThrow( () => other.Output.RegisterClient( bridgeToConsole ), "Now we can." );

            Assert.DoesNotThrow( () => monitor.Output.UnregisterClient( bridgeToConsole ), "Already removed." );
            monitor.Output.UnregisterClient( counter );
            monitor.Output.UnregisterClient( pathCatcher );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 0 ) );
        }

        [Test]
        [Category( "ActivityMonitor" )]
        public void registering_a_null_client_is_an_error()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            Assert.Throws<ArgumentNullException>( () => monitor.Output.RegisterClient( null ) );
            Assert.Throws<ArgumentNullException>( () => monitor.Output.UnregisterClient( null ) );
        }

        [Test]
        [Category( "Console" )]
        public void registering_a_null_bridge_or_a_bridge_to_an_already_briged_target_is_an_error()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            Assert.Throws<ArgumentNullException>( () => monitor.Output.CreateBridgeTo( null ) );
            Assert.Throws<ArgumentNullException>( () => monitor.Output.UnbridgeTo( null ) );

            monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget );
            Assert.Throws<InvalidOperationException>( () => monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) );
            monitor.Output.UnbridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget );

            IActivityMonitorOutput output = null;
            Assert.Throws<NullReferenceException>( () => output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) );
            Assert.Throws<NullReferenceException>( () => output.UnbridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) );
            Assert.Throws<ArgumentNullException>( () => new ActivityMonitorBridge( null, false, false ), "Null guards." );
        }

        [Test]
        [Category( "Console" )]
        public void when_bridging_unbalanced_close_groups_are_automatically_handled()
        {
            //Main app monitor
            IActivityMonitor mainMonitor = new ActivityMonitor();
            var mainDump = mainMonitor.Output.RegisterClient( new StupidStringClient() );
            using(mainMonitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ))
            {
                //Domain monitor
                IActivityMonitor domainMonitor = new ActivityMonitor();
                var domainDump = domainMonitor.Output.RegisterClient( new StupidStringClient() );

                int i = 0;
                for(; i < 10; i++) mainMonitor.OpenInfo().Send( "NOT Bridged n°{0}", i );

                using(domainMonitor.Output.CreateBridgeTo( mainMonitor.Output.BridgeTarget ))
                {
                    domainMonitor.OpenInfo().Send( "Bridged n°10" );
                    domainMonitor.OpenInfo().Send( "Bridged n°20" );
                    domainMonitor.CloseGroup( "Bridged close n°10" );
                    domainMonitor.CloseGroup( "Bridged close n°20" );

                    using(domainMonitor.OpenInfo().Send("Bridged n°50") )
                    {
                        using(domainMonitor.OpenInfo().Send( "Bridged n°60" ))
                        {
                            using(domainMonitor.OpenInfo().Send( "Bridged n°70" ))
                            {
                                // Number of Prematurely closed by Bridge removed depends on the max level of groups open
                            }
                        }
                    }
                }

                int j = 0;
                for(; j < 10; j++) mainMonitor.CloseGroup( String.Format( "NOT Bridge close n°{0}", j ) );
            }

            string allText = mainDump.ToString();
            Assert.That( Regex.Matches( allText, Impl.ActivityMonitorResources.ClosedByBridgeRemoved ).Count, Is.EqualTo( 0 ), "All Info groups are closed, no need to automatically close other groups" );
        }

        [Test]
        [Category( "Console" )]
        public void when_bridging_unbalanced_close_groups_are_automatically_handled_more_tests()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            var allDump = monitor.Output.RegisterClient( new StupidStringClient() );

            LogFilter InfoInfo = new LogFilter( LogLevelFilter.Info, LogLevelFilter.Info );

            // The pseudoConsole is a string dump of the console.
            // Both the console and the pseudoConsole accepts at most Info level.
            IActivityMonitor pseudoConsole = new ActivityMonitor();
            var consoleDump = pseudoConsole.Output.RegisterClient( new StupidStringClient() );
            pseudoConsole.MinimalFilter = InfoInfo;
            TestHelper.ConsoleMonitor.MinimalFilter = InfoInfo;
            // The monitor that is bridged to the Console accepts everything.
            monitor.MinimalFilter = LogFilter.Debug;

            int i = 0;
            for( ; i < 60; i++ ) monitor.OpenInfo().Send( "Not Bridged n°{0}", i );
            int j = 0;
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            using( monitor.Output.CreateBridgeTo( pseudoConsole.Output.BridgeTarget ) )
            {
                for( ; i < 62; i++ ) monitor.OpenInfo().Send( "Bridged n°{0} (appear in Console)", i );
                for( ; i < 64; i++ ) monitor.OpenTrace().Send( "Bridged n°{0} (#NOT appear# in Console since level is Trace)", i );
                for( ; i < 66; i++ ) monitor.OpenWarn().Send( "Bridged n°{0} (appear in Console)", i );

                // Now close the groups, but not completely.
                for( ; j < 2; j++ ) monitor.CloseGroup( String.Format( "Close n°{0} (Close Warn appear in Console)", j ) );
                monitor.CloseGroup( String.Format( "Close n°{0} (Close Trace does #NOT appear# in Console)", j++ ) );

                // Disposing: This removes the bridge to the console: the Trace is not closed (not opened because of Trace level), but the 2 Info are automatically closed.
            }
            string consoleText = consoleDump.ToString();
            Assert.That( consoleText, Is.Not.Contains( "#NOT appear#" ) );
            Assert.That( Regex.Matches( consoleText, "Close Warn appear" ).Count, Is.EqualTo( 2 ) );
            Assert.That( Regex.Matches( consoleText, Impl.ActivityMonitorResources.ClosedByBridgeRemoved ).Count, Is.EqualTo( 2 ), "The 2 Info groups have been automatically closed, but not the Warn nor the 60 first groups." );

            for( ; j < 66; j++ ) monitor.CloseGroup( String.Format( "CLOSE NOT BRIDGED - {0}", j ) );
            monitor.CloseGroup( "NEVER OPENED Group" );

            string allText = allDump.ToString();
            Assert.That( allText, Is.Not.Contains( Impl.ActivityMonitorResources.ClosedByBridgeRemoved ) );
            Assert.That( Regex.Matches( allText, "#NOT appear#" ).Count, Is.EqualTo( 3 ), "The 2 opened Warn + the only explicit close." );
            Assert.That( Regex.Matches( allText, "CLOSE NOT BRIDGED" ).Count, Is.EqualTo( 63 ), "The 60 opened groups at the beginning + the last Trace and the 2 Info." );
            Assert.That( allText, Is.Not.Contains( "NEVER OPENED" ) );
        }

        [Test]
        public void closing_group_restores_previous_AutoTags_and_MinimalFilter()
        {
            ActivityMonitor monitor = new ActivityMonitor();
            using( monitor.OpenTrace().Send( "G1" ) )
            {
                monitor.AutoTags = ActivityMonitor.Tags.Register( "Tag" );
                monitor.MinimalFilter = LogFilter.Monitor;
                using( monitor.OpenWarn().Send( "G2" ) )
                {
                    monitor.AutoTags = ActivityMonitor.Tags.Register( "A|B|C" );
                    monitor.MinimalFilter = LogFilter.Release;
                    Assert.That( monitor.AutoTags.ToString(), Is.EqualTo( "A|B|C" ) );
                    Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Release ) );
                }
                Assert.That( monitor.AutoTags.ToString(), Is.EqualTo( "Tag" ) );
                Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Monitor ) );
            }
            Assert.That( monitor.AutoTags, Is.SameAs( ActivityMonitor.Tags.Empty ) );
            Assert.That( monitor.MinimalFilter, Is.EqualTo( LogFilter.Undefined ) );
        }

        [Test]
        public void Off_FilterLevel_prevents_all_logs_even_UnfilteredLogs()
        {
            var m = new ActivityMonitor( false );
            var c = m.Output.RegisterClient( new StupidStringClient() );
            m.Trace().Send( "Trace1" );
            m.MinimalFilter = LogFilter.Off;
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Fatal, "NOSHOW-1", m.NextLogTime(), null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Fatal, null, "NOSHOW-2", m.NextLogTime(), null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Error, "NOSHOW-3", m.NextLogTime(), null );
            // Off will be restored by the group closing.
            m.MinimalFilter = LogFilter.Debug;
            m.CloseGroup( "NOSHOW-4" );
            m.MinimalFilter = LogFilter.Debug;
            m.Trace().Send( "Trace2" );

            var s = c.ToString();
            Assert.That( s, Does.Contain( "Trace1" ).And.Contain( "Trace2" ) );
            Assert.That( s, Does.Not.Contain( "NOSHOW" ) );
        }

        [Test]
        public void sending_a_null_or_empty_text_is_transformed_into_no_log_text()
        {
            var m = new ActivityMonitor( false );
            var c = m.Output.RegisterClient( new StupidStringClient() );
            m.Trace().Send( "" );
            m.UnfilteredLog( null, LogLevel.Error, null, m.NextLogTime(), null );
            m.OpenTrace().Send( ActivityMonitor.Tags.Empty, null, this );
            m.OpenInfo().Send( "" );

            Assert.That( c.Entries.All( e => e.Text == ActivityMonitor.NoLogText ) );
        }

        [Test]
        [Category( "Console" )]
        public void DefaultImpl()
        {
            IActivityMonitor monitor = new ActivityMonitor( false );
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                monitor.Output.RegisterClients( new StupidStringClient(), new StupidXmlClient( new StringWriter() ) );
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 3 ) );

                var tag1 = ActivityMonitor.Tags.Register( "Product" );
                var tag2 = ActivityMonitor.Tags.Register( "Sql" );
                var tag3 = ActivityMonitor.Tags.Register( "Combined Tag|Sql|Engine V2|Product" );

                using( monitor.OpenError().Send( "MainGroupError" ).ConcludeWith( () => "EndMainGroupError" ) )
                {
                    using( monitor.OpenTrace().Send( "MainGroup" ).ConcludeWith( () => "EndMainGroup" ) )
                    {
                        monitor.Trace().Send( tag1, "First" );
                        using( monitor.SetAutoTags( tag1 ) )
                        {
                            monitor.Trace().Send( "Second" );
                            monitor.Trace().Send( tag3, "Third" );
                            using( monitor.SetAutoTags( tag2 ) )
                            {
                                monitor.Info().Send( "First" );
                            }
                        }
                        using( monitor.OpenInfo().Send( "InfoGroup" ).ConcludeWith( () => "Conclusion of Info Group (no newline)." ) )
                        {
                            monitor.Info().Send( "Second" );
                            monitor.Trace().Send( "Fourth" );

                            string warnConclusion = "Conclusion of Warn Group" + Environment.NewLine + "with more than one line int it.";
                            using( monitor.OpenWarn().Send( "WarnGroup {0} - Now = {1}", 4, DateTime.UtcNow ).ConcludeWith( () => warnConclusion ) )
                            {
                                monitor.Info().Send( "Warn!" );
                                monitor.CloseGroup( "User conclusion with multiple lines."
                                    + Environment.NewLine + "It will be displayed on "
                                    + Environment.NewLine + "multiple lines." );
                            }
                            monitor.CloseGroup( "Conclusions on one line are displayed separated by dash." );
                        }
                    }
                }


                if( TestHelper.LogsToConsole )
                {
                    Console.WriteLine( monitor.Output.Clients.OfType<StupidStringClient>().Single().Writer );
                    Console.WriteLine( monitor.Output.Clients.OfType<StupidXmlClient>().Single().InnerWriter );
                }

                IReadOnlyList<XElement> elements = monitor.Output.Clients.OfType<StupidXmlClient>().Single().XElements;                

                Assert.That( elements.Descendants( "Info" ).Count(), Is.EqualTo( 3 ) );
                Assert.That( elements.Descendants( "Trace" ).Count(), Is.EqualTo( 2 ) );
            }
        }

        [Test]
        [Category( "Console" )]
        public void DumpException()
        {
            IActivityMonitor l = new ActivityMonitor( applyAutoConfigurations: false );
            var wLogLovely = new StringBuilder();
            var rawLog = new StupidStringClient();
            l.Output.RegisterClient( rawLog );
            using( l.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                var logLovely = new ActivityMonitorTextWriterClient( ( s ) => wLogLovely.Append( s ) );
                l.Output.RegisterClient( logLovely );

                l.Error().Send( new Exception( "EXERROR-1" ) );
                using( l.OpenFatal().Send( new Exception( "EXERROR-2" ), "EXERROR-TEXT2" ) )
                {
                    try
                    {
                        throw new Exception( "EXERROR-3" );
                    }
                    catch( Exception ex )
                    {
                        l.Trace().Send( ex, "EXERROR-TEXT3" );
                    }
                }
            }
            Assert.That( rawLog.ToString(), Does.Contain( "EXERROR-1" ) );
            Assert.That( rawLog.ToString(), Does.Contain( "EXERROR-2" ).And.Contain( "EXERROR-TEXT2" ) );
            Assert.That( rawLog.ToString(), Does.Contain( "EXERROR-3" ).And.Contain( "EXERROR-TEXT3" ) );

            string text = wLogLovely.ToString();
            Assert.That( text, Does.Contain( "EXERROR-1" ) );
            Assert.That( text, Does.Contain( "EXERROR-2" ).And.Contain( "EXERROR-TEXT2" ) );
            Assert.That( text, Does.Contain( "EXERROR-3" ).And.Contain( "EXERROR-TEXT3" ) );
            Assert.That( text, Does.Contain( "Stack:" ) );
        }

        [Test]
        [Category( "Console" )]
        public void DumpAggregatedException()
        {
            IActivityMonitor l = new ActivityMonitor( applyAutoConfigurations: false );
            var wLogLovely = new StringBuilder();
            using( l.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {

                var logLovely = new ActivityMonitorTextWriterClient( ( s ) => wLogLovely.Append( s ) );
                l.Output.RegisterClient( logLovely );


                l.Error().Send( new Exception( "EXERROR-1" ) );
                using( l.OpenFatal().Send( new Exception( "EXERROR-2" ), "EXERROR-TEXT2" ) )
                {
                    try
                    {
                        throw new AggregateException(
                            new Exception( "EXERROR-Aggreg-1" ),
                            new AggregateException(
                                new Exception( "EXERROR-Aggreg-2-1" ),
                                new Exception( "EXERROR-Aggreg-2-2" )
                            ),
                            new Exception( "EXERROR-Aggreg-3" ) );
                    }
                    catch( Exception ex )
                    {
                        l.Error().Send( ex, "EXERROR-TEXT3" );
                    }
                }
            }
            string text = wLogLovely.ToString();
            Assert.That( text, Does.Contain( "EXERROR-Aggreg-1" ) );
            Assert.That( text, Does.Contain( "EXERROR-Aggreg-2-1" ) );
            Assert.That( text, Does.Contain( "EXERROR-Aggreg-2-2" ) );
            Assert.That( text, Does.Contain( "EXERROR-Aggreg-3" ) );
        }

        [Test]
        [Category( "Console" )]
        public void ClosingWhenNoGroupIsOpened()
        {
            IActivityMonitor monitor = new ActivityMonitor( applyAutoConfigurations: false );
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                var log1 = monitor.Output.RegisterClient( new StupidStringClient() );
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ) );

                using( monitor.OpenTrace().Send( "First" ).ConcludeWith( () => "End First" ) )
                {
                    monitor.CloseGroup( "Pouf" );
                    using( monitor.OpenWarn().Send( "A group at level 0!" ) )
                    {
                        monitor.CloseGroup( "Close it." );
                        monitor.CloseGroup( "Close it again. (not seen)" );
                    }
                }
                string logged = log1.Writer.ToString();
                Assert.That( logged, Does.Contain( "Pouf" ).And.Contain( "End First" ), "Multiple conclusions." );
                Assert.That( logged, Does.Not.Contain( "Close it again" ), "Close forgets other closes..." );
            }
        }

        [Test]
        [Category( "Console" )]
        public void FilterLevel()
        {
            LogFilter FatalFatal = new LogFilter( LogLevelFilter.Fatal, LogLevelFilter.Fatal );
            LogFilter WarnWarn = new LogFilter( LogLevelFilter.Warn, LogLevelFilter.Warn );

            IActivityMonitor l = new ActivityMonitor( false );
            using( l.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                var log = l.Output.RegisterClient( new StupidStringClient() );
                using( l.SetMinimalFilter( LogLevelFilter.Error, LogLevelFilter.Error ) )
                {
                    l.Trace().Send( "NO SHOW" );
                    l.Info().Send( "NO SHOW" );
                    l.Warn().Send( "NO SHOW" );
                    l.Error().Send( "Error n°1." );
                    using( l.SetMinimalFilter( WarnWarn ) )
                    {
                        l.Trace().Send( "NO SHOW" );
                        l.Info().Send( "NO SHOW" );
                        l.Warn().Send( "Warn n°1." );
                        l.Error().Send( "Error n°2." );
                        using( l.OpenWarn().Send( "GroupWarn: this appears." ) )
                        {
                            Assert.That( l.MinimalFilter, Is.EqualTo( WarnWarn ), "Groups does not change the current filter level." );
                            l.Trace().Send( "NO SHOW" );
                            l.Info().Send( "NO SHOW" );
                            l.Warn().Send( "Warn n°2." );
                            l.Error().Send( "Error n°3." );
                            // Changing the level inside a Group.
                            l.MinimalFilter = FatalFatal;
                            l.Error().Send( "NO SHOW" );
                            l.Fatal().Send( "Fatal n°1." );
                        }
                        using( l.OpenInfo().Send( "GroupInfo: NO SHOW." ) )
                        {
                            Assert.That( l.MinimalFilter, Is.EqualTo( WarnWarn ), "Groups does not change the current filter level." );
                            l.Trace().Send( "NO SHOW" );
                            l.Info().Send( "NO SHOW" );
                            l.Warn().Send( "Warn n°2-bis." );
                            l.Error().Send( "Error n°3-bis." );
                            // Changing the level inside a Group.
                            l.MinimalFilter = FatalFatal;
                            l.Error().Send( "NO SHOW" );
                            l.Fatal().Send( "Fatal n°1." );
                            using( l.OpenError().Send( "GroupError: NO SHOW." ) )
                            {
                            }
                        }
                        Assert.That( l.MinimalFilter, Is.EqualTo( WarnWarn ), "But Groups restores the original filter level when closed." );
                        l.Trace().Send( "NO SHOW" );
                        l.Info().Send( "NO SHOW" );
                        l.Warn().Send( "Warn n°3." );
                        l.Error().Send( "Error n°4." );
                        l.Fatal().Send( "Fatal n°2." );
                    }
                    l.Trace().Send( "NO SHOW" );
                    l.Info().Send( "NO SHOW" );
                    l.Warn().Send( "NO SHOW" );
                    l.Error().Send( "Error n°5." );
                }
                Assert.That( log.Writer.ToString(), Does.Not.Contain( "NO SHOW" ) );
                Assert.That( log.Writer.ToString(), Does.Contain( "Error n°1." )
                                                        .And.Contain( "Error n°2." )
                                                        .And.Contain( "Error n°3." )
                                                        .And.Contain( "Error n°3-bis." )
                                                        .And.Contain( "Error n°4." )
                                                        .And.Contain( "Error n°5." ) );
                Assert.That( log.Writer.ToString(), Does.Contain( "Warn n°1." )
                                                        .And.Contain( "Warn n°2." )
                                                        .And.Contain( "Warn n°2-bis." )
                                                        .And.Contain( "Warn n°3." ) );
                Assert.That( log.Writer.ToString(), Does.Contain( "Fatal n°1." )
                                                        .And.Contain( "Fatal n°2." ) );
            }
        }

        [Test]
        [Category( "Console" )]
        public void CloseMismatch()
        {
            IActivityMonitor l = new ActivityMonitor();
            var log = l.Output.RegisterClient( new StupidStringClient() );
            {
                IDisposable g0 = l.OpenTrace().Send( "First" );
                IDisposable g1 = l.OpenTrace().Send( "Second" );
                IDisposable g2 = l.OpenTrace().Send( "Third" );

                g1.Dispose();
                l.Trace().Send( "Inside First" );
                g0.Dispose();
                l.Trace().Send( "At root" );

                var end = "Trace: Inside First" + Environment.NewLine + "-" + Environment.NewLine + "Trace: At root";
                Assert.That( log.Writer.ToString(), Does.EndWith( end ) );
            }
            {
                // g2 is closed after g1.
                IDisposable g0 = l.OpenTrace().Send( "First" );
                IDisposable g1 = l.OpenTrace().Send( "Second" );
                IDisposable g2 = l.OpenTrace().Send( "Third" );
                log.Writer.GetStringBuilder().Clear();
                g1.Dispose();
                // g2 has already been disposed by g1. 
                // Nothing changed.
                g2.Dispose();
                l.Trace().Send( "Inside First" );
                g0.Dispose();
                l.Trace().Send( "At root" );

                var end = "Trace: Inside First" + Environment.NewLine + "-" + Environment.NewLine + "Trace: At root";
                Assert.That( log.Writer.ToString(), Does.EndWith( end ) );
            }
        }

        class ObjectAsConclusion
        {
            public override string ToString()
            {
                return "Explicit User Conclusion";
            }
        }

        [Test]
        [Category( "Console" )]
        public void MultipleConclusions()
        {
            IActivityMonitor l = new ActivityMonitor();
            using( l.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                l.Output.RegisterClient( new ActivityMonitorErrorCounter( true ) );
                var log = l.Output.RegisterClient( new StupidStringClient() );

                // No explicit close conclusion: Success!
                using( l.OpenTrace().Send( "G" ).ConcludeWith( () => "From Opener" ) )
                {
                    l.Error().Send( "Pouf" );
                    l.CloseGroup( new ObjectAsConclusion() );
                }
                Assert.That( log.Writer.ToString(), Does.Contain( "Explicit User Conclusion, From Opener, 1 Error" ) );
            }
        }

        [Test]
        public void PathCatcherToStringPath()
        {
            var monitor = new ActivityMonitor();
            ActivityMonitorPathCatcher p = monitor.Output.RegisterClient( new ActivityMonitorPathCatcher() );

            using( monitor.OpenTrace().Send( "!T" ) )
            using( monitor.OpenInfo().Send( "!I" ) )
            using( monitor.OpenWarn().Send( "!W" ) )
            using( monitor.OpenError().Send( "!E" ) )
            using( monitor.OpenFatal().Send( "!F" ) )
            {
                Assert.That( p.DynamicPath.ToStringPath(), Does.Contain( "!T" ).And.Contain( "!I" ).And.Contain( "!W" ).And.Contain( "!E" ).And.Contain( "!F" ) );
            }
            var path = p.DynamicPath;
            path = null;
            Assert.That( path.ToStringPath(), Is.Empty );
        }

        [Test]
        [Category( "Console" )]
        public void PathCatcherTests()
        {
            var monitor = new ActivityMonitor( applyAutoConfigurations: false );
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                ActivityMonitorPathCatcher p = new ActivityMonitorPathCatcher();
                monitor.Output.RegisterClient( p );

                monitor.Trace().Send( "Trace n°1" );
                Assert.That( p.DynamicPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Trace|Trace n°1" ) );
                Assert.That( p.LastErrorPath, Is.Null );
                Assert.That( p.LastWarnOrErrorPath, Is.Null );

                monitor.Trace().Send( "Trace n°2" );
                Assert.That( p.DynamicPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Trace|Trace n°2" ) );
                Assert.That( p.LastErrorPath, Is.Null );
                Assert.That( p.LastWarnOrErrorPath, Is.Null );

                monitor.Warn().Send( "W1" );
                Assert.That( p.DynamicPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1" ) );
                Assert.That( p.LastErrorPath, Is.Null );
                Assert.That( p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1" ) );

                monitor.Error().Send( "E2" );
                monitor.Warn().Send( "W1bis" );
                Assert.That( p.DynamicPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1bis" ) );
                Assert.That( p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Error|E2" ) );
                Assert.That( p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1bis" ) );

                p.ClearLastWarnPath();
                Assert.That( p.LastErrorPath, Is.Not.Null );
                Assert.That( p.LastWarnOrErrorPath, Is.Null );

                p.ClearLastErrorPath();
                Assert.That( p.LastErrorPath, Is.Null );

                using( monitor.OpenTrace().Send( "G1" ) )
                {
                    using( monitor.OpenInfo().Send( "G2" ) )
                    {
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                        Assert.That( p.LastErrorPath, Is.Null );
                        using( monitor.OpenTrace().Send( "G3" ) )
                        {
                            using( monitor.OpenInfo().Send( "G4" ) )
                            {
                                monitor.Warn().Send( "W1" );

                                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>G4>W1" ) );

                                monitor.Info().Send(
                                    new Exception( "An exception logged as an Info.",
                                        new Exception( "With an inner exception. Since these exceptions have not been thrown, there is no stack trace." ) ),
                                    "Test With an exception: a Group is created. Since the text of the log is given, the Exception.Message must be displayed explicitely." );

                                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>G4>Test With an exception: a Group is created. Since the text of the log is given, the Exception.Message must be displayed explicitely." ) );

                                try
                                {
                                    try
                                    {
                                        try
                                        {
                                            try
                                            {
                                                throw new Exception( "Deepest exception." );
                                            }
                                            catch( Exception ex )
                                            {
                                                throw new Exception( "Yet another inner with inner Exception.", ex );
                                            }
                                        }
                                        catch( Exception ex )
                                        {
                                            throw new Exception( "Exception with inner Exception.", ex );
                                        }
                                    }
                                    catch( Exception ex )
                                    {
                                        throw new Exception( "Log without log text: the text of the entry is the Exception.Message.", ex );
                                    }
                                }
                                catch( Exception ex )
                                {
                                    monitor.Trace().Send( ex );
                                    Assert.That( p.DynamicPath.ToStringPath().Length > 0 );
                                }

                                Assert.That( p.LastErrorPath, Is.Null );
                                Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );
                            }
                            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.ToString() ) ), Is.EqualTo( "G1>G2>G3>G4" ) );
                            Assert.That( p.LastErrorPath, Is.Null );
                            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );

                            monitor.Error().Send( "E1" );
                            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>E1" ) );
                            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                        }
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3" ) );
                        Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    }
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                    using( monitor.OpenTrace().Send( "G2Bis" ) )
                    {
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2Bis" ) );
                        Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );

                        monitor.Warn().Send( "W2" );
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2Bis>W2" ) );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Trace|G2Bis>Warn|W2" ) );
                        Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    }
                    monitor.Fatal().Send( "F1" );
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>F1" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );
                }

                // Extraneous closing are ignored.
                monitor.CloseGroup( null );

                monitor.Warn().Send( "W3" );
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "W3" ) );
                Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Warn|W3" ) );
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );

                // Extraneous closing are ignored.
                monitor.CloseGroup( null );

                monitor.Warn().Send( "W4" );
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "W4" ) );
                Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Warn|W4" ) );
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.MaskedLevel.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );

                p.ClearLastWarnPath( true );
                Assert.That( p.LastErrorPath, Is.Null );
                Assert.That( p.LastWarnOrErrorPath, Is.Null );
            }
        }

        [Test]
        [Category( "Console" )]
        public void ErrorCounterTests()
        {
            var monitor = new ActivityMonitor( applyAutoConfigurations: false );
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {

                // Registers the ErrorCounter first: it will be the last one to be called, but
                // this does not prevent the PathCatcher to work: the path elements reference the group
                // so that any conclusion arriving after PathCatcher.OnClosing are available.
                ActivityMonitorErrorCounter c = new ActivityMonitorErrorCounter();
                monitor.Output.RegisterClient( c );

                // Registers the PathCatcher now: it will be called BEFORE the ErrorCounter.
                ActivityMonitorPathCatcher p = new ActivityMonitorPathCatcher();
                monitor.Output.RegisterClient( p );

                Assert.That( c.GenerateConclusion, Is.False, "False by default." );
                c.GenerateConclusion = true;
                Assert.That( c.Root.MaxLogLevel == LogLevel.None );

                monitor.Trace().Send( "T1" );
                Assert.That( !c.Root.HasWarnOrError && !c.Root.HasError );
                Assert.That( c.Root.MaxLogLevel == LogLevel.Trace );
                Assert.That( c.Root.ToString(), Is.Null );

                monitor.Warn().Send( "W1" );
                Assert.That( c.Root.HasWarnOrError && !c.Root.HasError );
                Assert.That( c.Root.MaxLogLevel == LogLevel.Warn );
                Assert.That( c.Root.ToString(), Is.Not.Null.And.Not.Empty );

                monitor.Error().Send( "E2" );
                Assert.That( c.Root.HasWarnOrError && c.Root.HasError );
                Assert.That( c.Root.ErrorCount == 1 );
                Assert.That( c.Root.MaxLogLevel == LogLevel.Error );
                Assert.That( c.Root.ToString(), Is.Not.Null.And.Not.Empty );

                c.Root.ClearError();
                Assert.That( c.Root.HasWarnOrError && !c.Root.HasError );
                Assert.That( c.Root.ErrorCount == 0 );
                Assert.That( c.Root.MaxLogLevel == LogLevel.Warn );
                Assert.That( c.Root.ToString(), Is.Not.Null );

                c.Root.ClearWarn();
                Assert.That( !c.Root.HasWarnOrError && !c.Root.HasError );
                Assert.That( c.Root.MaxLogLevel == LogLevel.Info );
                Assert.That( c.Root.ToString(), Is.Null );

                using( monitor.OpenTrace().Send( "G1" ) )
                {
                    string errorMessage;
                    using( monitor.OpenInfo().Send( "G2" ) )
                    {
                        monitor.Error().Send( "E1" );
                        monitor.Fatal().Send( "F1" );
                        Assert.That( c.Root.HasWarnOrError && c.Root.HasError );
                        Assert.That( c.Root.ErrorCount == 1 && c.Root.FatalCount == 1 );
                        Assert.That( c.Root.WarnCount == 0 );

                        using( monitor.OpenInfo().Send( "G3" ) )
                        {
                            Assert.That( !c.Current.HasWarnOrError && !c.Current.HasError );
                            Assert.That( c.Current.ErrorCount == 0 && c.Current.FatalCount == 0 && c.Current.WarnCount == 0 );

                            monitor.Error().Send( "An error..." );

                            Assert.That( c.Current.HasWarnOrError && c.Current.HasError );
                            Assert.That( c.Current.ErrorCount == 1 && c.Current.FatalCount == 0 && c.Current.WarnCount == 0 );

                            errorMessage = String.Join( "|", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) );
                            Assert.That( errorMessage, Is.EqualTo( "G1-|G2-|G3-|An error...-" ), "Groups are not closed: no conclusion exist yet." );
                        }
                        errorMessage = String.Join( "|", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) );
                        Assert.That( errorMessage, Is.EqualTo( "G1-|G2-|G3-1 Error|An error...-" ), "G3 is closed: its conclusion is available." );
                    }
                    errorMessage = String.Join( "|", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) );
                    Assert.That( errorMessage, Is.EqualTo( "G1-|G2-1 Fatal error, 2 Errors|G3-1 Error|An error...-" ) );
                    monitor.Error().Send( "E3" );
                    monitor.Fatal().Send( "F2" );
                    monitor.Warn().Send( "W2" );
                    Assert.That( c.Root.HasWarnOrError && c.Root.HasError );
                    Assert.That( c.Root.FatalCount == 2 );
                    Assert.That( c.Root.ErrorCount == 3 );
                    Assert.That( c.Root.MaxLogLevel == LogLevel.Fatal );
                }
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) ), Is.EqualTo( "G1-2 Fatal errors, 3 Errors, 1 Warning>F2-" ) );
                Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) ), Is.EqualTo( "G1-2 Fatal errors, 3 Errors, 1 Warning>W2-" ) );
            }
        }

        [Test]
        public void SimpleCollectorTest()
        {
            IActivityMonitor d = new ActivityMonitor();
            var c = new ActivityMonitorSimpleCollector();
            d.Output.RegisterClient( c );
            d.Warn().Send( "1" );
            d.Error().Send( "2" );
            d.Fatal().Send( "3" );
            d.Trace().Send( "4" );
            d.Info().Send( "5" );
            d.Warn().Send( "6" );
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "2,3" ) );

            c.MinimalFilter = LogLevelFilter.Fatal;
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "3" ) );

            c.MinimalFilter = LogLevelFilter.Off;
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "" ) );

            c.MinimalFilter = LogLevelFilter.Warn;
            using( d.OpenWarn().Send( "1" ) )
            {
                d.Error().Send( "2" );
                using( d.OpenFatal().Send( "3" ) )
                {
                    d.Trace().Send( "4" );
                    d.Info().Send( "5" );
                }
            }
            d.Warn().Send( "6" );
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "1,2,3,6" ) );

            c.MinimalFilter = LogLevelFilter.Fatal;
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "3" ) );
        }

        [Test]
        public void FilteredTextWriterTests()
        {
            StringBuilder sb = new StringBuilder();

            IActivityMonitor d = new ActivityMonitor();
            d.SetMinimalFilter( LogFilter.Debug );

            var c = new ActivityMonitorTextWriterClient(s => sb.Append(s), LogFilter.Release);
            d.Output.RegisterClient( c );

            d.Trace().Send( "NO SHOW" );
            d.Trace().Send( "NO SHOW" );
            using( d.OpenTrace().Send( "NO SHOW" ) )
            {
                d.Info().Send( "NO SHOW" );
                d.Info().Send( "NO SHOW" );
            }

            d.Error().Send( "Error line at root" );
            using( d.OpenInfo().Send( "NO SHOW" ) )
            {
                d.Warn().Send( "NO SHOW" );
                d.Error().Send( "Send error line inside group" );
                using( d.OpenError().Send( "Open error group" ) )
                {
                    d.Error().Send( "Send error line inside sub group" );
                }
            }

            Assert.That( sb.ToString(), Does.Not.Contain( "NO SHOW" ) );
            Assert.That( sb.ToString(), Does.Contain( "Error line at root" ) );
            Assert.That( sb.ToString(), Does.Contain( "Send error line inside group" ) );
            Assert.That( sb.ToString(), Does.Contain( "Open error group" ) );
            Assert.That( sb.ToString(), Does.Contain( "Send error line inside sub group" ) );
        }

        [Test]
        public void OnError_fires_synchronously()
        {
            var m = new ActivityMonitor( false );
            bool hasError = false;
            using( m.OnError( () => hasError = true ) )
            using( m.OpenInfo().Send( "Handling StObj objects." ) )
            {
                m.Fatal().Send( "Oops!" );
                Assert.That( hasError );
                hasError = false;
                m.OpenFatal().Send( "Oops! (Group)" ).Dispose();
                Assert.That( hasError );
                hasError = false;
            }
            hasError = false;
            m.Fatal().Send( "Oops!" );
            Assert.That( hasError, Is.False );
            
            bool hasFatal = false;
            using( m.OnError( () => hasFatal = true, () => hasError = true ) )
            {
                m.Fatal().Send( "Big Oops!" );
                Assert.That( hasFatal && !hasError );
                m.Error().Send( "Oops!" );
                Assert.That( hasFatal && hasError );
                hasFatal = hasError = false;
                m.OpenError().Send( "Oops! (Group)" ).Dispose();
                Assert.That( !hasFatal && hasError );
                m.OpenFatal().Send( "Oops! (Group)" ).Dispose();
                Assert.That( hasFatal && hasError );
                hasFatal = hasError = false;
            }
            m.Fatal().Send( "Oops!" );
            Assert.That( hasFatal || hasError, Is.False );
        }

        [Test]
        public void AsyncSetMininimalFilter()
        {
            ActivityMonitor m = new ActivityMonitor( false );
            var tester = m.Output.RegisterClient( new ActivityMonitorClientTester() );

            Assert.That( m.ActualFilter, Is.EqualTo( LogFilter.Undefined ) );
            tester.AsyncSetMinimalFilterBlock( LogFilter.Monitor );
            Assert.That( m.ActualFilter, Is.EqualTo( LogFilter.Monitor ) );
        }

        class CheckAlwaysFilteredClient : ActivityMonitorClient
        {
            protected override void OnOpenGroup( IActivityLogGroup group )
            {
                Assert.That( (group.GroupLevel & LogLevel.IsFiltered) != 0 );
            }

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                Assert.That( data.IsFilteredLog );
            }
        }

        [Test]
        public void Overloads()
        {
            Exception ex = new Exception( "EXCEPTION" );
            string fmt0 = "fmt", fmt1 = "fmt{0}", fmt2 = "fmt{0}{1}", fmt3 = "fmt{0}{1}{2}", fmt4 = "fmt{0}{1}{2}{3}", fmt5 = "fmt{0}{1}{2}{3}{4}", fmt6 = "fmt{0}{1}{2}{3}{4}{5}";
            string p1 = "p1", p2 = "p2", p3 = "p3", p4 = "p4", p5 = "p5", p6 = "p6";
            Func<string> onDemandText = () => "onDemand";
            Func<int,string> onDemandTextP1 = ( i ) => "onDemand" + i.ToString();
            Func<int,int,string> onDemandTextP2 = ( i, j ) => "onDemand" + i.ToString() + j.ToString();

            IActivityMonitor d = new ActivityMonitor();
            var collector = new ActivityMonitorSimpleCollector() { MinimalFilter = LogLevelFilter.Trace, Capacity = 1 };
            d.Output.RegisterClients( collector, new CheckAlwaysFilteredClient() );

            // d.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Trace, "CheckAlwaysFilteredClient works", DateTime.UtcNow, null );

            d.Trace().Send( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Trace().Send( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Trace().Send( fmt1, ex ) );
            d.Trace().Send( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Trace().Send( onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Trace().Send( onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Trace().Send( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Trace().Send( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Trace().Send( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Trace().Send( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Trace().Send( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Info().Send( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Info().Send( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Info().Send( fmt1, ex ) );
            d.Info().Send( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Info().Send( onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Info().Send( onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Info().Send( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Info().Send( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Info().Send( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Info().Send( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Info().Send( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Warn().Send( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Warn().Send( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Warn().Send( fmt1, ex ) );
            d.Warn().Send( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Warn().Send( onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Warn().Send( onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Warn().Send( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Warn().Send( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Warn().Send( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Warn().Send( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Warn().Send( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Error().Send( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Error().Send( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Error().Send( fmt1, ex ) );
            d.Error().Send( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Error().Send( onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Error().Send( onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Error().Send( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Error().Send( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Error().Send( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Error().Send( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Error().Send( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Fatal().Send( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Fatal().Send( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Fatal().Send( fmt1, ex ) );
            d.Fatal().Send( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Fatal().Send( onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Fatal().Send( onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Fatal().Send( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Fatal().Send( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Fatal().Send( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Fatal().Send( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Fatal().Send( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.OpenTrace().Send( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.OpenTrace().Send( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            d.OpenTrace().Send( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.OpenTrace().Send( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.OpenTrace().Send( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.OpenTrace().Send( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.OpenTrace().Send( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Trace().Send( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace().Send( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Info().Send( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info().Send( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Warn().Send( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn().Send( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Error().Send( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error().Send( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Fatal().Send( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal().Send( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.OpenTrace().Send( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenTrace().Send( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

        }


        [Test]
        public void OverloadsWithTraits()
        {
            Exception ex = new Exception( "EXCEPTION" );
            string fmt0 = "fmt", fmt1 = "fmt{0}", fmt2 = "fmt{0}{1}", fmt3 = "fmt{0}{1}{2}", fmt4 = "fmt{0}{1}{2}{3}", fmt5 = "fmt{0}{1}{2}{3}{4}", fmt6 = "fmt{0}{1}{2}{3}{4}{5}";
            string p1 = "p1", p2 = "p2", p3 = "p3", p4 = "p4", p5 = "p5", p6 = "p6";
            Func<string> onDemandText = () => "onDemand";
            Func<int,string> onDemandTextP1 = ( i ) => "onDemand" + i.ToString();
            Func<int,int,string> onDemandTextP2 = ( i, j ) => "onDemand" + i.ToString() + j.ToString();

            IActivityMonitor d = new ActivityMonitor();
            var collector = new ActivityMonitorSimpleCollector() { MinimalFilter = LogLevelFilter.Trace, Capacity = 1 };
            d.Output.RegisterClients( collector, new CheckAlwaysFilteredClient() );

            CKTrait tag = ActivityMonitor.Tags.Register( "TAG" );

            d.Trace().Send( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Trace().Send( tag, fmt1, ex ) );
            d.Trace().Send( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Trace().Send( tag, onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Trace().Send( tag, onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Trace().Send( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Info().Send( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Info().Send( tag, fmt1, ex ) );
            d.Info().Send( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Info().Send( tag, onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Info().Send( tag, onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Info().Send( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Warn().Send( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Warn().Send( tag, fmt1, ex ) );
            d.Warn().Send( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Warn().Send( tag, onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Warn().Send( tag, onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Warn().Send( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Error().Send( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Error().Send( tag, fmt1, ex ) );
            d.Error().Send( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Error().Send( tag, onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Error().Send( tag, onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Error().Send( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Fatal().Send( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Fatal().Send( tag, fmt1, ex ) );
            d.Fatal().Send( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Fatal().Send( tag, onDemandTextP1, 1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Fatal().Send( tag, onDemandTextP2, 1, 2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Fatal().Send( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.OpenTrace().Send( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Trace().Send( ex, tag ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace().Send( ex, tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Info().Send( ex, tag ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info().Send( ex, tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Warn().Send( ex, tag ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn().Send( ex, tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Error().Send( ex, tag ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error().Send( ex, tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Fatal().Send( ex, tag ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal().Send( ex, tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.OpenTrace().Send( ex, tag ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenTrace().Send( ex, tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

        }
    }
}
