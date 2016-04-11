using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture] 
    [Category( "ActivityMonitor" )]
    public class ActivityMonitorMultiThreadTests
    {
        internal class BuggyActivityMonitorClient : ActivityMonitorClient
        {
            private  IActivityMonitor _monitor;
            internal BuggyActivityMonitorClient( IActivityMonitor monitor )
            {
                _monitor = monitor;
            }

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                _monitor.Info().Send( "I'm buggy: I'm logging back in my monitor!" );
                base.OnUnfilteredLog( data );
            }
        }

        internal class NotBuggyActivityMonitorClient : ActivityMonitorClient
        {
            private  int _number;
            internal NotBuggyActivityMonitorClient( int number )
            {
                _number = number;
            }

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                if( TestHelper.LogsToConsole ) Console.WriteLine( "NotBuggyActivityMonitorClient.OnUnfilteredLog n°{0}: {1}", _number, data.Text );
            }
        }

        internal class ActionActivityMonitorClient : ActivityMonitorClient
        {
            Action _action;
            internal ActionActivityMonitorClient( Action log )
            {
                _action = log;
            }

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                _action();
            }
        }

        internal class WaitActivityMonitorClient : ActivityMonitorClient
        {
            readonly object _locker = new object();
            bool _done = false;

            readonly object _outLocker = new object();
            bool _outDone = false;

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                lock( _locker )
                {
                    lock( _outLocker )
                    {
                        _outDone = true;
                        Monitor.PulseAll( _outLocker );
                    }
                    while( !_done )
                        Monitor.Wait( _locker );
                }
            }

            internal void WaitForOnUnfilteredLog()
            {
                lock( _outLocker )
                    while( !_outDone )
                        Monitor.Wait( _outLocker );
            }

            internal void Free()
            {
                lock( _locker )
                {
                    _done = true;
                    Monitor.PulseAll( _locker );
                }
            }
        }

        [Test]
        public void buggy_clients_are_removed_from_Output()
        {
            ActivityMonitor.AutoConfiguration = null;
            ActivityMonitor monitor = new ActivityMonitor();

            int clientCount = 0;
            if( TestHelper.LogsToConsole )
            {
                ++clientCount;
                monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
            }
            ++clientCount;
            WaitActivityMonitorClient client = monitor.Output.RegisterClient( new WaitActivityMonitorClient() );

            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ) );

            try
            {
                Task.Factory.StartNew( () =>
                {
                    monitor.Info().Send( "Test must work in task" );
                } );

                client.WaitForOnUnfilteredLog();

                Assert.That( () => monitor.Info().Send( "Test must fail" ),
                    Throws.TypeOf( typeof( InvalidOperationException ) ).
                        And.Message.EqualTo( Impl.ActivityMonitorResources.ActivityMonitorConcurrentThreadAccess ) );
                
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ), "Still " + clientCount + ": Concurrent call: not the fault of the Client." );
            }
            finally
            {
                client.Free();
            }

            Thread.Sleep( 50 );
            monitor.Info().Send( "Test must work after task" );

            ++clientCount;
            monitor.Output.RegisterClient( new ActionActivityMonitorClient( () =>
            {
                Assert.That( () => monitor.Info().Send( "Test must fail reentrant client" ),
                    Throws.TypeOf( typeof( InvalidOperationException ) ).
                        And.Message.EqualTo( Impl.ActivityMonitorResources.ActivityMonitorReentrancyError ) );
            } ) );

            monitor.Info().Send( "Test must work after reentrant client" );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ), "The RegisterClient action above is ok: it checks that it triggered a reentrant call." );

            ++clientCount;
            monitor.Output.RegisterClient( new ActionActivityMonitorClient( () =>
            {
                monitor.Info().Send( "Test must fail reentrant client" );
            } ) );

            monitor.Info().Send( "Test must work after reentrant client" );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount - 1 ), "The BUGGY RegisterClient action above is NOT ok: it triggers a reentrant call exception => We have removed it." );

        }

        [Test]
        public void concurrent_access_are_detected()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            if( TestHelper.LogsToConsole ) monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
            // Artficially slows down logging to ensure that concurrent access occurs.
            monitor.Output.RegisterClient( new ActionActivityMonitorClient( () => Thread.Sleep( 50 )) );

            object lockTasks = new object();
            object lockRunner = new object();
            int enteredThread = 0;

            Action getLock = () =>
            {
                lock( lockTasks )
                {
                    Interlocked.Increment( ref enteredThread );
                    lock( lockRunner )
                        Monitor.Pulse( lockRunner );
                    Monitor.Wait( lockTasks );
                }
            };

            Task[] tasks = new Task[] 
            {            
                new Task( () => { getLock(); monitor.Info().Send( "Test T1" ); } ),
                new Task( () => { getLock(); monitor.Info().Send( new Exception(), "Test T2" ); } ),
                new Task( () => { getLock(); monitor.Info().Send( "Test T3" ); } )
            };

            Parallel.ForEach( tasks, t => t.Start() );

            lock( lockRunner )
                while( enteredThread < tasks.Length )
                    Monitor.Wait( lockRunner );

            lock( lockTasks )
                Monitor.PulseAll( lockTasks );

            Assert.Throws<AggregateException>( () => Task.WaitAll( tasks ) );

            CollectionAssert.AllItemsAreInstancesOfType( tasks.Where( x => x.IsFaulted ).
                                                                SelectMany( x => x.Exception.Flatten().InnerExceptions ),
                                                                typeof( InvalidOperationException ) );

            Assert.DoesNotThrow( () => monitor.Info().Send( "Test" ) );
        }

        [Test]
        public void simple_reentrancy_detection()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            using( monitor.Output.CreateBridgeTo( TestHelper.ConsoleMonitor.Output.BridgeTarget ) )
            {
                int clientCount = monitor.Output.Clients.Count;
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ) );

                BuggyActivityMonitorClient client = new BuggyActivityMonitorClient( monitor );
                monitor.Output.RegisterClient( client );
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount + 1 ) );
                monitor.Info().Send( "Test" );
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ) );

                Assert.DoesNotThrow( () => monitor.Info().Send( "Test" ) );
            }
        }
    }
}
