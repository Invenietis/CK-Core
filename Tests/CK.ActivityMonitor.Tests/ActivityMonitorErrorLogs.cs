using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using Xunit;
using FluentAssertions;

namespace CK.Core.Tests.Monitoring
{

    class SafeClient : IActivityMonitorClient
    {
        readonly StringBuilder _buffer = new StringBuilder();
        string _prefix = "";

        public void OnUnfilteredLog( ActivityMonitorLogData data )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendFormat( "[{0}]{1}", data.Level, data.Text ).AppendLine();
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            lock( _buffer )
            {
                _buffer.Append( _prefix ).AppendFormat( ">[{0}]{1}", group.GroupLevel, group.GroupText ).AppendLine();
                _prefix += " ";
            }
        }

        public void OnGroupClosing( IActivityLogGroup group, ref System.Collections.Generic.List<ActivityLogGroupConclusion> conclusions )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendFormat( "[Closing]{0}", group.GroupText ).AppendLine();
        }

        public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            lock( _buffer )
            {
                _prefix = _prefix.Substring( 0, _prefix.Length - 1 );
                _buffer.Append( _prefix ).AppendFormat( "<[Closed]{0}", group.GroupText ).AppendLine();
            }
        }

        public override string ToString()
        {
            lock( _buffer ) return _buffer.ToString();
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }
    }

    class BuggyClient : IActivityMonitorClient
    {
        readonly ThreadContext _c;
        readonly double _probFailPerCall;
        public readonly int CreatedThreadId;
        public int RunThreadId;
        public readonly int NumClient;
        public bool Failed;
        public bool FailureHasBeenReceivedThroughEvent;

        public BuggyClient( ThreadContext c, int numClient, double probFailPerCall = 0.2 )
        {
            NumClient = numClient;
            _c = c;
            _probFailPerCall = probFailPerCall;
            CreatedThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public override string ToString()
        {
            return String.Format( "BuggyClient n°{0} created on thread {1}, running on {2}. Failed={3}", NumClient, CreatedThreadId, RunThreadId, Failed );
        }

        void MayFail()
        {
            if( _c.Rand.NextDouble() < _probFailPerCall )
            {
                Failed = true;
                throw new CKException( "BuggyClient{0} failed.", NumClient );
            }
        }

        #region IActivityMonitorClient Members

        public void OnUnfilteredLog( ActivityMonitorLogData data )
        {
            MayFail();
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            MayFail();
        }

        public void OnGroupClosing( IActivityLogGroup group, ref System.Collections.Generic.List<ActivityLogGroupConclusion> conclusions )
        {
            MayFail();
        }

        public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            MayFail();
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }

        #endregion

    }

    class ThreadContext
    {
        readonly ActivityMonitorErrorLogs _context;
        readonly IActivityMonitor _monitor;
        readonly public int NumMonitor;
        readonly public int OperationCount;
        readonly public Random Rand;

        public ThreadContext( ActivityMonitorErrorLogs context, int numMonitor, int buggyClientCount, int operationCount )
        {
            _context = context;
            NumMonitor = numMonitor;
            OperationCount = operationCount;
            Rand = new Random();
            _monitor = CreateMonitorWithBuggyListeners( buggyClientCount );
        }

        public void Run()
        {
            _monitor.Info().Send( "ThreadContext{0}Begin", NumMonitor );
            foreach( var bc in _monitor.Output.Clients.OfType<BuggyClient>() )
            {
                bc.RunThreadId = Thread.CurrentThread.ManagedThreadId;
            }
            for( int i = 0; i < OperationCount; ++i )
            {
                double op = Rand.NextDouble();
                if( op < 1.0 / 60 ) _monitor.MinimalFilter = _monitor.MinimalFilter == LogFilter.Trace ? LogFilter.Verbose : LogFilter.Trace;
                
                if( op < 1.0/3 ) _monitor.Info().Send( "OP-{0}-{1}", NumMonitor, i );
                else if( op < 2.0/3 ) _monitor.OpenInfo().Send( "G-OP-{0}-{1}", NumMonitor, i );
                else _monitor.CloseGroup();
            }
            _monitor.Info().Send( "ThreadContext{0}End", NumMonitor );
        }

        IActivityMonitor CreateMonitorWithBuggyListeners( int buggyClientCount )
        {
            IActivityMonitor monitor = new ActivityMonitor();
            for( int i = 0; i < buggyClientCount; ++i ) monitor.Output.RegisterClient( _context.NewBuggyClient( this ) );
            monitor.Output.RegisterClient( _context.SafeClient );
            return monitor;
        }

        internal void CheckResult( string clientText )
        {
             clientText.Should().Contain( $"ThreadContext{NumMonitor}Begin" );
             clientText.Should().Contain( $"ThreadContext{NumMonitor}End" );
        }
    }

    public class ActivityMonitorErrorLogs : MutexTest<ActivityMonitor>
    {
        internal SafeClient SafeClient;
        Random _random;
        List<BuggyClient> _buggyClients;
        List<ThreadContext> _contexts;
        double _probFailurePerOperation;


        [Fact]
        public void MonitorErrorTrap()
        {
            using (LockFact())
            {
                // Run this test for at least X second.
                int nbSecond = 2;
                Stopwatch w = new Stopwatch();
                w.Start();
                do
                {
                    OneRun(threadCount: 30, operationCount: 225);
                }
                while (w.ElapsedMilliseconds < nbSecond * 1000);
            }
        }

        void InitializeEnv(int threadCount, int operationCount, int buggyClientCount, double probFailurePerOperation, double probBuggyOnErrorHandlerFailure)
        {
            SafeClient = new SafeClient();
            _random = new Random();
            _buggyClients = new List<BuggyClient>();
            _contexts = new List<ThreadContext>();
            _probFailurePerOperation = probFailurePerOperation;
            for (int i = 0; i < threadCount; ++i) _contexts.Add(new ThreadContext(this, _contexts.Count, buggyClientCount, operationCount));
            _inSafeErrorHandler = false;
            _maxNumberOfErrorReceivedAtOnce = 0;
            _lastSequenceNumberReceived = ActivityMonitor.CriticalErrorCollector.NextSequenceNumber - 1;
            _errorsFromBackground = new ConcurrentBag<string>();
            _probBuggyOnErrorHandlerFailure = probBuggyOnErrorHandlerFailure;
            _buggyOnErrorHandlerFailCount = 0;
            _buggyOnErrorHandlerReceivedCount = 0;
            _nbClearedWhileRaised = 0;
            _nbNotClearedWhileRaised = 0;
        }

        internal BuggyClient NewBuggyClient(ThreadContext c)
        {
            var r = new BuggyClient(c, _buggyClients.Count, _random.NextDouble() / 5);
            _buggyClients.Add(r);
            return r;
        }

        void OneRun(int threadCount, int operationCount)
        {
            ActivityMonitor.CriticalErrorCollector.Clear();
            ActivityMonitor.CriticalErrorCollector.Capacity = 300;

            InitializeEnv(
                threadCount: threadCount,
                buggyClientCount: 10,
                operationCount: 250,
                probFailurePerOperation: 0.9 / operationCount,
                probBuggyOnErrorHandlerFailure: 0.1);

            ActivityMonitor.CriticalErrorCollector.OnErrorFromBackgroundThreads += SafeOnErrorHandler;
            ActivityMonitor.CriticalErrorCollector.OnErrorFromBackgroundThreads += BuggyOnErrorHandler;

            int nextSeq = ActivityMonitor.CriticalErrorCollector.NextSequenceNumber;
            RunAllAndWaitForTermination();

            ActivityMonitor.CriticalErrorCollector.OnErrorFromBackgroundThreads -= SafeOnErrorHandler;
            ActivityMonitor.CriticalErrorCollector.OnErrorFromBackgroundThreads -= BuggyOnErrorHandler;

            if (TestHelper.LogsToConsole)
            {
                Console.WriteLine(@"ActivityMonitor.LoggingError Test:
                ThreadCount: {0}
                DispatchQueuedWorkItemCount: {1}
                OptimizedDispatchQueuedWorkItemCount: {2}
                Errors handled: {3}
                Errors from Error handler: {4}
                Errors Cleared while raised: {5}
                Errors not Cleared while raised: {6}",
                    threadCount,
                    ActivityMonitor.CriticalErrorCollector.DispatchQueuedWorkItemCount,
                    ActivityMonitor.CriticalErrorCollector.OptimizedDispatchQueuedWorkItemCount,
                    ActivityMonitor.CriticalErrorCollector.NextSequenceNumber - nextSeq,
                    _buggyOnErrorHandlerReceivedCount,
                    _nbClearedWhileRaised,
                    _nbNotClearedWhileRaised);
            }
            _errorsFromBackground.Should().BeEmpty();
            var buggyClientMismatch = _buggyClients.Where(c => c.Failed != c.FailureHasBeenReceivedThroughEvent).ToArray();
            buggyClientMismatch.Should().BeEmpty();
            string clientText = SafeClient.ToString();
            for (int i = 0; i < _contexts.Count; ++i) _contexts[i].CheckResult(clientText);
            _buggyOnErrorHandlerReceivedCount.Should().BeGreaterThan(0, "There must be at least one error from the buggy handler.");
            _buggyOnErrorHandlerReceivedCount.Should().Be(_buggyOnErrorHandlerFailCount);

            ActivityMonitor.CriticalErrorCollector.DispatchQueuedWorkItemCount.Should().BeGreaterThan(0, "Of course, events have been raised...");
            ActivityMonitor.CriticalErrorCollector.OptimizedDispatchQueuedWorkItemCount.Should().BeGreaterThan(0, "Optimizations must have saved us some works.");
            ActivityMonitor.CriticalErrorCollector.Capacity.Should().Be(500, "Changed in SafeOnErrorHandler.");
            _nbClearedWhileRaised.Should().BeGreaterThan(0, "Clear is called from SafeOnErrorHandler each 20 errors.");

            ActivityMonitor.CriticalErrorCollector.Clear();
        }


        bool _inSafeErrorHandler;
        int _maxNumberOfErrorReceivedAtOnce;
        int _lastSequenceNumberReceived;
        int _nbClearedWhileRaised;
        int _nbNotClearedWhileRaised;
        // One can not use Assert.That from a background thread since it throws... an exception that we handle :-).
        // Instead we collect strings.
        ConcurrentBag<string> _errorsFromBackground;

        void SafeOnErrorHandler(object source, CriticalErrorCollector.ErrorEventArgs e)
        {
            if (_inSafeErrorHandler)
            {
                _errorsFromBackground.Add("Error events are not raised simultaneously.");
                return;
            }

            // As soon as the first error, we increase the capacity to avoid losing any error.
            // This tests the tread-safety of the operation and shows that no deadlock occur (we are 
            // receiving an error event and can safely change the internal buffer capacity).
            ActivityMonitor.CriticalErrorCollector.Capacity = 500;

            _inSafeErrorHandler = true;
            _maxNumberOfErrorReceivedAtOnce = Math.Max(_maxNumberOfErrorReceivedAtOnce, e.LoggingErrors.Count);
            foreach (var error in e.LoggingErrors)
            {
                if (error.SequenceNumber % 10 == 0)
                {
                    int clearedErrors;
                    int notYetRaisedErrors;
                    ActivityMonitor.CriticalErrorCollector.Clear(out clearedErrors, out notYetRaisedErrors);
                    _nbClearedWhileRaised += clearedErrors;
                    _nbNotClearedWhileRaised += notYetRaisedErrors;
                }
                if (_lastSequenceNumberReceived != error.SequenceNumber - 1)
                {
                    _errorsFromBackground.Add(String.Format("Received {0}, expected {1}.", error.SequenceNumber - 1, _lastSequenceNumberReceived));
                    _inSafeErrorHandler = false;
                    return;
                }
                _lastSequenceNumberReceived = error.SequenceNumber;
                string msg = error.Exception.Message;

                if (msg.StartsWith("BuggyClient"))
                {
                    int idx = Int32.Parse(Regex.Match(msg, "\\d+").Value);
                    _buggyClients[idx].FailureHasBeenReceivedThroughEvent = true;
                }
                else if (msg.StartsWith("BuggyErrorHandler"))
                {
                    ++_buggyOnErrorHandlerReceivedCount;
                    if (!error.ToString().StartsWith(Impl.CoreResources.ErrorWhileCollectorRaiseError))
                    {
                        _errorsFromBackground.Add("Bad comment for error handling.");
                        _inSafeErrorHandler = false;
                        return;
                    }
                }
                else
                {
                    _errorsFromBackground.Add("Unexpected error: " + error.Exception.Message);
                    _inSafeErrorHandler = false;
                    return;
                }
            }
            _inSafeErrorHandler = false;
        }

        double _probBuggyOnErrorHandlerFailure;
        int _buggyOnErrorHandlerFailCount;
        int _buggyOnErrorHandlerReceivedCount;

        void BuggyOnErrorHandler(object source, CriticalErrorCollector.ErrorEventArgs e)
        {
            // Force at least one error regardless of the probability.
            if (_buggyOnErrorHandlerFailCount == 0 || _random.NextDouble() < _probBuggyOnErrorHandlerFailure)
            {
                // Subscribe again to this buggy event.
                ActivityMonitor.CriticalErrorCollector.OnErrorFromBackgroundThreads += BuggyOnErrorHandler;
                throw new CKException("BuggyErrorHandler{0}", _buggyOnErrorHandlerFailCount++);
            }
        }

        void RunAllAndWaitForTermination()
        {
            int monitorCount = _contexts.Count;
            (monitorCount % 2).Should().Be(0);
            Task[] tasks = new Task[monitorCount / 2];
            Thread[] threads = new Thread[monitorCount / 2];

            for (int i = 0; i < tasks.Length; i++) tasks[i] = new Task(_contexts[i].Run);
            for (int i = 0; i < threads.Length; i++) threads[i] = new Thread(_contexts[tasks.Length + i].Run);

            for (int i = 0; i < monitorCount; i++)
                if (i % 2 == 0) threads[i / 2].Start();
                else tasks[i / 2].Start();

            Task.WaitAll(tasks);
            for (int i = 0; i < threads.Length; i++) threads[i].Join();

            // Instead of:
            //
            // while( ActivityMonitor.LoggingError.OnErrorFromBackgroundThreadsPending ) Thread.Sleep( 1 );
            //
            // The right way to wait for something to happen is to block a thread until a signal unblocks it.
            // This is what the following function is doing.
            ActivityMonitor.CriticalErrorCollector.WaitOnErrorFromBackgroundThreadsPending();
            ActivityMonitor.CriticalErrorCollector.OnErrorFromBackgroundThreadsPending
               .Should().BeFalse("Since nobody calls ActivityMonitor.Add. In real situations, this would not necessarily be true.");
        }

    }
}
