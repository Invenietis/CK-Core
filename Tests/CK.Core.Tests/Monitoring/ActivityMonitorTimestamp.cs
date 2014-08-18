#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\ActivityMonitorTimestamp.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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

        [Test]
        public void TestNaturalCollision()
        {
            ActivityMonitor m = new ActivityMonitor( applyAutoConfigurations: false );
            var detect = new DateCollision();
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
            Assert.That( detect.NbClash, Is.EqualTo( 0 ) );
        }

        [Test]
        public void TestArtificialCollision()
        {
            ActivityMonitor m = new ActivityMonitor( applyAutoConfigurations: false );
            var detect = new DateCollision();
            m.Output.RegisterClient( detect );

            DateTimeStamp now = DateTimeStamp.UtcNow;
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", now, null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", now, null );
            m.UnfilteredLog( ActivityMonitor.Tags.Empty, LogLevel.Info, "This should clash!", new DateTimeStamp( now.TimeUtc.AddDays( -1 ) ), null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Info, null, "This should clash!", now, null );
            m.UnfilteredOpenGroup( ActivityMonitor.Tags.Empty, LogLevel.Info, null, "This should clash!", new DateTimeStamp( now.TimeUtc.AddTicks( -1 ) ), null );
            m.CloseGroup( new DateTimeStamp( now.TimeUtc.AddTicks( -1 ) ) );
            m.CloseGroup( now );

            Assert.That( detect.NbClash, Is.EqualTo( 0 ) );
        }
    }
}
