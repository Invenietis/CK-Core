using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
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

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                _monitor.Info( "I'm buggy: I'm logging back in my monitor!" );
                base.OnUnfilteredLog( tags, level, text, logTimeUtc );
            }
        }

        internal class NotBuggyActivityMonitorClient : ActivityMonitorClient
        {
            private  int _number;
            internal NotBuggyActivityMonitorClient( int number )
            {
                _number = number;
            }

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                Console.WriteLine( "NotBuggyActivityMonitorClient echo : {0}", _number );
            }
        }

        internal class ActionActivityMonitorClient : ActivityMonitorClient
        {
            Action _log;
            internal ActionActivityMonitorClient( Action log )
            {
                _log = log;
            }

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                _log();
            }
        }

        internal class WaitActivityMonitorClient : ActivityMonitorClient
        {
            readonly object _locker = new object();
            bool _done = false;

            readonly object _outLocker = new object();
            bool _outDone = false;

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
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
        public void ExhibeReentrancyAndMultiThreadErrors()
        {
            ActivityMonitor.AutoConfiguration.Clear();
            ActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
            WaitActivityMonitorClient client = monitor.Output.RegisterClient( new WaitActivityMonitorClient() );

            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ) );

            try
            {
                Task.Factory.StartNew( () =>
                {
                    monitor.Info( "Test must work in task" );
                } );

                client.WaitForOnUnfilteredLog();

                Assert.That( () => monitor.Info( "Test must fail" ),
                    Throws.TypeOf( typeof( InvalidOperationException ) ).
                        And.Message.EqualTo( R.ActivityMonitorConcurrentThreadAccess ) );
                
                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 2 ), "Still 2: Concurrent call: not the fault of the Client." );
            }
            finally
            {
                client.Free();
            }

            Thread.Sleep( 50 );
            monitor.Info( "Test must work after task" );

            monitor.Output.RegisterClient( new ActionActivityMonitorClient( () =>
            {
                Assert.That( () => monitor.Info( "Test must fail reentrant client" ),
                    Throws.TypeOf( typeof( InvalidOperationException ) ).
                        And.Message.EqualTo( R.ActivityMonitorReentrancyError ) );
            } ) );

            monitor.Info( "Test must work after reentrant client" );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 3 ), "The RegisterClient action above is ok: it checks that it triggered a reentrant call." );

            monitor.Output.RegisterClient( new ActionActivityMonitorClient( () =>
            {
                monitor.Info( "Test must fail reentrant client" );
            } ) );

            monitor.Info( "Test must work after reentrant client" );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 3 ), "The BUGGY RegisterClient action above is NOT ok: it let the a reentrant call exception => We have removed it." );

        }

        [Test]
        public void ReentrancyMultiThread()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );

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
                new Task( () => { getLock(); monitor.Info( "Test T1" ); } ),
                new Task( () => { getLock(); monitor.Info( new Exception(), "Test T2" ); } ),
                new Task( () => { getLock(); monitor.Info( "Test T3" ); } ),
                new Task( () => { getLock(); monitor.Info( new Exception(), "Test T4" ); } ),
                new Task( () => { getLock(); monitor.Info( "Test T5" ); } ),
                new Task( () => { getLock(); monitor.Info( new Exception(), "Test T6" ); } ),
                new Task( () => { getLock(); monitor.Info( "Test T7" ); } ),
                new Task( () => { getLock(); monitor.Info( new Exception(), "Test T8" ); } )
            };

            Parallel.ForEach( tasks, t => t.Start() );

            lock( lockRunner )
                while( enteredThread < 8 )
                    Monitor.Wait( lockRunner );

            lock( lockTasks )
                Monitor.PulseAll( lockTasks );

            Assert.Throws<AggregateException>( () => Task.WaitAll( tasks ) );

            CollectionAssert.AllItemsAreInstancesOfType( tasks.Where( x => x.IsFaulted ).
                                                                SelectMany( x => x.Exception.Flatten().InnerExceptions ),
                                                                typeof( InvalidOperationException ) );

            Assert.DoesNotThrow( () => monitor.Info( "Test" ) );
        }

        [Test]
        public void ReentrancyMonoThread()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            int clientCount = monitor.Output.Clients.Count;
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ) );

            BuggyActivityMonitorClient client = new BuggyActivityMonitorClient( monitor );
            monitor.Output.RegisterClient( client );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount + 1 ) );
            monitor.Info( "Test" );
            Assert.That( monitor.Output.Clients.Count, Is.EqualTo( clientCount ) );

            Assert.DoesNotThrow( () => monitor.Info( "Test" ) ); 
        }

        [Test]
        public void MultiThread()
        {
            IActivityMonitor monitor = new ActivityMonitor();
            monitor.Output.BridgeTo( TestHelper.ConsoleMonitor );
            var initCount = monitor.Output.Clients.Count;
            NotBuggyActivityMonitorClient[] clients = new NotBuggyActivityMonitorClient[]
            {
                new NotBuggyActivityMonitorClient(0),
                new NotBuggyActivityMonitorClient(1),
                new NotBuggyActivityMonitorClient(2),
                new NotBuggyActivityMonitorClient(3),
                new NotBuggyActivityMonitorClient(4),
                new NotBuggyActivityMonitorClient(5),
                new NotBuggyActivityMonitorClient(6),
                new NotBuggyActivityMonitorClient(7),
                new NotBuggyActivityMonitorClient(8),
                new NotBuggyActivityMonitorClient(9),
                new NotBuggyActivityMonitorClient(10),
                new NotBuggyActivityMonitorClient(11),
                new NotBuggyActivityMonitorClient(12),
                new NotBuggyActivityMonitorClient(13),
                new NotBuggyActivityMonitorClient(14),
                new NotBuggyActivityMonitorClient(15),
                new NotBuggyActivityMonitorClient(16),
                new NotBuggyActivityMonitorClient(17),
                new NotBuggyActivityMonitorClient(18),
                new NotBuggyActivityMonitorClient(19)
            };

            Task t = new Task( () =>
            {
                Console.WriteLine( "Internal tast Started" );

                Parallel.For( 0, 20, i => { monitor.Output.RegisterClient( clients[i] ); } );

                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( 20 + initCount ) );

                Thread.Sleep( 100 );

                Parallel.For( 0, 20, i => { monitor.Output.UnregisterClient( clients[i] ); } );

                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( initCount ) );

                Thread.Sleep( 100 );

                Random r = new Random();

                Parallel.For( 0, 20, i =>
                {
                    Console.WriteLine( "Add : {0}", i );
                    monitor.Output.RegisterClient( clients[i] );
                    Thread.Sleep( (int)Math.Round( r.NextDouble() * 50, 0 ) );
                    Console.WriteLine( "Remove : {0}", i );
                    monitor.Output.UnregisterClient( clients[i] );
                } );

                Assert.That( monitor.Output.Clients.Count, Is.EqualTo( initCount ) );

            } );

            t.Start();
            for( int i = 0; i < 50; i++ )
            {
                monitor.Info( "Ok go : " + i );
                Thread.Sleep( 10 );
            }
            t.Wait();
            if( t.Exception != null ) throw t.Exception;
            Assert.That( t.Exception, Is.Null );
        }
    }
}
