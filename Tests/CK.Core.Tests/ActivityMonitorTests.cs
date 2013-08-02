#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\ActivityMonitorTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "ActivityMonitor" )]
    public class ActivityMonitorTests
    {
        [Test]
        public void NonMultipleRegistrationClients()
        {
            ActivityMonitor.AutoConfiguration.Clear();
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

            var bridgeToConsole = monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 3 ) );
            Assert.That( bridgeToConsole.TargetMonitor, Is.SameAs( TestHelper.ConsoleMonitor ) );

            IActivityMonitor other = new ActivityMonitor();
            Assert.Throws<InvalidOperationException>( () => other.Output.RegisterClient( bridgeToConsole ), "Bridge can be associated to only one source monitor." );
            
            monitor.Output.UnregisterClient( bridgeToConsole );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ) );
            
            Assert.DoesNotThrow( () => other.Output.RegisterClient( bridgeToConsole ), "Now we can." );

            Assert.DoesNotThrow( () => monitor.Output.UnregisterClient( bridgeToConsole ), "Already removed." );
            monitor.Output.UnregisterClient( counter );
            monitor.Output.UnregisterClient( pathCatcher );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 0 ) );
        }

        [Test]
        [Category( "ActivityMonitor" )]
        public void OutputArguments()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            Assert.Throws<ArgumentNullException>( () => monitor.Output.RegisterClient( null ) );
            Assert.Throws<ArgumentNullException>( () => monitor.Output.UnregisterClient( null ) );
        }

        [Test]
        [Category( "Console" )]
        public void BridgeArguments()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            Assert.Throws<ArgumentNullException>( () => monitor.Output.BridgeTo( null ) );
            Assert.Throws<ArgumentNullException>( () => monitor.Output.UnbridgeTo( null ) );
            IActivityMonitorOutput output = null;
            Assert.Throws<NullReferenceException>( () => output.BridgeTo( TestHelper.ConsoleMonitor ) );
            Assert.Throws<NullReferenceException>( () => output.UnbridgeTo( TestHelper.ConsoleMonitor ) );
        }

        [Test]
        [Category( "Console" )]
        public void BridgeBalance()
        {
            Assert.Throws<ArgumentNullException>( () => new ActivityMonitorBridge( null ), "Null guards." );

            IActivityMonitor monitor = new ActivityMonitor();
            var allDump = monitor.Output.RegisterClient( new StupidStringClient() );

            // The consoleString is a string dump of the console.
            // Both the console and the string dump accepts at most Info level.
            IActivityMonitor consoleString = new ActivityMonitor();
            var consoleDump = consoleString.Output.RegisterClient( new StupidStringClient() );
            consoleString.Filter = LogLevelFilter.Info;
            TestHelper.ConsoleMonitor.Filter = LogLevelFilter.Info;

            int i = 0;
            for( ; i < 60; i++ ) monitor.OpenGroup( LogLevel.Info, String.Format( "Not Bridged n°{0}", i ) );
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            monitor.Output.BridgeTo( consoleString );
            for( ; i < 62; i++ ) monitor.OpenGroup( LogLevel.Info, String.Format( "Bridged n°{0} (appear in Console)", i ) );
            for( ; i < 64; i++ ) monitor.OpenGroup( LogLevel.Trace, String.Format( "Bridged n°{0} (#NOT appear# in Console since level is Trace)", i ) );
            for( ; i < 66; i++ ) monitor.OpenGroup( LogLevel.Warn, String.Format( "Bridged n°{0} (appear in Console)", i ) );
            
            // Now close the groups, but not completely.
            int j = 0;
            for( ; j < 2; j++ ) monitor.CloseGroup( String.Format( "Close n°{0} (Close Warn appear in Console)", j ) );
            monitor.CloseGroup( String.Format( "Close n°{0} (Close Trace does #NOT appear# in Console)", j++ ) );
            
            // Removes the bridge to the console: the Trace is not closed (not opened because of Trace level), but the 2 Info are automatically closed.
            monitor.Output.UnbridgeTo( TestHelper.ConsoleMonitor );
            monitor.Output.UnbridgeTo( consoleString );
            
            string consoleText = consoleDump.ToString();
            Assert.That( consoleText, Is.Not.StringContaining( "#NOT appear#" ) );
            Assert.That( Regex.Matches( consoleText, "Close Warn appear" ).Count, Is.EqualTo( 2 ) );
            Assert.That( Regex.Matches( consoleText, R.ClosedByBridgeRemoved ).Count, Is.EqualTo( 2 ), "The 2 Info groups have been automatically closed, but not the Warn nor the 60 first groups." );

            for( ; j < 66; j++ ) monitor.CloseGroup( String.Format( "CLOSE NOT BRIDGED - {0}", j ) );
            monitor.CloseGroup( "NEVER OPENED Group" );

            string allText = allDump.ToString();
            Assert.That( allText, Is.Not.StringContaining( R.ClosedByBridgeRemoved ) );
            Assert.That( Regex.Matches( allText, "#NOT appear#" ).Count, Is.EqualTo( 3 ), "The 2 opened Warn + the only explicit close." );
            Assert.That( Regex.Matches( allText, "CLOSE NOT BRIDGED" ).Count, Is.EqualTo( 63 ), "The 60 opened groups at the beginning + the last Trace and the 2 Info." );
            Assert.That( allText, Is.Not.StringContaining( "NEVER OPENED" ) );
        }

        [Test]
        public void TagsAndFilterRestored()
        {
            ActivityMonitor monitor = new ActivityMonitor();
            using( monitor.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                monitor.AutoTags = ActivityMonitor.RegisteredTags.FindOrCreate( "Tag" );
                monitor.Filter = LogLevelFilter.Info;
                using( monitor.OpenGroup( LogLevel.Warn, "G2" ) )
                {
                    monitor.AutoTags = ActivityMonitor.RegisteredTags.FindOrCreate( "A|B|C" );
                    monitor.Filter = LogLevelFilter.Error;
                    Assert.That( monitor.AutoTags.ToString(), Is.EqualTo( "A|B|C" ) );
                    Assert.That( monitor.Filter, Is.EqualTo( LogLevelFilter.Error ) );
                }
                Assert.That( monitor.AutoTags.ToString(), Is.EqualTo( "Tag" ) );
                Assert.That( monitor.Filter, Is.EqualTo( LogLevelFilter.Info ) );
            }
            Assert.That( monitor.AutoTags, Is.SameAs( ActivityMonitor.EmptyTag ) );
            Assert.That( monitor.Filter, Is.EqualTo( LogLevelFilter.None ) );
        }

        [Test]
        [Category( "Console" )]
        public void DefaultImpl()
        {
            ActivityMonitor.AutoConfiguration.Clear();
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            monitor.Output.RegisterClients( new StupidStringClient(), new StupidXmlClient( new StringWriter() ) );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 3 ) );

            var tag1 = ActivityMonitor.RegisteredTags.FindOrCreate( "Product" );
            var tag2 = ActivityMonitor.RegisteredTags.FindOrCreate( "Sql" );
            var tag3 = ActivityMonitor.RegisteredTags.FindOrCreate( "Combined Tag|Sql|Engine V2|Product" );

            using( monitor.OpenGroup( LogLevel.None, () => "EndMainGroup", "MainGroup" ) )
            {
                using( monitor.OpenGroup( LogLevel.Trace, () => "EndMainGroup", "MainGroup" ) )
                {
                    monitor.Trace( tag1, "First" );
                    using( monitor.AutoTags( tag1 ) )
                    {
                        monitor.Trace( "Second" );
                        monitor.Trace( tag3, "Third" );
                        using( monitor.AutoTags( tag2 ) )
                        {
                            monitor.Info( "First" );
                        }
                    }
                    using( monitor.OpenGroup( LogLevel.Info, () => "Conclusion of Info Group (no newline).", "InfoGroup" ) )
                    {
                        monitor.Info( "Second" );
                        monitor.Trace( "Fourth" );

                        string warnConclusion = "Conclusion of Warn Group" + Environment.NewLine + "with more than one line int it.";
                        using( monitor.OpenGroup( LogLevel.Warn, () => warnConclusion, "WarnGroup {0} - Now = {1}", 4, DateTime.UtcNow ) )
                        {
                            monitor.Info( "Warn!" );
                            monitor.CloseGroup( "User conclusion with multiple lines." 
                                + Environment.NewLine + "It will be displayed on "
                                + Environment.NewLine + "multiple lines." );
                        }
                        monitor.CloseGroup( "Conclusions on one line are displayed separated by dash." );
                    }
                }
            }

            Console.WriteLine( monitor.Output.Clients.OfType<StupidStringClient>().Single().Writer );
            Console.WriteLine( monitor.Output.Clients.OfType<StupidXmlClient>().Single().InnerWriter );

            XPathDocument d = new XPathDocument( new StringReader( monitor.Output.Clients.OfType<StupidXmlClient>().Single().InnerWriter.ToString() ) );

            Assert.That( d.CreateNavigator().SelectDescendants( "Info", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 3 ) );
            Assert.That( d.CreateNavigator().SelectDescendants( "Trace", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 2 ) );

        }

        [Test]
        [Category( "Console" )]
        public void DumpException()
        {
            IActivityMonitor l = new ActivityMonitor();
            var rawLog = new StupidStringClient();
            l.Output.RegisterClient( rawLog );
            l.Output.BridgeTo( TestHelper.ConsoleMonitor );

            var wLogLovely = new StringWriter();
            var logLovely = new ActivityMonitorTextWriterClient( wLogLovely );
            l.Output.RegisterClient( logLovely );

            l.Error( new Exception( "EXERROR-1" ) );
            using( l.OpenGroup( LogLevel.Fatal, new Exception( "EXERROR-2" ), "EXERROR-TEXT2" ) )
            {
                try
                {
                    throw new Exception( "EXERROR-3" );
                }
                catch( Exception ex )
                {
                    l.Trace( ex, "EXERROR-TEXT3" );
                }
            }
            Assert.That( rawLog.ToString(), Is.StringContaining( "EXERROR-1" ) );
            Assert.That( rawLog.ToString(), Is.StringContaining( "EXERROR-2" ).And.StringContaining( "EXERROR-TEXT2" ) );
            Assert.That( rawLog.ToString(), Is.StringContaining( "EXERROR-3" ).And.StringContaining( "EXERROR-TEXT3" ) );

            string text = wLogLovely.ToString();
            Assert.That( text, Is.StringContaining( "EXERROR-1" ) );
            Assert.That( text, Is.StringContaining( "EXERROR-2" ).And.StringContaining( "EXERROR-TEXT2" ) );
            Assert.That( text, Is.StringContaining( "EXERROR-3" ).And.StringContaining( "EXERROR-TEXT3" ) );
            Assert.That( text, Is.StringContaining( "Stack:" ) );
        }

        [Test]
        [Category( "Console" )]
        public void DumpAggregatedException()
        {
            IActivityMonitor l = new ActivityMonitor();
            l.Output.BridgeTo( TestHelper.ConsoleMonitor );

            var wLogLovely = new StringWriter();
            var logLovely = new ActivityMonitorTextWriterClient( wLogLovely );
            l.Output.RegisterClient( logLovely );


            l.Error( new Exception( "EXERROR-1" ) );
            using( l.OpenGroup( LogLevel.Fatal, new Exception( "EXERROR-2" ), "EXERROR-TEXT2" ) )
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
                    l.Error( ex, "EXERROR-TEXT3" );
                }
            }

            string text = wLogLovely.ToString();
            Assert.That( text, Is.StringContaining( "EXERROR-Aggreg-1" ) );
            Assert.That( text, Is.StringContaining( "EXERROR-Aggreg-2-1" ) );
            Assert.That( text, Is.StringContaining( "EXERROR-Aggreg-2-2" ) );
            Assert.That( text, Is.StringContaining( "EXERROR-Aggreg-3" ) );
        }

        [Test]
        [Category( "Console" )]
        public void MultipleClose()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );

            var log1 = monitor.Output.RegisterClient( new StupidStringClient() );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ) );

            using( monitor.OpenGroup( LogLevel.Trace, () => "End First", "First" ) )
            {
                monitor.CloseGroup( "Pouf" );
                using( monitor.OpenGroup( LogLevel.Warn, "A group at level 0!" ) )
                {
                    monitor.CloseGroup( "Close it." );
                    monitor.CloseGroup( "Close it again. (not seen)" );
                }
            }
            string logged = log1.Writer.ToString();
            Assert.That( logged, Is.StringContaining( "Pouf" ).And.StringContaining( "End First" ), "Multiple conclusions." );
            Assert.That( logged, Is.Not.StringContaining( "Close it again" ), "Close forgets other closes..." );
        }

        [Test]
        [Category( "Console" )]
        public void FilterLevel()
        {
            IActivityMonitor l = new ActivityMonitor();
            l.Output.BridgeTo( TestHelper.ConsoleMonitor );

            var log = l.Output.RegisterClient( new StupidStringClient() );
            using( l.Filter( LogLevelFilter.Error ) )
            {
                l.Trace( "NO SHOW" );
                l.Info( "NO SHOW" );
                l.Warn( "NO SHOW" );
                l.Error( "Error n°1" );
                using( l.Filter( LogLevelFilter.Warn ) )
                {
                    l.Trace( "NO SHOW" );
                    l.Info( "NO SHOW" );
                    l.Warn( "Warn n°1" );
                    l.Error( "Error n°2" );
                    using( l.OpenGroup( LogLevel.Info, "GroupInfo" ) )
                    {
                        Assert.That( l.Filter, Is.EqualTo( LogLevelFilter.Warn ), "Groups does not change the current filter level." );
                        l.Trace( "NO SHOW" );
                        l.Info( "NO SHOW" );
                        l.Warn( "Warn n°2" );
                        l.Error( "Error n°3" );
                        // Changing the level inside a Group.
                        l.Filter = LogLevelFilter.Fatal;
                        l.Error( "NO SHOW" );
                        l.Fatal( "Fatal n°1" );
                    }
                    Assert.That( l.Filter, Is.EqualTo( LogLevelFilter.Warn ), "But Groups restores the original filter level when closed." );
                    l.Trace( "NO SHOW" );
                    l.Info( "NO SHOW" );
                    l.Warn( "Warn n°3" );
                    l.Error( "Error n°4" );
                    l.Fatal( "Fatal n°2" );
                }
                l.Trace( "NO SHOW" );
                l.Info( "NO SHOW" );
                l.Warn( "NO SHOW" );
                l.Error( "Error n°5" );
            }
            Assert.That( log.Writer.ToString(), Is.Not.StringContaining( "NO SHOW" ) );
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Error n°1" )
                                                    .And.StringContaining( "Error n°2" )
                                                    .And.StringContaining( "Error n°3" )
                                                    .And.StringContaining( "Error n°4" )
                                                    .And.StringContaining( "Error n°5" ) );
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Warn n°1" )
                                                    .And.StringContaining( "Warn n°2" )
                                                    .And.StringContaining( "Warn n°3" ) );
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Fatal n°1" )
                                                    .And.StringContaining( "Fatal n°2" ) );
        }

        [Test]
        [Category( "Console" )]
        public void CloseMismatch()
        {
            IActivityMonitor l = new ActivityMonitor();
            var log = l.Output.RegisterClient( new StupidStringClient() );
            {
                IDisposable g0 = l.OpenGroup( LogLevel.Trace, "First" );
                IDisposable g1 = l.OpenGroup( LogLevel.Trace, "Second" );
                IDisposable g2 = l.OpenGroup( LogLevel.Trace, "Third" );

                g1.Dispose();
                l.Trace( "Inside First" );
                g0.Dispose();
                l.Trace( "At root" );

                var end = "Trace: Inside First" + Environment.NewLine + "-" + Environment.NewLine + "Trace: At root";
                Assert.That( log.Writer.ToString(), Is.StringEnding( end ) );
            }
            {
                // g2 is closed after g1.
                IDisposable g0 = l.OpenGroup( LogLevel.Trace, "First" );
                IDisposable g1 = l.OpenGroup( LogLevel.Trace, "Second" );
                IDisposable g2 = l.OpenGroup( LogLevel.Trace, "Third" );
                log.Writer.GetStringBuilder().Clear();
                g1.Dispose();
                // g2 has already been disposed by g1. 
                // Nothing changed.
                g2.Dispose();
                l.Trace( "Inside First" );
                g0.Dispose();
                l.Trace( "At root" );

                var end = "Trace: Inside First" + Environment.NewLine + "-" + Environment.NewLine + "Trace: At root";
                Assert.That( log.Writer.ToString(), Is.StringEnding( end ) );
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
            l.Output.BridgeTo( TestHelper.ConsoleMonitor );
            l.Output.RegisterClient( new ActivityMonitorErrorCounter( true ) );
            var log = l.Output.RegisterClient( new StupidStringClient() );

            // No explicit close conclusion: Success!
            using( l.OpenGroup( LogLevel.Trace, () => "From Opener", "G" ) )
            {
                l.Error( "Pouf" );
                l.CloseGroup( new ObjectAsConclusion() );
            }
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Explicit User Conclusion, From Opener, 1 Error" ) );
        }
        
        [Test]
        public void ErrorAgurments()
        {
            IActivityMonitor l = new ActivityMonitor();
            l.Output.BridgeTo( TestHelper.ConsoleMonitor );
            Assert.Throws<ArgumentException>( () => l.UnfilteredLog( ActivityMonitor.EmptyTag, LogLevel.Error, "Text may be null", DateTime.Now, null ), "DateTime must be Utc." );
            Assert.Throws<ArgumentException>( () => l.OpenGroup( ActivityMonitor.EmptyTag, LogLevel.Error, null, "Text may be null", DateTime.Now, null ), "DateTime must be Utc." );
        }

        [Test]
        public void PathCatcherToStringPath()
        {
            var monitor = new ActivityMonitor();
            ActivityMonitorPathCatcher p = monitor.Output.RegisterClient( new ActivityMonitorPathCatcher() );

            using( monitor.OpenGroup( LogLevel.Trace, "!T" ) )
            using( monitor.OpenGroup( LogLevel.Info, "!I" ) )
            using( monitor.OpenGroup( LogLevel.Warn, "!W" ) )
            using( monitor.OpenGroup( LogLevel.Error, "!E" ) )
            using( monitor.OpenGroup( LogLevel.Fatal, "!F" ) )
            {
                Assert.That( p.DynamicPath.ToStringPath(), Is.StringContaining( "!T" ).And.StringContaining( "!I" ).And.StringContaining( "!W" ).And.StringContaining( "!E" ).And.StringContaining( "!F" ) );
            }
            var path = p.DynamicPath;
            path = null;
            Assert.That( path.ToStringPath(), Is.Empty );
        }

        [Test]
        [Category( "Console" )]
        public void PathCatcherTests()
        {
            var monitor = new ActivityMonitor();
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            
            ActivityMonitorPathCatcher p = new ActivityMonitorPathCatcher();
            monitor.Output.RegisterClient( p );

            monitor.Trace( "Trace n°1" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Trace|Trace n°1" ) );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

            monitor.Trace( "Trace n°2" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Trace|Trace n°2" ) );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

            monitor.Warn( "W1" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1" ) );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1" ) );

            monitor.Error( "E2" );
            monitor.Warn( "W1bis" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1bis" ) );
            Assert.That( p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Error|E2" ) );
            Assert.That( p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1bis" ) );

            p.ClearLastWarnPath();
            Assert.That( p.LastErrorPath, Is.Not.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

            p.ClearLastErrorPath();
            Assert.That( p.LastErrorPath, Is.Null );

            using( monitor.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                using( monitor.OpenGroup( LogLevel.Info, "G2" ) )
                {
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                    Assert.That( p.LastErrorPath, Is.Null );
                    using( monitor.OpenGroup( LogLevel.Trace, "G3" ) )
                    {
                        using( monitor.OpenGroup( LogLevel.Info, "G4" ) )
                        {
                            monitor.Warn( "W1" );

                            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>G4>W1" ) );

                            monitor.Info( 
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
                                monitor.Trace( ex );
                                Assert.That( p.DynamicPath.ToStringPath().Length > 0 );
                            }

                            Assert.That( p.LastErrorPath, Is.Null );
                            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );
                        }
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.ToString() ) ), Is.EqualTo( "G1>G2>G3>G4" ) );
                        Assert.That( p.LastErrorPath, Is.Null );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );

                        monitor.Error( "E1" );
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>E1" ) );
                        Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    }
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                }
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                using( monitor.OpenGroup( LogLevel.Trace, "G2Bis" ) )
                {
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2Bis" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );

                    monitor.Warn( "W2" );
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2Bis>W2" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Trace|G2Bis>Warn|W2" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                }
                monitor.Fatal( "F1" );
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>F1" ) );
                Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );
            }

            // Extraneous closing are ignored.
            monitor.CloseGroup(  null );

            monitor.Warn( "W3" );
            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "W3" ) );
            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Warn|W3" ) );
            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );

            // Extraneous closing are ignored.
            monitor.CloseGroup( null );
            
            monitor.Warn( "W4" );
            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "W4" ) );
            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Warn|W4" ) );
            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );

            p.ClearLastWarnPath( true );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

        }

        [Test]
        [Category( "Console" )]
        public void ErrorCounterTests()
        {
            var monitor = new ActivityMonitor();
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );

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

            monitor.Trace( "T1" );
            Assert.That( !c.Root.HasWarnOrError && !c.Root.HasError );
            Assert.That( c.Root.MaxLogLevel == LogLevel.Trace );
            Assert.That( c.Root.ToString(), Is.Null );

            monitor.Warn( "W1" );
            Assert.That( c.Root.HasWarnOrError && !c.Root.HasError );
            Assert.That( c.Root.MaxLogLevel == LogLevel.Warn );
            Assert.That( c.Root.ToString(), Is.Not.Null.And.Not.Empty );

            monitor.Error( "E2" );
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

            using( monitor.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                string errorMessage;
                using( monitor.OpenGroup( LogLevel.Info, "G2" ) )
                {
                    monitor.Error( "E1" );
                    monitor.Fatal( "F1" );
                    Assert.That( c.Root.HasWarnOrError && c.Root.HasError );
                    Assert.That( c.Root.ErrorCount == 1 && c.Root.FatalCount == 1 );
                    Assert.That( c.Root.WarnCount == 0 );

                    using( monitor.OpenGroup( LogLevel.Info, "G3" ) )
                    {
                        Assert.That( !c.Current.HasWarnOrError && !c.Current.HasError );
                        Assert.That( c.Current.ErrorCount == 0 && c.Current.FatalCount == 0 && c.Current.WarnCount == 0 );
                        
                        monitor.Error( "An error..." );

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
                monitor.Error( "E3" );
                monitor.Fatal( "F2" );
                monitor.Warn( "W2" );
                Assert.That( c.Root.HasWarnOrError && c.Root.HasError );
                Assert.That( c.Root.FatalCount == 2 );
                Assert.That( c.Root.ErrorCount == 3 );
                Assert.That( c.Root.MaxLogLevel == LogLevel.Fatal );
            }
            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) ), Is.EqualTo( "G1-2 Fatal errors, 3 Errors, 1 Warning>F2-" ) );
            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) ), Is.EqualTo( "G1-2 Fatal errors, 3 Errors, 1 Warning>W2-" ) );
        }

        [Test]
        public void SimpleCollectorTest()
        {
            IActivityMonitor d = new ActivityMonitor();
            var c = new ActivityMonitorSimpleCollector();
            d.Output.RegisterClient( c );
            d.Warn( "1" );
            d.Error( "2" );
            d.Fatal( "3" );
            d.Trace( "4" );
            d.Info( "5" );
            d.Warn( "6" );
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "2,3" ) );

            c.LevelFilter = LogLevelFilter.Fatal;
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "3" ) );

            c.LevelFilter = LogLevelFilter.Off;
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "" ) );
            
            c.LevelFilter = LogLevelFilter.Warn;
            using( d.OpenGroup( LogLevel.Warn, "1" ) )
            {
                d.Error( "2" );
                using( d.OpenGroup( LogLevel.Fatal, "3" ) )
                {
                    d.Trace( "4" );
                    d.Info( "5" );
                }
            }
            d.Warn( "6" );
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "1,2,3,6" ) );

            c.LevelFilter = LogLevelFilter.Fatal;
            Assert.That( String.Join( ",", c.Entries.Select( e => e.Text ) ), Is.EqualTo( "3" ) );
        }

        [Test]
        public void CatchTests()
        {
            IActivityMonitor d = new ActivityMonitor();
            
            d.Error( "Pouf" );
            using( d.CatchCounter( e => Assert.That( e == 2 ) ) )
            using( d.CatchCounter( ( f, e ) => Assert.That( f == 1 && e == 1 ) ) )
            using( d.CatchCounter( ( f, e, w ) => Assert.That( f == 1 && e == 1 && w == 1 ) ) )
            using( d.Catch( e => Assert.That( String.Join( ",", e.Select( t => t.Text ) ) == "One,Two" ) ) )
            {
                d.Error( "One" );
                d.Warn( "Warn" );
                d.Fatal( "Two" );
            }
            d.Error( "Out..." );
            using( d.CatchCounter( e => Assert.That( e == 2 ) ) )
            using( d.CatchCounter( ( f, e ) => Assert.That( f == 1 && e == 1 ) ) )
            using( d.CatchCounter( ( f, e, w ) => Assert.That( f == 1 && e == 1 && w == 1 ) ) )
            using( d.Catch( e => e.Single( t => t.Text == "Two" ), LogLevelFilter.Fatal ) )
            {
                d.Error( "One" );
                d.Warn( "Warn" );
                d.Fatal( "Two" );
            }

            using( d.CatchCounter( e => Assert.Fail( "No Error occured." ) ) )
            using( d.CatchCounter( ( f, e ) => Assert.Fail( "No Error occured." ) ) )
            using( d.CatchCounter( ( f, e, w ) => Assert.That( f == 0 && e == 0 && w == 1 ) ) )
            using( d.Catch( e => Assert.Fail( "No Error occured." ) ) )
            {
                d.Trace( "One" );
                d.Warn( "Warn" );
            }

            using( d.CatchCounter( e => Assert.That( e == 1  ) ) )
            using( d.CatchCounter( ( f, e ) => Assert.That( f == 0 && e == 1 ) ) )
            using( d.CatchCounter( ( f, e, w ) => Assert.That( f == 0 && e == 1 && w == 1 ) ) )
            using( d.Catch( e => Assert.Fail( "No Fatal occured." ), LogLevelFilter.Fatal ) )
            {
                d.Error( "One" );
                d.Warn( "Warn" );
            }

            Assert.Throws<ArgumentNullException>( () => d.Catch( null ) );
            Action<int> f1Null = null;
            Assert.Throws<ArgumentNullException>( () => d.CatchCounter( f1Null ) );
            Action<int,int> f2Null = null;
            Assert.Throws<ArgumentNullException>( () => d.CatchCounter( f2Null ) );
            Action<int,int,int> f3Null = null;
            Assert.Throws<ArgumentNullException>( () => d.CatchCounter( f3Null ) );
            d = null;
            Assert.Throws<NullReferenceException>( () => d.Catch( e => Console.Write("") ) );
            Assert.Throws<NullReferenceException>( () => d.CatchCounter( eOrF => Console.Write( "" ) ) );
            Assert.Throws<NullReferenceException>( () => d.CatchCounter( ( f, e ) => Console.Write( "" ) ) );
            Assert.Throws<NullReferenceException>( () => d.CatchCounter( ( f, e, w ) => Console.Write( "" ) ) );

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
            var collector = new ActivityMonitorSimpleCollector() { LevelFilter = LogLevelFilter.Trace, Capacity = 1 };
            d.Output.RegisterClient( collector );

            d.Trace( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Trace( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Trace( fmt1, ex ) );
            d.Trace( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Trace( 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Trace( 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Trace( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Trace( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Trace( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Trace( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Trace( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Info( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Info( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Info( fmt1, ex ) );
            d.Info( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Info( 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Info( 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Info( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Info( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Info( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Info( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Info( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Warn( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Warn( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Warn( fmt1, ex ) );
            d.Warn( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Warn( 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Warn( 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Warn( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Warn( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Warn( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Warn( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Warn( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Error( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Error( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Error( fmt1, ex ) );
            d.Error( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Error( 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Error( 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Error( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Error( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Error( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Error( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Error( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Fatal( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Fatal( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            Assert.Throws<ArgumentException>( () => d.Fatal( fmt1, ex ) );
            d.Fatal( onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Fatal( 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Fatal( 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Fatal( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Fatal( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Fatal( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Fatal( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Fatal( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.OpenGroup( LogLevel.Trace, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.OpenGroup( LogLevel.Trace, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            d.OpenGroup( LogLevel.Trace, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.OpenGroup( LogLevel.Trace, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.OpenGroup( LogLevel.Trace, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.OpenGroup( LogLevel.Trace, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.OpenGroup( LogLevel.Trace, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Trace( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Trace( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Info( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Info( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Warn( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Warn( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Error( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Error( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.Fatal( ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.Fatal( ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

            d.OpenGroup( LogLevel.Trace, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );
            d.OpenGroup( LogLevel.Trace, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );

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
            var collector = new ActivityMonitorSimpleCollector() { LevelFilter = LogLevelFilter.Trace, Capacity = 1 };
            d.Output.RegisterClient( collector );

            CKTrait tag = ActivityMonitor.RegisteredTags.FindOrCreate( "TAG" );

            d.Trace( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Trace( tag, fmt1, ex ) );
            d.Trace( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Trace( tag, 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Trace( tag, 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Trace( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Info( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Info( tag, fmt1, ex ) );
            d.Info( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Info( tag, 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Info( tag, 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Info( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Warn( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Warn( tag, fmt1, ex ) );
            d.Warn( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Warn( tag, 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Warn( tag, 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Warn( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Error( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Error( tag, fmt1, ex ) );
            d.Error( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Error( tag, 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Error( tag, 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Error( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Fatal( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            Assert.Throws<ArgumentException>( () => d.Fatal( tag, fmt1, ex ) );
            d.Fatal( tag, onDemandText ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand" ) );
            d.Fatal( tag, 1, onDemandTextP1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand1" ) );
            d.Fatal( tag, 1, 2, onDemandTextP2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "onDemand12" ) );
            d.Fatal( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.OpenGroup( tag, LogLevel.Trace, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Trace( tag, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Info( tag, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Warn( tag, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Error( tag, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Fatal( tag, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.OpenGroup( tag, LogLevel.Trace, ex ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "EXCEPTION" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.OpenGroup( tag, LogLevel.Trace, ex, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Exception, Is.SameAs( ex ) );Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

        }
    }
}
