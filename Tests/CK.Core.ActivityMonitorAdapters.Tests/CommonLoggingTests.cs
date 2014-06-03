using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using NLog.Targets;
using NUnit.Framework;

namespace CK.Core.ActivityMonitorAdapters.Tests
{
    [TestFixture]
    class CommonLoggingTests
    {
        MemoryTarget _target;
        IActivityMonitor _m;

        [Test]
        public void CommonLoggingViaNLogTest()
        {
            NLogTests.NLogOutputTest( _m, _target );
        }

        [SetUp]
        public void SetUp()
        {
            // Configure Common.Logging
            var properties = new Common.Logging.Configuration.NameValueCollection();
            properties.Add( "configType", "INLINE" );

            ILoggerFactoryAdapter adapter = new Common.Logging.NLog.NLogLoggerFactoryAdapter( properties );
            Common.Logging.LogManager.Adapter = adapter;

            // Configure NLog
            _target = new MemoryTarget();
            _target.Name = "mo";
            _target.Layout = "${longdate}\t${uppercase:${level}}\t${message}\t${exception:format=tostring}";

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging( _target, NLog.LogLevel.Trace );

            // Start Common.Logging
            CommonLoggingAdapter.Initialize();

            _m = new ActivityMonitor();
        }

        [TearDown]
        public void TearDown()
        {
            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
        }
    }
}
