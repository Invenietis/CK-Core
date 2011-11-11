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

        public class StringImpl : IDefaultActivityLoggerSink, IDisposable
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

            public void OnGroupOpen( DefaultActivityLogger.Group g )
            {
                Debug.Assert( Writer != null );
                Writer.WriteLine();
                Writer.Write( "++ {0} / {1} / {2}", g.Depth, g.GroupLevel, g.GroupText );
            }

            public void OnGroupClose( DefaultActivityLogger.Group g, string conclusion )
            {
                Writer.WriteLine();
                Writer.Write( "-- {0}", conclusion );
            }

            public void Dispose()
            {
                if( Writer != null )
                {
                    Writer.Flush();
                    Writer.Close();
                    Writer.Dispose();
                }
            }

            #region IActivityLoggerImpl Membres

            public IActivityLogger Logger
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        public class XmlImpl : IDefaultActivityLoggerSink
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
                XmlWriter.WriteElementString( level.ToString(), text );
            }

            public void OnContinueOnSameLevel( LogLevel level, string text )
            {
                XmlWriter.WriteElementString( level.ToString(), text );
            }

            public void OnLeaveLevel( LogLevel level )
            {
                //XmlWriter.Flush();
            }

            public void OnGroupOpen( DefaultActivityLogger.Group g )
            {
                XmlWriter.WriteStartElement( g.GroupLevel.ToString() + "s" );
                XmlWriter.WriteAttributeString( "Depth", g.Depth.ToString() );
                XmlWriter.WriteAttributeString( "Level", g.GroupLevel.ToString() );
                XmlWriter.WriteAttributeString( "Text", g.GroupText.ToString() );
            }

            public void OnGroupClose( DefaultActivityLogger.Group g, string conclusion )
            {
                XmlWriter.WriteEndElement();
                XmlWriter.Flush();
            }
        }

        [Test]
        public void DefaultImpl()
        {
            DefaultActivityLogger l = new DefaultActivityLogger();

            l.Register( new StringImpl() ).Register( new XmlImpl( new StringWriter() ) );

            Assert.That( l.RegisteredLoggers.Count(), Is.EqualTo( 2 ) );

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

            Console.WriteLine( l.FirstLogger<StringImpl>().Writer );
            Console.WriteLine( l.FirstLogger<XmlImpl>().InnerWriter );

            XPathDocument d = new XPathDocument( new StringReader( l.FirstLogger<XmlImpl>().InnerWriter.ToString() ) );

            Assert.That( d.CreateNavigator().SelectDescendants( "Info", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 3 ) );
            Assert.That( d.CreateNavigator().SelectDescendants( "Trace", String.Empty, false ), Is.Not.Empty.And.Count.EqualTo( 4 ) );

        }

        [Test]
        public void MultipleClose()
        {
            DefaultActivityLogger l = new DefaultActivityLogger();

            var log1 = new StringImpl();
            var log2 = new XmlImpl( new StringWriter() );
            l.Register( log1 ).Register( log2 );

            Assert.That( l.RegisteredLoggers.Count(), Is.EqualTo( 2 ) );

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

            Assert.That( log1.Writer.ToString(), Is.Not.ContainsSubstring( "End First" ), "Close forgets other closes..." );
            Assert.That( log1.Writer.ToString(), Is.Not.ContainsSubstring( "Close it again" ), "Close forgets other closes..." );
        }

    }
}
