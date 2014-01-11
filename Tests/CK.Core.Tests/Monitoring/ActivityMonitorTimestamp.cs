using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    public class ActivityMonitorTimestamp
    {
        class DateCollision : IActivityMonitorClient
        {
            LogTimestamp _lastOne;
            
            public int NbClash;

            public void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                if( data.LogTime <= _lastOne ) ++NbClash;
                _lastOne = data.LogTime;
            }

            public void OnOpenGroup( IActivityLogGroup group )
            {
                if( group.LogTime <= _lastOne ) ++NbClash;
                _lastOne = group.LogTime;
            }

            public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
            {
            }

            public void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
                if( group.CloseLogTime <= _lastOne ) ++NbClash;
                _lastOne = group.CloseLogTime;
            }

            public void OnTopicChanged( string newTopic, string fileName, int lineNumber )
            {
            }

            public void OnAutoTagsChanged( CKTrait newTrait )
            {
            }
        }

        [Test]
        public void TestNaturalCollision()
        {
            ActivityMonitor m = new ActivityMonitor( applyAutoConfigurations: false );
            var detect = new DateCollision();
            m.Output.RegisterClient( detect );
            for( int i = 0; i < 10; ++i )
            {
                m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", LogTimestamp.UtcNow, null );
            }
            for( int i = 0; i < 10; ++i )
            {
                m.Trace().Send( "This should clash!" );
            }
            for( int i = 0; i < 10; ++i )
            {
                using( m.OpenTrace().Send( "This should clash!" ) )
                {
                }
            }
            Assert.That( detect.NbClash, Is.EqualTo( 0 ) );
        }

        [Test]
        public void TestArtificialCollision()
        {
            ActivityMonitor m = new ActivityMonitor( applyAutoConfigurations: false );
            var detect = new DateCollision();
            m.Output.RegisterClient( detect );

            LogTimestamp now = LogTimestamp.UtcNow;
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", now, null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", now, null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", new LogTimestamp( now.TimeUtc.AddDays( -1 ) ), null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Info, null, "This should clash!", now, null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Info, null, "This should clash!", new LogTimestamp( now.TimeUtc.AddTicks( -1 ) ), null );
            m.CloseGroup( new LogTimestamp( now.TimeUtc.AddTicks( -1 ) ) );
            m.CloseGroup( now );

            Assert.That( detect.NbClash, Is.EqualTo( 0 ) );
        }
    }
}
