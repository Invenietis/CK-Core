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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

namespace Core
{
    [TestFixture]
    public class ActivityLoggerTests
    {

        public class StringImpl : IActivityLoggerSink
        {
            public StringWriter Writer { get; private set; }

            public StringImpl()
            {
                Writer = new StringWriter();
            }

            public void OnEnterLevel( LogLevel level, string text )
            {
                Writer.WriteLine();
                Writer.Write( level.ToString() + ": " + text );
            }

            public void OnContinueOnSameLevel( LogLevel level, string text )
            {
                Writer.Write( text );
            }

            public void OnLeaveLevel( LogLevel level )
            {
                Writer.Flush();
            }

            public void OnGroupOpen( IActivityLogGroup g )
            {
                Writer.WriteLine();
                Writer.Write( new String( '+', g.Depth ) ); 
                Writer.Write( "{1} ({0})", g.GroupLevel, g.GroupText );
            }

            public void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
                Writer.WriteLine();
                Writer.Write( new String( '-', g.Depth ) );
                Writer.Write( String.Join( ", ", conclusions.Select( c => c.Conclusion ) ) );
            }
        }

        public class XmlImpl : IActivityLoggerSink
        {
            XmlWriter XmlWriter { get; set; }

            public TextWriter InnerWriter { get; private set; }

            public XmlImpl( StringWriter s )
            {
                XmlWriter = XmlWriter.Create( s, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true } );
                InnerWriter = s;
            }

            public void OnEnterLevel( LogLevel level, string text )
            {
                XmlWriter.WriteStartElement( level.ToString() );
                XmlWriter.WriteString( text );
            }

            public void OnContinueOnSameLevel( LogLevel level, string text )
            {
                XmlWriter.WriteString( text );
            }

            public void OnLeaveLevel( LogLevel level )
            {
                XmlWriter.WriteEndElement();
            }

            public void OnGroupOpen( IActivityLogGroup g )
            {
                XmlWriter.WriteStartElement( g.GroupLevel.ToString() + "s" );
                XmlWriter.WriteAttributeString( "Depth", g.Depth.ToString() );
                XmlWriter.WriteAttributeString( "Level", g.GroupLevel.ToString() );
                XmlWriter.WriteAttributeString( "Text", g.GroupText.ToString() );
            }

