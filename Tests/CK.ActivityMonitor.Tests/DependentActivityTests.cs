using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    [CLSCompliant(false)]
    public class DependentActivityTests
    {

        [Test]
        public void DependentToken_API_use()
        {
            var monitor = new ActivityMonitor();
            monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget );

            using( monitor.OpenTrace().Send( "Create token and dependent monitor." ) )
            {
                // Creates the token.
                var token = monitor.DependentActivity().CreateToken();
                // Creates a dependent monitor.
                using( var monitorDep = token.CreateDependentMonitor( m => m.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) ) )
                {
                    monitor.Trace().Send( "Doing something..." );
                    // ...
                }
            }
            using( monitor.OpenTrace().Send( "Create token with delayed launch of the dependent activity." ) )
            {
                // Creates the token.
                var token = monitor.DependentActivity().CreateToken( delayedLaunch: true );
                // Signals the launch of the dependent activity.
                monitor.DependentActivity().Launch( token );
                // Creates a dependent monitor.
                using( var monitorDep = token.CreateDependentMonitor( m => m.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) ) )
                {
                    monitor.Trace().Send( "Doing something..." );
                    // ...
                }
            }
            using( monitor.OpenTrace().Send( "Starting a dependent activity on an existing monitor." ) )
            {
                // Creates the token.
                var token = monitor.DependentActivity().CreateToken();
            
                IActivityMonitor wMonitor = monitor;
                using( wMonitor.StartDependentActivity( token ) )
                {
                    wMonitor.Trace().Send( "Doing something..." );
                    // ...
                }
            }
        }

        [TestCase( "A topic!" )]
        [TestCase( "A 'topic' with quote." )]
        [TestCase( "A 'topic' \r\n with quote and new lines." )]
        [TestCase( null )]
        [TestCase( "" )]
        [TestCase( " " )]
        public void parsing_DependentToken_with_topics( string topic )
        {
            var monitor = new ActivityMonitor();
            var t1 = monitor.DependentActivity().CreateTokenWithTopic( topic );
            var t2 = monitor.DependentActivity().CreateTokenWithTopic( topic );
            var t3 = monitor.DependentActivity().CreateTokenWithTopic( topic );
            Assume.That( t2.CreationDate.Uniquifier + t3.CreationDate.Uniquifier > 0 );

            var r1 = ActivityMonitor.DependentToken.Parse( t1.ToString() );

            Assert.That( r1.OriginatorId, Is.EqualTo( t1.OriginatorId ) );
            Assert.That( r1.CreationDate, Is.EqualTo( t1.CreationDate ) );
            Assert.That( r1.Topic, Is.EqualTo( t1.Topic ) );
            Assert.That( r1.ToString(), Is.EqualTo( t1.ToString() ) );

            var r2 = ActivityMonitor.DependentToken.Parse( t2.ToString() );
            Assert.That( r2.OriginatorId, Is.EqualTo( t2.OriginatorId ) );
            Assert.That( r2.CreationDate, Is.EqualTo( t2.CreationDate ) );
            Assert.That( r2.Topic, Is.EqualTo( t2.Topic ) );
            Assert.That( r2.ToString(), Is.EqualTo( t2.ToString() ) );

            var r3 = ActivityMonitor.DependentToken.Parse( t3.ToString() );
            Assert.That( r3.OriginatorId, Is.EqualTo( t3.OriginatorId ) );
            Assert.That( r3.CreationDate, Is.EqualTo( t3.CreationDate ) );
            Assert.That( r3.Topic, Is.EqualTo( t3.Topic ) );
            Assert.That( r3.ToString(), Is.EqualTo( t3.ToString() ) );
        }

        [Test]
        public void parsing_dependent_token_and_start_and_create_messages_with_time_collision()
        {
            ActivityMonitor m = new ActivityMonitor( false );
            m.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget );
            StupidStringClient cLaunch = m.Output.RegisterClient( new StupidStringClient() );

            // Generates a token with time collision.
            int loopNeeded = 0;
            ActivityMonitor.DependentToken token;
            while( (token = m.DependentActivity().CreateTokenWithTopic( "Test..." )).CreationDate.Uniquifier == 0 ) ++loopNeeded;
            Assert.That( token.Topic, Is.EqualTo( "Test..." ) );
            m.Trace().Send( "Generating time collision required {0} loops.", loopNeeded );

            string launchMessage = cLaunch.Entries[loopNeeded].Text;
            {
                bool launched;
                bool launchWithTopic;
                string launchDependentTopic;
                Assert.That( ActivityMonitor.DependentToken.TryParseLaunchOrCreateMessage( launchMessage, out launched, out launchWithTopic, out launchDependentTopic ) );
                Assert.That( !launched, "We used CreateToken." );
                Assert.That( launchWithTopic );
                Assert.That( launchDependentTopic, Is.EqualTo( "Test..." ) );
            }

            string tokenToString = token.ToString();
            {
                ActivityMonitor.DependentToken t2 = ActivityMonitor.DependentToken.Parse( tokenToString );
                Assert.That( t2.OriginatorId, Is.EqualTo( ((IUniqueId)m).UniqueId ) );
                Assert.That( t2.CreationDate, Is.EqualTo( cLaunch.Entries[loopNeeded].LogTime ) );
                Assert.That( t2.Topic, Is.EqualTo( "Test..." ) );
            }

            StupidStringClient.Entry[] logs = RunDependentActivity( token );
            {
                Assert.That( logs[0].Text, Is.EqualTo( ActivityMonitor.SetTopicPrefix + "Test..." ) );
                Guid id;
                DateTimeStamp time;
                Assert.That( ActivityMonitor.DependentToken.TryParseStartMessage( logs[1].Text, out id, out time ) );
                Assert.That( id, Is.EqualTo( ((IUniqueId)m).UniqueId ) );
                Assert.That( time, Is.EqualTo( cLaunch.Entries[loopNeeded].LogTime ) );
            }
        }

        [Test]
        public void parsing_start_and_create_messages()
        {
            ActivityMonitor m = new ActivityMonitor( false );
            StupidStringClient cLaunch = m.Output.RegisterClient( new StupidStringClient() );
            StupidStringClient.Entry[] dependentLogs = null;

            string dependentTopic = "A topic 'with' quotes '-\"..." + Environment.NewLine + " and multi-line";
            dependentLogs = LaunchAndRunDependentActivityWithTopic( m, dependentTopic );

            string launchMessage = cLaunch.Entries[0].Text;
            string topicSetMessage = dependentLogs[0].Text;
            string startMessage = dependentLogs[1].Text;

            Assert.That( topicSetMessage, Is.EqualTo( ActivityMonitor.SetTopicPrefix + dependentTopic ) );
            Assert.That( dependentLogs[2].Text, Is.EqualTo( "Hello!" ) );

            Assert.That( launchMessage, Does.StartWith( "Launching dependent activity" ) );
            bool launched;
            bool launchWithTopic;
            string launchDependentTopic;
            Assert.That( ActivityMonitor.DependentToken.TryParseLaunchOrCreateMessage( launchMessage, out launched, out launchWithTopic, out launchDependentTopic ) );
            Assert.That( launched );
            Assert.That( launchWithTopic );
            Assert.That( launchDependentTopic, Is.EqualTo( dependentTopic ) );

            Assert.That( startMessage, Does.StartWith( "Starting dependent activity" ) );
            Guid id;
            DateTimeStamp time;
            Assert.That( ActivityMonitor.DependentToken.TryParseStartMessage( startMessage, out id, out time ) );
            Assert.That( id, Is.EqualTo( ((IUniqueId)m).UniqueId ) );
            Assert.That( time, Is.EqualTo( cLaunch.Entries[0].LogTime ) );
        }

        private static StupidStringClient.Entry[] LaunchAndRunDependentActivityWithTopic( ActivityMonitor m, string dependentTopic )
        {
            StupidStringClient.Entry[] dependentLogs = null;
            m.DependentActivity().LaunchWithTopic( token => { dependentLogs = RunDependentActivity( token ); }, dependentTopic );
            return dependentLogs;
        }

        private static StupidStringClient.Entry[] RunDependentActivity( ActivityMonitor.DependentToken token )
        {
            string depMonitorTopic = null;
            StupidStringClient.Entry[] dependentLogs = null;
            var task = Task.Factory.StartNew( t =>
            {
                StupidStringClient cStarted = new StupidStringClient();
                using( var depMonitor = token.CreateDependentMonitor( mD => mD.Output.RegisterClient( cStarted ) ) )
                {
                    depMonitorTopic = depMonitor.Topic;
                    depMonitor.Trace().Send( "Hello!" );
                }
                dependentLogs = cStarted.Entries.ToArray();
            }, token );
            task.Wait();
            Assert.That( depMonitorTopic, Is.EqualTo( token.Topic ) );
            return dependentLogs;
        }
    }
}
