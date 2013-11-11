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
        int _sampleReentrantCount;
        bool _opened;

        public EventDispatcherLocalTestStrategy( int maxCapacity = 64*1024, int reenableCapacity = 0, int samplingCount = 0 )
        {
            if( maxCapacity < 1000 || (reenableCapacity > 0 && maxCapacity < reenableCapacity) ) throw new ArgumentException();
            _maxCapacity = maxCapacity;
            _reenableCapacity = reenableCapacity > 0 ? reenableCapacity : (4 * maxCapacity) / 5;
            _samplingCount = samplingCount > 0 ? samplingCount : maxCapacity / 10;
        }

        void IGrandOutputDispatcherStrategy.Initialize( Func<int> instantLoad, Thread dispatcher )
        {
            _count = instantLoad;
            _opened = true;
            _sample = _samplingCount;
            dispatcher.Priority = ThreadPriority.Normal;
        }

        public int SampleReentrantCount
        {
            get { return _sampleReentrantCount; }
        }
        
        public bool IsOpened( ref int maxQueuedCount )
        {
            if( Interlocked.Decrement( ref _sample ) == 0 )
            {
                if( Interlocked.CompareExchange( ref _sampleReentrantFlag, 1, 0 ) == 1 ) Interlocked.Increment( ref _sampleReentrantCount );
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
