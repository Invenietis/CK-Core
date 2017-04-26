using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CK.Core.Tests.Monitoring
{
    public class ActivityMonitorTimestamp
    {
        class DateTimeStampCollision : IActivityMonitorClient
        {
            DateTimeStamp _lastOne;
            
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

        [Fact]
        public void DateTimeStamp_collision_can_not_happen()
        {
            ActivityMonitor m = new ActivityMonitor( applyAutoConfigurations: false );
            var detect = new DateTimeStampCollision();
            m.Output.RegisterClient( detect );
            for( int i = 0; i < 10; ++i )
            {
                m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", DateTimeStamp.UtcNow, null );
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
             detect.NbClash.Should().Be( 0 );
        }

        [Fact]
        public void DateTimeStamp_collision_can_not_happen_even_when_artificially_forcing_them()
        {
            ActivityMonitor m = new ActivityMonitor( applyAutoConfigurations: false );
            var detect = new DateTimeStampCollision();
            m.Output.RegisterClient( detect );

            DateTimeStamp now = DateTimeStamp.UtcNow;
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", now, null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", now, null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", new DateTimeStamp( now.TimeUtc.AddDays( -1 ) ), null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Info, null, "This should clash!", now, null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Info, null, "This should clash!", new DateTimeStamp( now.TimeUtc.AddTicks( -1 ) ), null );
            m.CloseGroup( new DateTimeStamp( now.TimeUtc.AddTicks( -1 ) ) );
            m.CloseGroup( now );

             detect.NbClash.Should().Be( 0 );
        }
    }
}
