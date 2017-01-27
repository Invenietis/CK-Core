using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Xunit;
using FluentAssertions;

namespace CK.Core.Tests.Monitoring
{
    public class SystemActivityMonitorTests : MutexTest<ActivityMonitor>
    {
        public SystemActivityMonitorTests()
        {
            var logs = Path.Combine( TestHelper.TestFolder, SystemActivityMonitor.SubDirectoryName );
            TestHelper.CleanupFolder(logs);
            SystemActivityMonitor.RootLogPath = TestHelper.TestFolder;
        }

        [Fact]
        public void SimpleTest()
        {
            using (LockFact())
            {
                bool eventHasBeenRaised = false;
                var h = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>(
                        delegate (object sender, SystemActivityMonitor.LowLevelErrorEventArgs e)
                        {
                            e.ErrorWhileWritingLogFile.Should().BeNull();
                            e.ErrorMessage.Should().Contain("The-Test-Exception-Message");
                            e.ErrorMessage.Should().Contain("Produced by SystemActivityMonitorTests.SimpleTest");
                            File.ReadAllText(e.FullLogFilePath).Should().Be(e.ErrorMessage);
                            eventHasBeenRaised = true;
                        });
                SystemActivityMonitor.OnError += h;
                try
                {
                    ActivityMonitor.CriticalErrorCollector.Add(new CKException("The-Test-Exception-Message"), "Produced by SystemActivityMonitorTests.SimpleTest");
                    ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                    eventHasBeenRaised.Should().BeTrue();
                }
                finally
                {
                    SystemActivityMonitor.OnError -= h;
                }
            }
        }

        [Fact]
        public void OnErrorEventIsSecured()
        {
            using (LockFact())
            {
                int eventHandlerCount = 0;
                int buggyEventHandlerCount = 0;

                var hGood = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>((sender, e) => { ++eventHandlerCount; });
                var hBad = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>((sender, e) => { ++buggyEventHandlerCount; throw new Exception("From buggy handler."); });
                SystemActivityMonitor.OnError += hGood;
                SystemActivityMonitor.OnError += hBad;
                try
                {
                    ActivityMonitor.CriticalErrorCollector.Add(new CKException("The-Test-Exception-Message"), "First call to SystemActivityMonitorTests.OnErrorEventIsSecured");
                    ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                    eventHandlerCount.Should().Be(2, "We also received the error of the buggy handler :-).");
                    buggyEventHandlerCount.Should().Be(1);

                    ActivityMonitor.CriticalErrorCollector.Add(new CKException("The-Test-Exception-Message"), "Second call to SystemActivityMonitorTests.OnErrorEventIsSecured");
                    ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                    eventHandlerCount.Should().Be(3);
                    buggyEventHandlerCount.Should().Be(1);
                }
                finally
                {
                    SystemActivityMonitor.OnError -= hGood;
                    SystemActivityMonitor.OnError -= hBad;
                }
            }
        }

    }
}
