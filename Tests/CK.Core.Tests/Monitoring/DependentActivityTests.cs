using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class DependentActivityTests
    {

        [Test]
        public void ParseDependentMessageWithUniquifier()
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

            StupidStringClient.Entry[] logs = RunDependentActivity( token );
            {
                Assert.That( logs[0].Text, Is.EqualTo( "Topic: Test..." ) );
                Guid id;
                LogTimestamp time;
                Assert.That( ActivityMonitor.DependentToken.TryParseStartMessage( logs[1].Text, out id, out time ) );
                Assert.That( id, Is.EqualTo( ((IUniqueId)m).UniqueId ) );
                Assert.That( time, Is.EqualTo( cLaunch.Entries[loopNeeded].LogTime ) );
            }
        }

        [Test]
        public void ParseDependentMessageWithTopic()
        {
            ActivityMonitor m = new ActivityMonitor( false );
            StupidStringClient cLaunch = m.Output.RegisterClient( new StupidStringClient() );
            StupidStringClient.Entry[] dependentLogs = null;

            string dependentTopic = "A topic 'with' quotes '-\"...\r\n and multi-line";
            dependentLogs = LaunchAndRunDependentActivityWithTopic( m, dependentTopic );

            string launchMessage = cLaunch.Entries[0].Text;
            string topicSetMessage = dependentLogs[0].Text;
            string startMessage = dependentLogs[1].Text;

            Assert.That( topicSetMessage, Is.EqualTo( "Topic: "+dependentTopic ) );
            Assert.That( dependentLogs[2].Text, Is.EqualTo( "Hello!" ) );

            Assert.That( launchMessage, Is.StringStarting( "Launching dependent activity" ) );
            bool launched;
            bool launchWithTopic;
            string launchDependentTopic;
            Assert.That( ActivityMonitor.DependentToken.TryParseLaunchOrCreateMessage( launchMessage, out launched, out launchWithTopic, out launchDependentTopic ) );
            Assert.That( launched );
            Assert.That( launchWithTopic );
            Assert.That( launchDependentTopic, Is.EqualTo( dependentTopic ) );

            Assert.That( startMessage, Is.StringStarting( "Starting dependent activity" ) );
            Guid id;
            LogTimestamp time;
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
