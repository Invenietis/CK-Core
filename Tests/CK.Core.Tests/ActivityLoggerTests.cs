#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\ActivityLoggerTests.cs) is part of CiviKey. 
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
    [Category( "ActivityLogger" )]
    public class ActivityLoggerTests
    {
        [Test]
        public void NonRemovableOrLockedClients()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            Assert.Throws<InvalidOperationException>( () => logger.Output.UnregisterClient( logger.ErrorCounter ), "Default Counter can not be unregistered." );
            Assert.Throws<InvalidOperationException>( () => logger.Output.UnregisterClient( logger.PathCatcher ), "Default PathCatcher can not be unregistered." );
            Assert.Throws<InvalidOperationException>( () => logger.Output.UnregisterClient( logger.Tap ), "Default Tap can not be unregistered." );

            var counter = new ActivityLoggerErrorCounter();
            logger.Output.RegisterClient( counter );
            Assert.Throws<InvalidOperationException>( () => TestHelper.Logger.Output.RegisterClient( counter ), "Counter can be registered in one source at a time." );

            var tap = new ActivityLoggerTap();
            logger.Output.RegisterClient( tap );
            Assert.Throws<InvalidOperationException>( () => TestHelper.Logger.Output.RegisterClient( tap ), "Tap can be registered in one source at a time." );

            var pathCatcher = new ActivityLoggerPathCatcher();
            logger.Output.RegisterClient( pathCatcher );
            Assert.Throws<InvalidOperationException>( () => TestHelper.Logger.Output.RegisterClient( pathCatcher ), "PathCatcher can be registered in one source at a time." );

            var bridgeToConsole = logger.Output.BridgeTo( TestHelper.Logger );
            Assert.That( bridgeToConsole.TargetLogger, Is.SameAs( TestHelper.Logger ) );

            IActivityLogger other = new ActivityLogger();
            Assert.Throws<InvalidOperationException>( () => other.Output.RegisterClient( bridgeToConsole ), "Bridge can be associated to only one source logger." );
            logger.Output.UnregisterClient( bridgeToConsole );
            Assert.DoesNotThrow( () => other.Output.RegisterClient( bridgeToConsole ), "Now we can." );
        }

        [Test]
        [Category( "Console" )]
        public void BridgeBalance()
        {
            Assert.Throws<ArgumentNullException>( () => new ActivityLoggerBridge( null ), "Null guards." );

            IDefaultActivityLogger logger = new DefaultActivityLogger();
            var allDump = new StringImpl();
            logger.Tap.Register( allDump );

            // The consoleString is a string dump of the console.
            // Both the console and the string dump accepts at most Info level.
            IDefaultActivityLogger consoleString = new DefaultActivityLogger();
            var consoleDump = new StringImpl();
            consoleString.Tap.Register( consoleDump );
            consoleString.Filter = LogLevelFilter.Info;
            TestHelper.Logger.Filter = LogLevelFilter.Info;

            int i = 0;
            for( ; i < 60; i++ ) logger.OpenGroup( LogLevel.Info, String.Format( "Not Bridged n°{0}", i ) );
            logger.Output.BridgeTo( TestHelper.Logger );
            logger.Output.BridgeTo( consoleString );
            for( ; i < 62; i++ ) logger.OpenGroup( LogLevel.Info, String.Format( "Bridged n°{0} (appear in Console)", i ) );
            for( ; i < 64; i++ ) logger.OpenGroup( LogLevel.Trace, String.Format( "Bridged n°{0} (#NOT appear# in Console since level is Trace)", i ) );
            for( ; i < 66; i++ ) logger.OpenGroup( LogLevel.Warn, String.Format( "Bridged n°{0} (appear in Console)", i ) );
            
            // Now close the groups, but not completely.
            int j = 0;
            for( ; j < 2; j++ ) logger.CloseGroup( String.Format( "Close n°{0} (Close Warn appear in Console)", j ) );
            logger.CloseGroup( String.Format( "Close n°{0} (Close Trace does #NOT appear# in Console)", j++ ) );
            
            // Removes the bridge to the console: the Trace is not closed (not opened because of Trace level), but the 2 Info are automatically closed.
            logger.Output.UnbridgeTo( TestHelper.Logger );
            logger.Output.UnbridgeTo( consoleString );
            
            string consoleText = consoleDump.ToString();
            Assert.That( consoleText, Is.Not.StringContaining( "#NOT appear#" ) );
            Assert.That( Regex.Matches( consoleText, "Close Warn appear" ).Count, Is.EqualTo( 2 ) );
            Assert.That( Regex.Matches( consoleText, R.ClosedByBridgeRemoved ).Count, Is.EqualTo( 2 ), "The 2 Info groups have been automatically closed, but not the Warn nor the 60 first groups." );

            for( ; j < 66; j++ ) logger.CloseGroup( String.Format( "CLOSE NOT BRIDGED - {0}", j ) );
            logger.CloseGroup( "NEVER OPENED Group" );

            string allText = allDump.ToString();
            Assert.That( allText, Is.Not.StringContaining( R.ClosedByBridgeRemoved ) );
            Assert.That( Regex.Matches( allText, "#NOT appear#" ).Count, Is.EqualTo( 3 ), "The 2 opened Warn + the only explicit close." );
            Assert.That( Regex.Matches( allText, "CLOSE NOT BRIDGED" ).Count, Is.EqualTo( 63 ), "The 60 opened groups at the beginning + the last Trace and the 2 Info." );
            Assert.That( allText, Is.Not.StringContaining( "NEVER OPENED" ) );
        }

        [Test]
        public void TagsAndFilterRestored()
        {
            ActivityLogger logger = new ActivityLogger();
            using( logger.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "Tag" );
                logger.Filter = LogLevelFilter.Info;
                using( logger.OpenGroup( LogLevel.Warn, "G2" ) )
                {
                    logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "A|B|C" );
                    logger.Filter = LogLevelFilter.Error;
                    Assert.That( logger.AutoTags.ToString(), Is.EqualTo( "A|B|C" ) );
                    Assert.That( logger.Filter, Is.EqualTo( LogLevelFilter.Error ) );
                }
                Assert.That( logger.AutoTags.ToString(), Is.EqualTo( "Tag" ) );
                Assert.That( logger.Filter, Is.EqualTo( LogLevelFilter.Info ) );
            }
            Assert.That( logger.AutoTags, Is.SameAs( ActivityLogger.EmptyTag ) );
            Assert.That( logger.Filter, Is.EqualTo( LogLevelFilter.None ) );
        }

        [Test]
        [Category( "Console" )]
        public void DefaultImpl()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Output.BridgeTo( TestHelper.Logger );
            logger.Tap.Register( new StringImpl() ).Register( new XmlImpl( new StringWriter() ) );
            Assert.That( logger.Tap.RegisteredSinks.Count, Is.EqualTo( 2 ) );

            var tag1 = ActivityLogger.RegisteredTags.FindOrCreate( "Product" );
            var tag2 = ActivityLogger.RegisteredTags.FindOrCreate( "Sql" );
            var tag3 = ActivityLogger.RegisteredTags.FindOrCreate( "Combined Tag|Sql|Engine V2|Product" );

            using( logger.OpenGroup( LogLevel.None, () => "EndMainGroup", "MainGroup" ) )
            {
                using( logger.OpenGroup( LogLevel.Trace, () => "EndMainGroup", "MainGroup" ) )
                {
                    logger.Trace( tag1, "First" );
                    using( logger.Tags( tag1 ) )
                    {
                        logger.Trace( "Second" );
                        logger.Trace( tag3, "Third" );
                        using( logger.Tags( tag2 ) )
                        {
                            logger.Info( "First" );
                        }
                    }
                    using( logger.OpenGroup( LogLevel.Info, () => "Conclusion of Info Group (no newline).", "InfoGroup" ) )
                    {
                        logger.Info( "Second" );
                        logger.Trace( "Fourth" );

                        string warnConclusion = "Conclusion of Warn Group" + Environment.NewLine + "with more than one line int it.";
                        using( logger.OpenGroup( LogLevel.Warn, () => warnConclusion, "WarnGroup {0} - Now = {1}", 4, DateTime.UtcNow ) )
                        {
                            logger.Info( "Warn!" );
                            logger.CloseGroup( "User conclusion with multiple lines." 
                                + Environment.NewLine + "It will be displayed on "
                                + Environment.NewLine + "multiple lines." );
                        }
                        logger.CloseGroup( "Conclusions on one line are displayed separated by dash." );
                    }
                }
            }

            Console.WriteLine( logger.Tap.RegisteredSinks.OfType<StringImpl>().Single().Writer );
            Console.WriteLine( logger.Tap.RegisteredSinks.OfType<XmlImpl>().Single().InnerWriter );

            XPathDocument d = new XPathDocument( new StringReader( logger.Tap.RegisteredSinks.OfType<XmlImpl>().Single().InnerWriter.ToString() ) );

            Assert.That( d.CreateNavigator().SelectDescendants( "Info", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 3 ) );
            Assert.That( d.CreateNavigator().SelectDescendants( "Trace", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 2 ) );

        }

        [Test]
        [Category( "Console" )]
        public void MultipleClose()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Output.BridgeTo( TestHelper.Logger );

            var log1 = new StringImpl();
            logger.Tap.Register( log1 );

            Assert.That( logger.Tap.RegisteredSinks.Count, Is.EqualTo( 1 ) );

            using( logger.OpenGroup( LogLevel.Trace, () => "End First", "First" ) )
            {
                logger.CloseGroup( "Pouf" );
                using( logger.OpenGroup( LogLevel.Warn, "A group at level 0!" ) )
                {
                    logger.CloseGroup( "Close it." );
                    logger.CloseGroup( "Close it again. (not seen)" );
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
            IDefaultActivityLogger l = new DefaultActivityLogger();
            l.Output.BridgeTo( TestHelper.Logger );
            
            var log = new StringImpl();
            l.Tap.Register( log );
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
            IDefaultActivityLogger l = new DefaultActivityLogger();
            var log = new StringImpl();
            l.Tap.Register( log );
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

        [Test]
        [Category( "Console" )]
        public void MultipleConclusions()
        {
            IDefaultActivityLogger l = new DefaultActivityLogger();
            l.Output.BridgeTo( TestHelper.Logger );
            
            var log = new StringImpl();
            l.Tap.Register( log );

            // No explicit close conclusion: Success!
            using( l.OpenGroup( LogLevel.Trace, () => "From Opener", "G" ) )
            {
                l.Error( "Pouf" );
                l.CloseGroup( "Explicit User Conclusion" );
            }
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Explicit User Conclusion, From Opener, 1 Error" ) );
        }

        [Test]
        [Category( "Console" )]
        public void PathCatcherTests()
        {
            var logger = new DefaultActivityLogger();
            logger.Output.BridgeTo( TestHelper.Logger );
            
            ActivityLoggerPathCatcher p = new ActivityLoggerPathCatcher();
            logger.Output.RegisterClient( p );

            logger.Trace( "Trace n°1" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Trace|Trace n°1" ) );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

            logger.Trace( "Trace n°2" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Trace|Trace n°2" ) );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

            logger.Warn( "W1" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1" ) );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1" ) );

            logger.Error( "E2" );
            logger.Warn( "W1bis" );
            Assert.That( p.DynamicPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1bis" ) );
            Assert.That( p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Error|E2" ) );
            Assert.That( p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ).Single(), Is.EqualTo( "Warn|W1bis" ) );

            p.ClearLastWarnPath();
            Assert.That( p.LastErrorPath, Is.Not.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

            p.ClearLastErrorPath();
            Assert.That( p.LastErrorPath, Is.Null );

            using( logger.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                using( logger.OpenGroup( LogLevel.Info, "G2" ) )
                {
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                    Assert.That( p.LastErrorPath, Is.Null );
                    using( logger.OpenGroup( LogLevel.Trace, "G3" ) )
                    {
                        using( logger.OpenGroup( LogLevel.Info, "G4" ) )
                        {
                            logger.Warn( "W1" );

                            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>G4>W1" ) );

                            logger.Info( 
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
                                logger.Trace( ex );
                            }

                            Assert.That( p.LastErrorPath, Is.Null );
                            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );
                        }
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>G4" ) );
                        Assert.That( p.LastErrorPath, Is.Null );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );

                        logger.Error( "E1" );
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>E1" ) );
                        Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    }
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                }
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                using( logger.OpenGroup( LogLevel.Trace, "G2Bis" ) )
                {
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2Bis" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );

                    logger.Warn( "W2" );
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2Bis>W2" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Trace|G2Bis>Warn|W2" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                }
                logger.Fatal( "F1" );
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>F1" ) );
                Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );
            }

            // Extraneous closing are ignored.
            logger.CloseGroup(  null );

            logger.Warn( "W3" );
            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "W3" ) );
            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Warn|W3" ) );
            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );

            // Extraneous closing are ignored.
            logger.CloseGroup( null );
            
            logger.Warn( "W4" );
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
            var logger = new ActivityLogger();
            logger.Output.BridgeTo( TestHelper.Logger );

            // Registers the ErrorCounter first: it will be the last one to be called, but
            // this does not prevent the PathCatcher to work: the path elements reference the group
            // so that any conclusion arriving after PathCatcher.OnClosing are available.
            ActivityLoggerErrorCounter c = new ActivityLoggerErrorCounter();
            logger.Output.RegisterClient( c );

            // Registers the PathCatcher now: it will be called BEFORE the ErrorCounter.
            ActivityLoggerPathCatcher p = new ActivityLoggerPathCatcher();
            logger.Output.RegisterClient( p );
            
            Assert.That( c.GenerateConclusion, Is.False, "False by default." );
            c.GenerateConclusion = true;
            Assert.That( c.Root.MaxLogLevel == LogLevel.None );

            logger.Trace( "T1" );
            Assert.That( !c.Root.HasWarnOrError && !c.Root.HasError );
            Assert.That( c.Root.MaxLogLevel == LogLevel.Trace );
            Assert.That( c.Root.ToString(), Is.Null );

            logger.Warn( "W1" );
            Assert.That( c.Root.HasWarnOrError && !c.Root.HasError );
            Assert.That( c.Root.MaxLogLevel == LogLevel.Warn );
            Assert.That( c.Root.ToString(), Is.Not.Null.And.Not.Empty );

            logger.Error( "E2" );
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

            using( logger.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                string errorMessage;
                using( logger.OpenGroup( LogLevel.Info, "G2" ) )
                {
                    logger.Error( "E1" );
                    logger.Fatal( "F1" );
                    Assert.That( c.Root.HasWarnOrError && c.Root.HasError );
                    Assert.That( c.Root.ErrorCount == 1 && c.Root.FatalCount == 1 );
                    Assert.That( c.Root.WarnCount == 0 );

                    using( logger.OpenGroup( LogLevel.Info, "G3" ) )
                    {
                        Assert.That( !c.Current.HasWarnOrError && !c.Current.HasError );
                        Assert.That( c.Current.ErrorCount == 0 && c.Current.FatalCount == 0 && c.Current.WarnCount == 0 );
                        
                        logger.Error( "An error..." );

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
                logger.Error( "E3" );
                logger.Fatal( "F2" );
                logger.Warn( "W2" );
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
            IDefaultActivityLogger d = new DefaultActivityLogger();
            var c = new ActivityLoggerSimpleCollector();
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
            IDefaultActivityLogger d = new DefaultActivityLogger();
            
            Assert.Throws<ArgumentNullException>( () => d.Catch( null ) );

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
        }

        [Test]
        public void Overloads()
        {
            Exception ex = new Exception( "EXCEPTION" );
            string fmt0 = "fmt", fmt1 = "fmt{0}", fmt2 = "fmt{0}{1}", fmt3 = "fmt{0}{1}{2}", fmt4 = "fmt{0}{1}{2}{3}", fmt5 = "fmt{0}{1}{2}{3}{4}", fmt6 = "fmt{0}{1}{2}{3}{4}{5}";
            string p1 = "p1", p2 = "p2", p3 = "p3", p4 = "p4", p5 = "p5", p6 = "p6";

            IDefaultActivityLogger d = new DefaultActivityLogger();
            var collector = new ActivityLoggerSimpleCollector() { LevelFilter = LogLevelFilter.Trace, Capacity = 1 };
            d.Output.RegisterClient( collector );

            d.Trace( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Trace( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            d.Trace( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Trace( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Trace( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Trace( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Trace( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Info( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Info( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            d.Info( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Info( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Info( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Info( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Info( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Warn( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Warn( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            d.Warn( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Warn( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Warn( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Warn( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Warn( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Error( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Error( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
            d.Error( fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) );
            d.Error( fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) );
            d.Error( fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) );
            d.Error( fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) );
            d.Error( fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) );

            d.Fatal( fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) );
            d.Fatal( fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) );
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

            IDefaultActivityLogger d = new DefaultActivityLogger();
            var collector = new ActivityLoggerSimpleCollector() { LevelFilter = LogLevelFilter.Trace, Capacity = 1 };
            d.Output.RegisterClient( collector );

            CKTrait tag = ActivityLogger.RegisteredTags.FindOrCreate( "TAG" );

            d.Trace( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Trace( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Info( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Info( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Warn( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Warn( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Error( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt2, p1, p2 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt3, p1, p2, p3 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt4, p1, p2, p3, p4 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt5, p1, p2, p3, p4, p5 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Error( tag, fmt6, p1, p2, p3, p4, p5, p6 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1p2p3p4p5p6" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );

            d.Fatal( tag, fmt0 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmt" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
            d.Fatal( tag, fmt1, p1 ); Assert.That( collector.Entries.Last().Text, Is.EqualTo( "fmtp1" ) ); Assert.That( collector.Entries.Last().Tags, Is.SameAs( tag ) );
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
