#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\ActivityMonitorClientTester.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core.Impl;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    class ActivityMonitorClientTester : IActivityMonitorBoundClient
    {
        IActivityMonitorImpl _source;
        LogFilter _minimalFilter;
        int _depth;
        string[] _text;

        public LogFilter MinimalFilter
        {
            get { return _minimalFilter; }
            set
            {
                var prev = _minimalFilter;
                if( prev != value )
                {
                    _minimalFilter = value;
                    if( _source != null ) _source.SetClientMinimalFilterDirty();
                }
            }
        }

        public void AsyncSetMinimalFilter( LogLevelFilter filter, int delayMilliSeconds = 0 )
        {
            ThreadPool.QueueUserWorkItem( DoAsyncSetMinimalFilter, Tuple.Create( TimeSpan.FromMilliseconds( delayMilliSeconds ), filter ) );
        }

        class Flag { public bool Set; }

        public void AsyncSetMinimalFilterBlock( LogFilter filter, int delayMilliSeconds = 0 )
        {
            var state = Tuple.Create( TimeSpan.FromMilliseconds( delayMilliSeconds ), filter, new Flag() );
            ThreadPool.QueueUserWorkItem( DoAsyncSetMinimalFilterBlock, state );
            lock( state ) 
                while( !state.Item3.Set )
                    Monitor.Wait( state );
        }

        void DoAsyncSetMinimalFilter( object state )
        {
            var o = (Tuple<TimeSpan, LogFilter>)state;
            if( o.Item1 != TimeSpan.Zero ) Thread.Sleep( o.Item1 );
            MinimalFilter = o.Item2;
        }

        void DoAsyncSetMinimalFilterBlock( object state )
        {
            var o = (Tuple<TimeSpan, LogFilter,Flag>)state;
            if( o.Item1 != TimeSpan.Zero ) Thread.Sleep( o.Item1 );
            MinimalFilter = o.Item2;
            lock( o )
            {
                o.Item3.Set = true;
                Monitor.Pulse( o );
            }
        }

        void IActivityMonitorBoundClient.SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _source != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            if( source != null )
            {
                Interlocked.Exchange( ref _text, Util.EmptyStringArray );
                _source = source;
            }
            else _source = null;
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            Assert.That( data.FileName, Is.Not.Null.And.Not.Empty );
            Util.InterlockedAdd( ref _text, String.Format( "{0} {1} - {2} -[{3}]", new String( '>', _depth ), data.Level, data.Text, data.Tags ) ); 
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            Assert.That( group.FileName, Is.Not.Null.And.Not.Empty );
            int d = Interlocked.Increment( ref _depth );
            Util.InterlockedAdd( ref _text, String.Format( "{0} {1} - {2} -[{3}]", new String( '>', d ), group.GroupLevel, group.GroupText, group.GroupTags ) );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            Assert.That( fileName, Is.Not.Null.And.Not.Empty );
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }
    }
}
