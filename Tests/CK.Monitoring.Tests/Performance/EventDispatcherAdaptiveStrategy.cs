#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Performance\EventDispatcherAdaptiveStrategy.cs) is part of CiviKey. 
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Monitoring.Impl
{
    public sealed class EventDispatcherLocalTestStrategy : IGrandOutputDispatcherStrategy
    {
        readonly int _maxCapacity;
        readonly int _reenableCapacity;
        readonly int _samplingCount;
        Func<int> _count;
        int _sample;
        int _sampleReentrantFlag;
        int _ignoredConcurrentCallCount;
        bool _opened;

        public EventDispatcherLocalTestStrategy( int maxCapacity = 64*1024, int reenableCapacity = 0, int samplingCount = 0 )
        {
            if( maxCapacity < 1000 || (reenableCapacity > 0 && maxCapacity < reenableCapacity) ) throw new ArgumentException();
            _maxCapacity = maxCapacity;
            _reenableCapacity = reenableCapacity > 0 ? reenableCapacity : (4 * maxCapacity) / 5;
            _samplingCount = samplingCount > 0 ? samplingCount : maxCapacity / 10;
        }

        void IGrandOutputDispatcherStrategy.Initialize( Func<int> instantLoad, Thread dispatcher, out Func<int,int> idleManager )
        {
            _count = instantLoad;
            _opened = true;
            _sample = _samplingCount;
            dispatcher.Priority = ThreadPriority.Normal;
            idleManager = i => -1;
        }

        public int IgnoredConcurrentCallCount
        {
            get { return _ignoredConcurrentCallCount; }
        }
        
        public bool IsOpened( ref int maxQueuedCount )
        {
            if( Interlocked.Decrement( ref _sample ) == 0 )
            {
                if( Interlocked.CompareExchange( ref _sampleReentrantFlag, 1, 0 ) == 1 ) Interlocked.Increment( ref _ignoredConcurrentCallCount );
                else
                {
                    int waitingCount = _count();
                    if( maxQueuedCount < waitingCount ) maxQueuedCount = waitingCount;
                    Thread.MemoryBarrier();
                    if( _opened )
                    {
                        if( waitingCount > _maxCapacity )
                        {
                            _opened = false;
                            Thread.MemoryBarrier();
                        }
                    }
                    else
                    {
                        if( waitingCount > _reenableCapacity )
                        {
                            _opened = true;
                            Thread.MemoryBarrier();
                        }
                    }
                    Interlocked.Exchange( ref _sampleReentrantFlag, 0 );
                    Interlocked.Exchange( ref _sample, _samplingCount );
                }
                return _opened;
            }
            Thread.MemoryBarrier();
            return _opened;
        }

    }
}
