using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Monitoring.Impl
{
    /// <summary>
    /// Implements a basic strategy that handles activities logging overloads.
    /// </summary>
    public sealed class EventDispatcherBasicStrategy : IGrandOutputDispatcherStrategy
    {
        readonly int _maxCapacity;
        readonly int _reenableCapacity;
        readonly int _samplingCount;
        Func<int> _count;
        int _sample;
        int _sampleReentrantFlag;
        int _ignoredConcurrentCallCount;
        bool _opened;

        /// <summary>
        /// Initializes a new basic strategy. 
        /// Default parameters should be used.
        /// </summary>
        /// <param name="maxCapacity">Maximum capacity.</param>
        /// <param name="reenableCapacity">Defaults to 4/5 of the maximum capacity.</param>
        /// <param name="samplingCount">Actual check of the queue count is done by default each 1/10 of the maximum capacity.</param>
        public EventDispatcherBasicStrategy( int maxCapacity = 256*1024, int reenableCapacity = 0, int samplingCount = 0 )
        {
            if( maxCapacity < 1000 || (reenableCapacity > 0 && maxCapacity < reenableCapacity) ) throw new ArgumentException();
            _maxCapacity = maxCapacity;
            _reenableCapacity = reenableCapacity > 0 ? reenableCapacity : ( 4 * maxCapacity ) / 5;
            _samplingCount = samplingCount > 0 ? samplingCount : maxCapacity / 10;
        }

        /// <summary>
        /// Gets the count of concurrent sampling (when this strategy has been called while it was already called by another thread).
        /// </summary>
        public int IgnoredConcurrentCallCount
        {
            get { return _ignoredConcurrentCallCount; }
        }

        void IGrandOutputDispatcherStrategy.Initialize( Func<int> instantLoad, Thread dispatcher, out Func<int,int> idleManager )
        {
            _count = instantLoad;
            _opened = true;
            _sample = _samplingCount;
            dispatcher.Priority = ThreadPriority.Normal;
            idleManager = IdleManager;
        }

        static readonly int[] _idleTimes = new[] { 20, 100, 400, 800, 1600, 2400, 4*1000, 10*000, 30*1000, 1*60*1000, 5*60*1000, 10*60*1000, 20*60*1000  };
        static int IdleManager( int idleCount )
        {
            if( idleCount >= _idleTimes.Length ) return 30 * 60 * 1000;
            return _idleTimes[idleCount];
        }

        bool IGrandOutputDispatcherStrategy.IsOpened( ref int maxQueuedCount )
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
