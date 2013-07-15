using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
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
    }
}
