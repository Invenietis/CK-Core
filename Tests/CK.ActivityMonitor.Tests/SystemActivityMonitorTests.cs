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
                SystemActivityMonitor.LowLevelErrorEventArgs catched = null;
                EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs> h = (sender, e) => catched = e;
                SystemActivityMonitor.OnError += h;
                try
                {
                    ActivityMonitor.CriticalErrorCollector.Add(new CKException("The-Test-Exception-Message"), "Produced by SystemActivityMonitorTests.SimpleTest");
                    ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                    catched.Should().NotBeNull();
                    catched.ErrorWhileWritingLogFile.Should().BeNull();
                    catched.ErrorMessage.Should().Contain("The-Test-Exception-Message");
                    catched.ErrorMessage.Should().Contain("Produced by SystemActivityMonitorTests.SimpleTest");
                    File.ReadAllText(catched.FullLogFilePath).Should().Be(catched.ErrorMessage);
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
                int buggyEventHandlerCount = 0;

                var goodCollector = new List<string>();
                Action<string> addMsg = s => 
                {
                    lock(goodCollector) { goodCollector.Add(s); }
                };

                var hGood = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>((sender, e) => addMsg( e.ErrorMessage ) );
                var hBad = new EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs>((sender, e) => { ++buggyEventHandlerCount; throw new Exception("From buggy handler."); });
                SystemActivityMonitor.OnError += hGood;
                SystemActivityMonitor.OnError += hBad;
                try
                {
                    ActivityMonitor.CriticalErrorCollector.Add(new CKException("The-Test-Exception-Message"), "First call to SystemActivityMonitorTests.OnErrorEventIsSecured");
                    ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                    buggyEventHandlerCount.Should().Be(1);
                    if( goodCollector.Count != 2 )
                    {
                        string.Join(Environment.NewLine+"-"+ Environment.NewLine, goodCollector)
                            .Should().Be( "Only 2 messages should have been received." );
                    }
                    goodCollector.Count.Should().Be(2, "We also received the error of the buggy handler :-).");

                    ActivityMonitor.CriticalErrorCollector.Add(new CKException("The-Test-Exception-Message"), "Second call to SystemActivityMonitorTests.OnErrorEventIsSecured");
                    ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
                    goodCollector.Count.Should().Be(3);
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
