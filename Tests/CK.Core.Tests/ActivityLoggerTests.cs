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
            public StringWriter Writer { get; set; }

            public StringImpl()
            {
                Writer = new StringWriter();
            }

            public void OnEnterLevel( LogLevel level, string text )
            {
                Debug.Assert( Writer != null );
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
                Debug.Assert( Writer != null );
                Writer.WriteLine();
                Writer.Write( new String( '+', g.Depth ) ); 
                Writer.Write( "{1} ({0})", g.GroupLevel, g.GroupText );
            }

            public void OnGroupClose( IActivityLogGroup g, string conclusion )
            {
                Writer.WriteLine();
                Writer.Write( new String( '-', g.Depth ) );
                Writer.Write( conclusion );
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

            public void OnGroupClose( IActivityLogGroup g, string conclusion )
            {
                XmlWriter.WriteEndElement();
                XmlWriter.Flush();
            }
        }

        [Test]
        public void DefaultImpl()
        {
            IDefaultActivityLogger l = DefaultActivityLogger.Create();

            l.Register( new StringImpl() ).Register( new XmlImpl( new StringWriter() ) );

            Assert.That( l.RegisteredSinks.Count, Is.EqualTo( 2 ) );

            using( l.OpenGroup( LogLevel.Trace, () => "EndMainGroup", "MainGroup" ) )
            {
                l.Trace( "First" );
                l.Trace( "Second" );
                l.Trace( "Third" );
                l.Info( "First" );

                using( l.OpenGroup( LogLevel.Info, () => "EndInfoGroup", "InfoGroup" ) )
                {
                    l.Info( "Second" );
                    l.Trace( "Fourth" );
                    
                    using( l.OpenGroup( LogLevel.Warn, () => "EndWarnGroup", "WarnGroup {0} - Now = {1}", 4, DateTime.UtcNow ) )
                    {
                        l.Info( "Warn!" );
                    }
                }
            }

            Console.WriteLine( l.RegisteredSinks.OfType<StringImpl>().Single().Writer );
            Console.WriteLine( l.RegisteredSinks.OfType<XmlImpl>().Single().InnerWriter );

            XPathDocument d = new XPathDocument( new StringReader( l.RegisteredSinks.OfType<XmlImpl>().Single().InnerWriter.ToString() ) );

            Assert.That( d.CreateNavigator().SelectDescendants( "Info", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 3 ) );
            Assert.That( d.CreateNavigator().SelectDescendants( "Trace", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 2 ) );

        }

        [Test]
        public void MultipleClose()
        {
            IDefaultActivityLogger l = DefaultActivityLogger.Create();

            var log1 = new StringImpl();
            var log2 = new XmlImpl( new StringWriter() );
            l.Register( log1 ).Register( log2 );

            Assert.That( l.RegisteredSinks.Count, Is.EqualTo( 2 ) );

            using( l.OpenGroup( LogLevel.Trace, () => "End First", "First" ) )
            {
                l.CloseGroup( "Pouf" );
                using( l.OpenGroup( LogLevel.Warn, "A group at level 0!" ) )
                {
                    l.CloseGroup( "Close it." );
                    l.CloseGroup( "Close it again." );
                }
            }
            Console.WriteLine( log1.Writer.ToString() );
            Console.WriteLine( log2.InnerWriter.ToString() );

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
                            Assert.That( p.LastErrorPath, Is.Null );
                            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );
                        }
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3" ) );
                        Assert.That( p.LastErrorPath, Is.Null );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Info|G4>Warn|W1" ) );

                        logger.Error( "E1" );
                        Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2>G3>E1" ) );
                        Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                        Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    }
                    Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1>G2" ) );
                    Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                    Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Info|G2>Trace|G3>Error|E1" ) );
                }
                Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "G1" ) );
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
            logger.Warn( "W3" );
            Assert.That( String.Join( ">", p.DynamicPath.Select( e => e.Text ) ), Is.EqualTo( "W3" ) );
            Assert.That( String.Join( ">", p.LastWarnOrErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Warn|W3" ) );
            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Level.ToString() + '|' + e.Text ) ), Is.EqualTo( "Trace|G1>Fatal|F1" ) );

            p.ClearLastWarnPath( true );
            Assert.That( p.LastErrorPath, Is.Null );
            Assert.That( p.LastWarnOrErrorPath, Is.Null );

        }

        [Test]
        public void ErrorCounterTests()
        {
            var logger = new ActivityLogger();
            ActivityLoggerErrorCounter c = new ActivityLoggerErrorCounter();
            ActivityLoggerPathCatcher p = new ActivityLoggerPathCatcher();
            logger.Output.RegisterMuxClient( c );
            logger.Output.RegisterClient( p );

            Assert.That( c.ConclusionMode, Is.EqualTo( ActivityLoggerErrorCounter.ConclusionTextMode.SetWhenEmpty ), "Must be the default." );

            logger.Trace( "T1" );
            Assert.That( !c.HasWarnOrError && !c.HasError );
            Assert.That( c.MaxLogLevel == LogLevel.Trace );
            Assert.That( c.GetCurrentMessage(), Is.Null );

            logger.Warn( "W1" );
            Assert.That( c.HasWarnOrError && !c.HasError );
            Assert.That( c.MaxLogLevel == LogLevel.Warn );
            Assert.That( c.GetCurrentMessage(), Is.Not.Null.And.Not.Empty );

            logger.Error( "E2" );
            Assert.That( c.HasWarnOrError && c.HasError );
            Assert.That( c.ErrorCount == 1 );
            Assert.That( c.MaxLogLevel == LogLevel.Error );
            Assert.That( c.GetCurrentMessage(), Is.Not.Null.And.Not.Empty );

            c.ClearWarn();
            Assert.That( c.HasWarnOrError && c.HasError );
            Assert.That( c.MaxLogLevel == LogLevel.Error );
            Assert.That( c.GetCurrentMessage(), Is.Not.Null );

            c.ClearError();
            Assert.That( !c.HasWarnOrError && !c.HasError );
            Assert.That( c.ErrorCount == 0 );
            Assert.That( c.MaxLogLevel == LogLevel.Info );
            Assert.That( c.GetCurrentMessage(), Is.Null );

            using( logger.OpenGroup( LogLevel.Trace, "G1" ) )
            {
                using( logger.OpenGroup( LogLevel.Info, "G2" ) )
                {
                    logger.Error( "E2" );
                    logger.Fatal( "F1" );
                    Assert.That( c.HasWarnOrError && c.HasError );
                    Assert.That( c.ErrorCount == 1 && c.FatalCount == 1 );
                    Assert.That( c.WarnCount == 0 );
                }
                Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion ) ), Is.EqualTo( "G1->G2-1 Fatal error, 1 Error>F1-" ) );
                logger.Error( "E3" );
                logger.Fatal( "F2" );
                logger.Warn( "W2" );
                Assert.That( c.HasWarnOrError && c.HasError );
                Assert.That( c.ErrorCount == 2 );
                Assert.That( c.MaxLogLevel == LogLevel.Fatal );
            }
            Assert.That( String.Join( ">", p.LastErrorPath.Select( e => e.Text + '-' + e.GroupConclusion ) ), Is.EqualTo( "G1-2 Fatal errors, 2 Errors, 1 Warning>F2-" ) );
        }

    }
}
