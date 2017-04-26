using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CK.Core.Tests.Monitoring
{
    public class DependentActivityTests : MutexTest<ActivityMonitor>
    {

        [Fact]
        public void DependentToken_API_use()
        {
            using (LockFact())
            {

                var monitor = new ActivityMonitor();
                monitor.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);

                using (monitor.OpenTrace().Send("Create token and dependent monitor."))
                {
                    // Creates the token.
                    var token = monitor.DependentActivity().CreateToken();
                    // Creates a dependent monitor.
                    var dep = new ActivityMonitor();
                    dep.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);
                    using (dep.StartDependentActivity(token))
                    {
                        monitor.Trace().Send("Doing something...");
                        // ...
                    }
                    dep.MonitorEnd();
                }
                using (monitor.OpenTrace().Send("Create token with delayed launch of the dependent activity."))
                {
                    // Creates the token.
                    var token = monitor.DependentActivity().CreateToken(delayedLaunch: true);
                    // Signals the launch of the dependent activity.
                    monitor.DependentActivity().Launch(token);
                    // Creates a dependent monitor.
                    var dep = new ActivityMonitor();
                    dep.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);
                    using (dep.StartDependentActivity(token))
                    {
                        monitor.Trace().Send("Doing something...");
                        // ...
                    }
                    dep.MonitorEnd();
                }
            }
        }

        [Theory]
        [InlineData("A topic!")]
        [InlineData("A 'topic' with quote.")]
        [InlineData("A 'topic' \r\n with quote and new lines.")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void parsing_DependentToken_with_topics(string topic)
        {
            var monitor = new ActivityMonitor(applyAutoConfigurations: false);
            var t1 = monitor.DependentActivity().CreateTokenWithTopic(topic);
            var t2 = monitor.DependentActivity().CreateTokenWithTopic(topic);
            var t3 = monitor.DependentActivity().CreateTokenWithTopic(topic);
            //Assume.That( t2.CreationDate.Uniquifier + t3.CreationDate.Uniquifier > 0 );

            var r1 = ActivityMonitor.DependentToken.Parse(t1.ToString());

            r1.OriginatorId.Should().Be(t1.OriginatorId);
            r1.CreationDate.Should().Be(t1.CreationDate);
            r1.Topic.Should().Be(t1.Topic);
            r1.ToString().Should().Be(t1.ToString());

            var r2 = ActivityMonitor.DependentToken.Parse(t2.ToString());
            r2.OriginatorId.Should().Be(t2.OriginatorId);
            r2.CreationDate.Should().Be(t2.CreationDate);
            r2.Topic.Should().Be(t2.Topic);
            r2.ToString().Should().Be(t2.ToString());

            var r3 = ActivityMonitor.DependentToken.Parse(t3.ToString());
            r3.OriginatorId.Should().Be(t3.OriginatorId);
            r3.CreationDate.Should().Be(t3.CreationDate);
            r3.Topic.Should().Be(t3.Topic);
            r3.ToString().Should().Be(t3.ToString());
        }

        [Fact]
        public void parsing_dependent_token_and_start_and_create_messages_with_time_collision()
        {
            using (LockFact())
            {
                ActivityMonitor m = new ActivityMonitor(applyAutoConfigurations: false);
                m.Output.CreateBridgeTo(TestHelper.ConsoleMonitor.Output.BridgeTarget);
                StupidStringClient cLaunch = m.Output.RegisterClient(new StupidStringClient());

                // Generates a token with time collision.
                int loopNeeded = 0;
                ActivityMonitor.DependentToken token;
                while ((token = m.DependentActivity().CreateTokenWithTopic("Test...")).CreationDate.Uniquifier == 0) ++loopNeeded;
                token.Topic.Should().Be("Test...");
                m.Trace().Send("Generating time collision required {0} loops.", loopNeeded);

                string launchMessage = cLaunch.Entries[loopNeeded].Text;
                {
                    bool launched;
                    bool launchWithTopic;
                    string launchDependentTopic;
                    ActivityMonitor.DependentToken.TryParseLaunchOrCreateMessage(launchMessage, out launched, out launchWithTopic, out launchDependentTopic)
                       .Should().BeTrue();
                    launched.Should().BeFalse("We used CreateToken.");
                    launchWithTopic.Should().BeTrue();
                    launchDependentTopic.Should().Be("Test...");
                }

                string tokenToString = token.ToString();
                {
                    ActivityMonitor.DependentToken t2 = ActivityMonitor.DependentToken.Parse(tokenToString);
                    t2.OriginatorId.Should().Be(((IUniqueId)m).UniqueId);
                    t2.CreationDate.Should().Be(cLaunch.Entries[loopNeeded].LogTime);
                    t2.Topic.Should().Be("Test...");
                }

                StupidStringClient.Entry[] logs = RunDependentActivity(token);
                {
                    logs[0].Text.Should().Be(ActivityMonitor.SetTopicPrefix + "Test...");
                    Guid id;
                    DateTimeStamp time;
                    ActivityMonitor.DependentToken.TryParseStartMessage(logs[1].Text, out id, out time).Should().BeTrue();
                    id.Should().Be(((IUniqueId)m).UniqueId);
                    time.Should().Be(cLaunch.Entries[loopNeeded].LogTime);
                }
            }
        }

        [Fact]
        public void parsing_start_and_create_messages()
        {
            ActivityMonitor m = new ActivityMonitor(applyAutoConfigurations: false);
            StupidStringClient cLaunch = m.Output.RegisterClient(new StupidStringClient());
            StupidStringClient.Entry[] dependentLogs = null;

            string dependentTopic = "A topic 'with' quotes '-\"..." + Environment.NewLine + " and multi-line";
            dependentLogs = LaunchAndRunDependentActivityWithTopic(m, dependentTopic);

            string launchMessage = cLaunch.Entries[0].Text;
            string topicSetMessage = dependentLogs[0].Text;
            string startMessage = dependentLogs[1].Text;

            topicSetMessage.Should().Be(ActivityMonitor.SetTopicPrefix + dependentTopic);
            dependentLogs[2].Text.Should().Be("Hello!");

            launchMessage.Should().StartWith("Launching dependent activity");
            bool launched;
            bool launchWithTopic;
            string launchDependentTopic;
            ActivityMonitor.DependentToken.TryParseLaunchOrCreateMessage(launchMessage, out launched, out launchWithTopic, out launchDependentTopic).Should().BeTrue();
            launched.Should().BeTrue();
            launchWithTopic.Should().BeTrue();
            launchDependentTopic.Should().Be(dependentTopic);

            startMessage.Should().StartWith("Starting dependent activity");
            Guid id;
            DateTimeStamp time;
            ActivityMonitor.DependentToken.TryParseStartMessage(startMessage, out id, out time).Should().BeTrue();
            id.Should().Be(((IUniqueId)m).UniqueId);
            time.Should().Be(cLaunch.Entries[0].LogTime);
        }

        private static StupidStringClient.Entry[] LaunchAndRunDependentActivityWithTopic(ActivityMonitor m, string dependentTopic)
        {
            StupidStringClient.Entry[] dependentLogs = null;
            m.DependentActivity().LaunchWithTopic(token => { dependentLogs = RunDependentActivity(token); }, dependentTopic);
            return dependentLogs;
        }

        private static StupidStringClient.Entry[] RunDependentActivity(ActivityMonitor.DependentToken token)
        {
            string depMonitorTopic = null;
            StupidStringClient.Entry[] dependentLogs = null;
            var task = Task.Factory.StartNew(t =>
           {
               StupidStringClient cStarted = new StupidStringClient();
               var depMonitor = new ActivityMonitor();
               depMonitor.Output.RegisterClient(cStarted);
               using (depMonitor.StartDependentActivity( token))
               {
                   depMonitorTopic = depMonitor.Topic;
                   depMonitor.Trace().Send("Hello!");
               }
               dependentLogs = cStarted.Entries.ToArray();
           }, token);
            task.Wait();
            depMonitorTopic.Should().Be(token.Topic);
            return dependentLogs;
        }
    }
}
