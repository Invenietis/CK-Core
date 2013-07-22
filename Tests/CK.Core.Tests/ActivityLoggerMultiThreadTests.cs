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
    [Category( "ActivityLogger" )]
    public class ActivityLoggerMultiThreadTests
    {
        internal class BuggyActivityLoggerClient : ActivityLoggerClient
        {
            private  IActivityLogger _logger;
            internal BuggyActivityLoggerClient( IActivityLogger logger )
            {
                _logger = logger;
            }

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                _logger.Info( "Je suis buggé et je log dans le logger dont je suis client" );
                base.OnUnfilteredLog( tags, level, text, logTimeUtc );
            }
        }

        internal class NotBuggyActivityLoggerClient : ActivityLoggerClient
        {
            private  int _number;
            internal NotBuggyActivityLoggerClient( int number )
            {
                _number = number;
            }

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                Console.WriteLine( "NotBuggyActivityLoggerClient echo : {0}", _number );
            }
        }

        internal class ActionActivityLoggerClient : ActivityLoggerClient
        {
            Action _log;
            internal ActionActivityLoggerClient( Action log )
            {
                _log = log;
            }

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                _log();
            }
        }

        internal class WaitActivityLoggerClient : ActivityLoggerClient
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

            internal void WaitForWait()
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
            DefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Tap.Register( new ActivityLoggerConsoleSink() );
            WaitActivityLoggerClient client = new WaitActivityLoggerClient();
            logger.Output.RegisterClient( client );

            try
            {
                Task.Factory.StartNew( () =>
                {
                    logger.Info( "Test must work in task" );
                } );

                client.WaitForWait();

                Assert.That( () => logger.Info( "Test must fail" ),
                    Throws.TypeOf( typeof( InvalidOperationException ) ).
                        And.Message.EqualTo( R.ActivityLoggerConcurrentThreadAccess ) );
            }
            finally
            {
                client.Free();
            }

            Thread.Sleep( 50 );
            logger.Info( "Test must work after task" );

            logger.Output.RegisterClient( new ActionActivityLoggerClient( () =>
            {
                Assert.That( () => logger.Info( "Test must fail reentrant client" ),
                    Throws.TypeOf( typeof( InvalidOperationException ) ).
                        And.Message.EqualTo( R.ActivityLoggerReentrancyError ) );
            } ) );

            logger.Info( "Test must work with reentrant client" );
            logger.Info( "Test must work after reentrant client" );
        }

        [Test]
        public void ReentrancyMultiThread()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Tap.Register( new ActivityLoggerConsoleSink() );

            Task[] tasks = new Task[] 
            {            
                new Task( () => { logger.Info( "Test T1" ); } ),
                new Task( () => { logger.Info( "Test T2" ); } ),
                new Task( () => { logger.Info( "Test T3" ); } ),
                new Task( () => { logger.Info( "Test T4" ); } ),
                new Task( () => { logger.Info( "Test T5" ); } ),
                new Task( () => { logger.Info( "Test T6" ); } ),
                new Task( () => { logger.Info( "Test T7" ); } ),
                new Task( () => { logger.Info( "Test T8" ); } )
            };

            Parallel.ForEach( tasks, t => t.Start() );
            Assert.Throws<AggregateException>( () => Task.WaitAll( tasks ) );

            CollectionAssert.AllItemsAreInstancesOfType( tasks.Where( x => x.IsFaulted ).
                                                                SelectMany( x => x.Exception.Flatten().InnerExceptions ),
                                                                typeof( InvalidOperationException ) );

            Assert.DoesNotThrow( () => logger.Info( "Test" ) );
        }

        [Test]
        public void ReentrancyMonoThread()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            int clientCount = logger.Output.RegisteredClients.Count;
            Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( clientCount ) );
            logger.Tap.Register( new ActivityLoggerConsoleSink() );
            BuggyActivityLoggerClient client = new BuggyActivityLoggerClient( logger );
            logger.Output.RegisterClient( client );
            Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( clientCount + 1 ) );
            logger.Info( "Test" );
            Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( clientCount ) );

            logger.Info( "Test" ); // Expected no exceptions
        }

        [Test]
        public void MultiThread()
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            logger.Tap.Register( new ActivityLoggerConsoleSink() );
            var initCount = logger.Output.RegisteredClients.Count;
            NotBuggyActivityLoggerClient[] clients = new NotBuggyActivityLoggerClient[]
            {
                new NotBuggyActivityLoggerClient(0),
                new NotBuggyActivityLoggerClient(1),
                new NotBuggyActivityLoggerClient(2),
                new NotBuggyActivityLoggerClient(3),
                new NotBuggyActivityLoggerClient(4),
                new NotBuggyActivityLoggerClient(5),
                new NotBuggyActivityLoggerClient(6),
                new NotBuggyActivityLoggerClient(7),
                new NotBuggyActivityLoggerClient(8),
                new NotBuggyActivityLoggerClient(9),
                new NotBuggyActivityLoggerClient(10),
                new NotBuggyActivityLoggerClient(11),
                new NotBuggyActivityLoggerClient(12),
                new NotBuggyActivityLoggerClient(13),
                new NotBuggyActivityLoggerClient(14),
                new NotBuggyActivityLoggerClient(15),
                new NotBuggyActivityLoggerClient(16),
                new NotBuggyActivityLoggerClient(17),
                new NotBuggyActivityLoggerClient(18),
                new NotBuggyActivityLoggerClient(19)
            };

            Task t = new Task( () =>
            {
                Console.WriteLine( "Internal tast Started" );

                Parallel.For( 0, 20, i => { logger.Output.RegisterClient( clients[i] ); } );

                Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( 20 + initCount ) );

                Thread.Sleep( 100 );

                Parallel.For( 0, 20, i => { logger.Output.UnregisterClient( clients[i] ); } );

                Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( initCount ) );

                Thread.Sleep( 100 );

                Random r = new Random();

                Parallel.For( 0, 20, i =>
                {
                    Console.WriteLine( "Add : {0}", i );
                    logger.Output.RegisterClient( clients[i] );
                    Thread.Sleep( (int)Math.Round( r.NextDouble() * 50, 0 ) );
                    Console.WriteLine( "Remove : {0}", i );
                    logger.Output.UnregisterClient( clients[i] );
                } );

                Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( initCount ) );

            } );

            t.Start();
            for( int i = 0; i < 50; i++ )
            {
                logger.Info( "Ok go : " + i );
                Thread.Sleep( 10 );
            }
            t.Wait();
            if( t.Exception != null ) throw t.Exception;
            Assert.That( t.Exception, Is.Null );
        }
    }
}
