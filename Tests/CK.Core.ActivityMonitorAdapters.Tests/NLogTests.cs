using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using NUnit.Framework;

namespace CK.Core.ActivityMonitorAdapters.Tests
{
    [TestFixture]
    public class NLogTests
    {
        IActivityMonitor _m;
        MemoryTarget _target;


        [Test]
        public void NLogAutoLogTest()
        {
            // Monitor and target init in SetUp.
            NLogOutputTest( _m, _target );
        }

        public static void NLogOutputTest( IActivityMonitor m, MemoryTarget target)
        {
            // Simple message
            m.Trace().Send( "Trace message" );

            var line = target.GetLastLogLine();
            DateTime t;
            Assert.That( DateTime.TryParse( line[0], out t ), Is.True, "NLog DateTime can be parsed by DateTime.TryParse" );
            Assert.That( t, Is.GreaterThan( DateTime.MinValue ) );

            Assert.That( line[1], Is.EqualTo( "TRACE" ) );

            Assert.That( line[2], Is.EqualTo( "Trace message" ) );

            Assert.That( line[3], Is.EqualTo( String.Empty ) );

            // Tagged message
            CKTrait tag = ActivityMonitor.Tags.Register( "TestTag" );
            m.Warn().Send( tag, "Warn message" );

            line = target.GetLastLogLine();
            DateTime t2 = DateTime.Parse( line[0] );
            Assert.That( t2, Is.GreaterThanOrEqualTo( t ) );

            Assert.That( line[1], Is.EqualTo( "WARN" ) );
            Assert.That( line[2], Is.EqualTo( "[TestTag] Warn message" ) );
            Assert.That( line[3], Is.EqualTo( String.Empty ) );

            // Open/close group
            using( var g = m.OpenInfo().Send( "Info OpenGroup" ) )
            {
                line = target.GetLastLogLine();
                DateTime t3 = DateTime.Parse( line[0] );
                Assert.That( t3, Is.GreaterThanOrEqualTo( t2 ) );

                Assert.That( line[1], Is.EqualTo( "INFO" ) );
                Assert.That( line[2], Contains.Substring( "Info OpenGroup" ) );
                Assert.That( line[3], Is.EqualTo( String.Empty ) );

                g.ConcludeWith( () => "TestConclusion" );
            }
            // Get CloseGroup line
            line = target.Logs[target.Logs.Count - 2].Split( '\t' );

            Assert.That( line[1], Is.EqualTo( "INFO" ) );
            Assert.That( line[2], Contains.Substring( "Info OpenGroup" ) );
            Assert.That( line[3], Is.EqualTo( String.Empty ) );

            // Conclusions line
            line = target.GetLastLogLine();
            DateTime t4 = DateTime.Parse( line[0] );

            Assert.That( line[1], Is.EqualTo( "INFO" ) );
            Assert.That( line[2], Contains.Substring( "TestConclusion" ) );
            Assert.That( line[3], Is.EqualTo( String.Empty ) );

            // Exception on line
            try { throw new NotImplementedException( "Exception Message" ); }
            catch( Exception e ) { m.Error().Send( e, "Exception Log message" ); }
            line = target.GetLastLogLine();
            DateTime t5 = DateTime.Parse( line[0] );

            Assert.That( line[1], Is.EqualTo( "ERROR" ) );
            Assert.That( line[2], Is.EqualTo( "Exception Log message" ) );
            Assert.That( line[3], Contains.Substring( "NotImplementedException" ) );
            Assert.That( line[3], Contains.Substring( "Exception Message" ) );

            // Exception on group (and tagged group)
            try { throw new NotImplementedException( "Exception Message 2" ); }
            catch( Exception e ) { using( m.OpenError().Send( e, tag, "Exception Log message 2" ) ) { } }

            // Get OpenGroup line
            line = target.Logs[target.Logs.Count - 2].Split( '\t' );

            Assert.That( line[1], Is.EqualTo( "ERROR" ) );
            Assert.That( line[2], Contains.Substring( "TestTag" ) );
            Assert.That( line[2], Contains.Substring( "Exception Log message 2" ) );
            Assert.That( line[3], Contains.Substring( "NotImplementedException" ) );
            Assert.That( line[3], Contains.Substring( "Exception Message 2" ) );
            // Exception output is handled by NLog's configuration (see SetUp). Here, it's basically using the exception's ToString().
        }

        [SetUp]
        public void SetUp()
        {
            _target = new MemoryTarget();
            _target.Name = "mo";
            _target.Layout = "${longdate}\t${uppercase:${level}}\t${message}\t${exception:format=tostring}";

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging( _target, NLog.LogLevel.Trace );

            NLogAdapter.Initialize();

            _m = new ActivityMonitor();
        }

        [TearDown]
        public void TearDown()
        {
            LogManager.Flush();
            LogManager.Shutdown();
        }
    }

    static class NLogExtensions
    {
        public static string[] GetLastLogLine( this MemoryTarget @this )
        {
            LogManager.Flush();
            string[] a = @this.Logs[@this.Logs.Count - 1].Split( '\t' );

            Assert.That( a.Length, Is.EqualTo( 4 ) );

            return a;
        }
    }
}
