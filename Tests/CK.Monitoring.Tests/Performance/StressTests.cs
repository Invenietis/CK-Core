using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    [Fixture]
    public class StressTests
    {

        class RunContextParam
        {
            /// <summary>
            /// This is much more small than the default 64*1024 default.
            /// </summary>
            public int DispatcherMaxCapacity = 10000;
            public bool UseLocalTestStrategy = false;
            
            /// <summary>
            /// The default value is 20. 
            /// This stress test creates a lot of events in a very short period of time (at least with SendingThreadProbToSleep sets to 0).
            /// For this to have an effect it must be set to 0 or 1.
            /// </summary>
            public int HandlerExtraLoad = 20;
            public int PoolThreadCount = 5;
            public int NewThreadCount = 25;
            public int LoopCount = 400;
            public double SendingThreadProbToSleep = 0.0;
        }

        class RunContext : IDisposable
        {
            public readonly GrandOutput GrandOutput;
            public readonly int _newThreadCount;
            public readonly int LoopCount;
            readonly Barrier Barrier;
            int _perfTraceCount;

            public RunContext( RunContextParam parameters )
            {
                GrandOutput = CreateGrandOutputWithFakeHandler( parameters.HandlerExtraLoad, parameters.UseLocalTestStrategy, parameters.DispatcherMaxCapacity );
                LoopCount = parameters.LoopCount;
                Barrier = new Barrier( parameters.PoolThreadCount + parameters.NewThreadCount + 1 );
                _newThreadCount = parameters.NewThreadCount;
            }

            public int TotalThreadCount { get { return Barrier.ParticipantCount - 1; } }

            public int IncrementPerfTrace()
            {
                return Interlocked.Increment( ref _perfTraceCount );
            }

            public int RunAndGetPerfTraceCount( Action<RunContext, IActivityMonitor, Random> a )
            {
                _perfTraceCount = 0;
                CK.Monitoring.GrandOutputHandlers.FakeHandler.HandlePerfTraceCount = 0;
                CK.Monitoring.GrandOutputHandlers.FakeHandler.TotalHandleCount = 0;
                CK.Monitoring.GrandOutputHandlers.FakeHandler.SizeHandled = 0;

                for( int i = 0; i < TotalThreadCount - _newThreadCount; ++i ) ThreadPool.QueueUserWorkItem( Run, a );
                for( int i = 0; i < _newThreadCount; ++i ) new Thread( Run ).Start( a );
                Barrier.SignalAndWait();
                Barrier.SignalAndWait();
                Assert.That( Barrier.ParticipantsRemaining, Is.EqualTo( Barrier.ParticipantCount ) );
                return _perfTraceCount;
            }

            void Run( object state )
            {
                Random r = new Random();
                var a = (Action<RunContext, IActivityMonitor,Random>)state;
                IActivityMonitor m = new ActivityMonitor( false );
                GrandOutput.Register( m );
                Barrier.SignalAndWait();
                for( int i = 0; i < LoopCount; ++i )
                {
                    a( this, m, r );
                }
                Barrier.SignalAndWait();
            }

            static GrandOutput CreateGrandOutputWithFakeHandler( int handlerExtralLoad, bool useAdaptive, int dispatcherMaxCapacity )
            {
                IActivityMonitor mLoad = new ActivityMonitor( false );

                GrandOutputConfiguration c = new GrandOutputConfiguration();
                var textConfig = @"<GrandOutputConfiguration><Add Type=""FakeHandler, CK.Monitoring.Tests"" Name=""GlobalCatch"" ExtraLoad="""
                    + handlerExtralLoad.ToString()
                    + @""" /></GrandOutputConfiguration>";
                Assert.That( c.Load( XDocument.Parse( textConfig ).Root, mLoad ) );
                Assert.That( c.ChannelsConfiguration.Configurations.Count, Is.EqualTo( 1 ) );
                SystemActivityMonitor.RootLogPath = TestHelper.TestFolder;

                IGrandOutputDispatcherStrategy strat;
                if( useAdaptive )
                {
                    strat = new Impl.EventDispatcherLocalTestStrategy( dispatcherMaxCapacity );
                }
                else 
                {
                    strat = new Impl.EventDispatcherBasicStrategy( dispatcherMaxCapacity );
                }
                GrandOutput g = new GrandOutput( strat );
                g.SetConfiguration( c, mLoad );
                return g;
            }


            public void Dispose()
            {
                GrandOutput.Dispose();
            }
        }

        static int RunStressTestAndGetLostMessageCount( RunContextParam p )
        {
            int nbThreads = p.PoolThreadCount + p.NewThreadCount;
            SystemActivityMonitor.RootLogPath = null;
            int criticalErrorCount = 0;
            EventHandler<SystemActivityMonitor.LowLevelErrorEventArgs> h = ( sender, e ) => ++criticalErrorCount;
            SystemActivityMonitor.OnError += h;
            int nbLost = 0;
            int maxQueuedCount = 0;
            int sampleReentrantCount = 0;
            try
            {
                int nbCalls = 0;
                using( RunContext c = new RunContext( p ) )
                {
                    c.RunAndGetPerfTraceCount( ( context, monitor, random ) =>
                    {
                        Interlocked.Increment( ref nbCalls );
                        using( monitor.OpenTrace().Send( "A group..." ) )
                        {
                            monitor.Trace().Send( "PerfTrace: {0}", context.IncrementPerfTrace() );
                            if( p.SendingThreadProbToSleep > random.NextDouble() )
                            {
                                int ms = random.Next( 10 );
                                Thread.Sleep( ms );
                            }
                        }
                    } );
                    Assert.That( nbCalls, Is.EqualTo( nbThreads * p.LoopCount ) );
                    nbLost = c.GrandOutput.LostEventCount;
                    maxQueuedCount = c.GrandOutput.MaxQueuedCount;
                    sampleReentrantCount = c.GrandOutput.IgnoredConcurrentCallCount;
                }
                ActivityMonitor.MonitoringError.WaitOnErrorFromBackgroundThreadsPending();
            }
            finally
            {
                SystemActivityMonitor.OnError -= h;
            }
            int theoricalTotal = nbThreads * p.LoopCount * 3;
            int receivedTotal = CK.Monitoring.GrandOutputHandlers.FakeHandler.TotalHandleCount;
            TestHelper.ConsoleMonitor.Info().Send( "Local Test Strategy:{6} - Total should be {0}, Total received = {1}, Binary size = {2},  MaxQueuedCount={3}, Number of lost messages={4}, IgnoredConcurrentCallCount={7}, Number of Critical Errors={5}.",
                theoricalTotal,
                receivedTotal,
                CK.Monitoring.GrandOutputHandlers.FakeHandler.SizeHandled,
                maxQueuedCount,
                nbLost,
                criticalErrorCount,
                p.UseLocalTestStrategy,
                sampleReentrantCount );
            if( receivedTotal == theoricalTotal )
            {
                Assert.That( CK.Monitoring.GrandOutputHandlers.FakeHandler.HandlePerfTraceCount, Is.EqualTo( nbThreads * p.LoopCount ) );
            }
            else
            {
                Assert.That( criticalErrorCount > 0 );
                Assert.That( receivedTotal, Is.EqualTo( theoricalTotal - nbLost ) );
            }
            return nbLost;
        }

        /// <summary>
        /// This is more a laboratory than a test...
        /// </summary>
        [Test]
        public void ThroughputHypothesisAndBlockInputs()
        {
            RunContextParam p = new RunContextParam();
            Assert.That( p.SendingThreadProbToSleep == 0 );

            RunStressTestAndGetLostMessageCount( p );            
            p.SendingThreadProbToSleep = 0.05;
            do
            {
                p.SendingThreadProbToSleep += 0.1;
                if( p.SendingThreadProbToSleep > 1.0 ) break;
                TestHelper.ConsoleMonitor.Info().Send( "Prob to sleep: {0}", p.SendingThreadProbToSleep );
            }
            while( RunStressTestAndGetLostMessageCount( p ) > 0 );
        }

    }
}