            public void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
                XmlWriter.WriteEndElement();
                XmlWriter.Flush();
            }
        }

        [Test]
        public void DefaultImpl()
        {
            IDefaultActivityLogger logger = DefaultActivityLogger.Create();
            // Binds the TestHelper.Logger logger to this one.
            logger.Output.RegisterMuxClient( TestHelper.Logger.Output.ExternalInput );

            logger.Register( new StringImpl() ).Register( new XmlImpl( new StringWriter() ) );

            Assert.That( logger.RegisteredSinks.Count, Is.EqualTo( 2 ) );

            using( logger.OpenGroup( LogLevel.Trace, () => "EndMainGroup", "MainGroup" ) )
            {
                logger.Trace( "First" );
                logger.Trace( "Second" );
                logger.Trace( "Third" );
                logger.Info( "First" );

                using( logger.OpenGroup( LogLevel.Info, () => "EndInfoGroup", "InfoGroup" ) )
                {
                    logger.Info( "Second" );
                    logger.Trace( "Fourth" );
                    
                    using( logger.OpenGroup( LogLevel.Warn, () => "EndWarnGroup", "WarnGroup {0} - Now = {1}", 4, DateTime.UtcNow ) )
                    {
                        logger.Info( "Warn!" );
                    }
                }
            }

            Console.WriteLine( logger.RegisteredSinks.OfType<StringImpl>().Single().Writer );
            Console.WriteLine( logger.RegisteredSinks.OfType<XmlImpl>().Single().InnerWriter );

            XPathDocument d = new XPathDocument( new StringReader( logger.RegisteredSinks.OfType<XmlImpl>().Single().InnerWriter.ToString() ) );

            Assert.That( d.CreateNavigator().SelectDescendants( "Info", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 3 ) );
            Assert.That( d.CreateNavigator().SelectDescendants( "Trace", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 2 ) );

        }

        [Test]
        public void MultipleClose()
        {
            IDefaultActivityLogger logger = DefaultActivityLogger.Create();
            // Binds the TestHelper.Logger logger to this one.
            logger.Output.RegisterMuxClient( TestHelper.Logger.Output.ExternalInput );

            var log1 = new StringImpl();
            logger.Register( log1 );

            Assert.That( logger.RegisteredSinks.Count, Is.EqualTo( 1 ) );

            using( logger.OpenGroup( LogLevel.Trace, () => "End First", "First" ) )
            {
                logger.CloseGroup( "Pouf" );
                using( logger.OpenGroup( LogLevel.Warn, "A group at level 0!" ) )
                {
                    logger.CloseGroup( "Close it." );
                    logger.CloseGroup( "Close it again." );
                }
            }

            Assert.That( log1.Writer.ToString(), Is.Not.StringContaining( "End First" ), "Close forgets other closes..." );
            Assert.That( log1.Writer.ToString(), Is.Not.StringContaining( "Close it again" ), "Close forgets other closes..." );
        }

        [Test]
        public void DefaultActivityLoggerDefaults()
        {
            IDefaultActivityLogger l = DefaultActivityLogger.Create();
            Assert.Throws<InvalidOperationException>( () => l.Output.UnregisterClient( l.Tap ) );
            Assert.Throws<InvalidOperationException>( () => l.Output.UnregisterMuxClient( l.Tap ) );
            Assert.Throws<InvalidOperationException>( () => l.Output.UnregisterClient( l.PathCatcher ) );
            Assert.Throws<InvalidOperationException>( () => l.Output.UnregisterMuxClient( l.PathCatcher ) );
            Assert.Throws<InvalidOperationException>( () => l.Output.UnregisterClient( l.ErrorCounter ) );
            Assert.Throws<InvalidOperationException>( () => l.Output.UnregisterMuxClient( l.ErrorCounter ) );
        }

        [Test]
        public void FilterLevel()
        {
            IDefaultActivityLogger l = DefaultActivityLogger.Create();
            // Binds the TestHelper.Logger logger to this one.
            l.Output.RegisterMuxClient( TestHelper.Logger.Output.ExternalInput );
            
            var log = new StringImpl();
            l.Register( log );
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
        public void CloseMismatch()
        {
            IDefaultActivityLogger l = DefaultActivityLogger.Create();
            var log = new StringImpl();
            l.Register( log );
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
        public void ExplicitCloseWins()
        {
            IDefaultActivityLogger l = DefaultActivityLogger.Create();
            // Binds the TestHelper.Logger logger to this one.
            l.Output.RegisterMuxClient( TestHelper.Logger.Output.ExternalInput );
            
            var log = new StringImpl();
            l.Register( log );

            // No explicit close conclusion: Success!
            using( l.OpenGroup( LogLevel.Trace, () => "Success!", "First" ) )
            {
                l.Error( "Pouf" );
            }
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Pouf" ) );
            Assert.That( log.Writer.ToString(), Is.Not.StringContaining( "Failed!" ), "Default conclusion wins." );
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Success!" ), "Default conclusion." );

            log.Writer.GetStringBuilder().Clear();
            Assert.That( log.Writer.ToString(), Is.Empty );

            // Explicit conclusion: Failed!
            using( l.OpenGroup( LogLevel.Trace, () => "Success!", "First" ) )
            {
                l.Error( "Pouf" );
                l.CloseGroup( "Failed!" );
            }
            Console.WriteLine( log.Writer.ToString() );

            Assert.That( log.Writer.ToString(), Is.StringContaining( "Pouf" ) );
            Assert.That( log.Writer.ToString(), Is.StringContaining( "Failed!" ), "Explicit conclusion wins." );
            Assert.That( log.Writer.ToString(), Is.Not.StringContaining( "Success!" ), "Explicit conclusion wins." );
        }

        [Test]
        public void PathCatcherTests()
        {
            var logger = DefaultActivityLogger.Create();
            // Binds the TestHelper.Logger logger to this one.
            logger.Output.RegisterMuxClient( TestHelper.Logger.Output.ExternalInput );
            
            ActivityLoggerPathCatcher p = new ActivityLoggerPathCatcher();
            logger.Output.RegisterMuxClient( p );

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
                                            throw new Exception( "Deepest excpetion." );
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
        public void ErrorCounterTests()
        {
            var logger = new ActivityLogger();
            // Binds the TestHelper.Logger logger to this one.
            logger.Output.RegisterMuxClient( TestHelper.Logger.Output.ExternalInput );

            // Registers the ErrorCounter first: it will be the last one to be called, but
            // this does not prevent the PathCatcher to work: the path elements reference the group
            // so that aany conclusion arriving after PathCatcher.OnClosing are available.
            ActivityLoggerErrorCounter c = new ActivityLoggerErrorCounter();
            logger.Output.RegisterMuxClient( c );

            // Registers the PathCatcher now: it will be called BEFORE the ErrorCounter.
            ActivityLoggerPathCatcher p = new ActivityLoggerPathCatcher();
            logger.Output.RegisterClient( p );
            
            Assert.That( c.GenerateConclusion, Is.True, "Must be the default." );
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
                        
                        logger.Error( "E2" );
                        
                        Assert.That( c.Current.HasWarnOrError && c.Current.HasError );
                        Assert.That( c.Current.ErrorCount == 1 && c.Current.FatalCount == 0 && c.Current.WarnCount == 0 );
                    }
                }
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion.ToStringGroupConclusion() ) ), Is.EqualTo( "G1->G2-1 Fatal error, 2 Errors>G3-1 Error>E2-" ) );
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

    }
}
