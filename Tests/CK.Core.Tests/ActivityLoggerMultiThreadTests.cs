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
            internal NotBuggyActivityLoggerClient(int number)
            {
                _number = number;
            }

            protected override void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
            {
                Console.WriteLine( "NotBuggyActivityLoggerClient echo : ", _number );
            }
        }

        [Test]
        public void Reentrancy()
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
                new NotBuggyActivityLoggerClient(9)
            };

            Task t = new Task( () =>
                {

                    Parallel.For( 0, 10, i => { logger.Output.RegisterClient( clients[i] ); } );

                    Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( 10 + initCount ) );

                    Thread.Sleep( 100 );

                    Parallel.For( 0, 10, i => { logger.Output.UnregisterClient( clients[i] ); } );

                    Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( initCount ) );

                    Thread.Sleep( 100 );

                    Parallel.For( 0, 10, i => { logger.Output.RegisterClient( clients[i] ); Thread.Sleep( 50 ); logger.Output.UnregisterClient( clients[i] ); } );

                    Assert.That( logger.Output.RegisteredClients.Count, Is.EqualTo( initCount ) );

                } );

            t.Start();
            for( int i = 0; i < 100; i++ )
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
