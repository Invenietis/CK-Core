using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// This collector keeps <see cref="Capacity"/> <see cref="Error"/>s (and no more).
    /// It raises <see cref="OnErrorFromBackgroundThreads"/> event on each <see cref="Add"/>.
    /// It is totally thread-safe and guaranties (as long as its Capacity is big enough) that no error can be lost
    /// (even errors raised while dispatching the event are themselves collected) and that errors are dispatched in
    /// sequence.
    /// <para>
    /// This class is typically used as a static property or field by any object that must handle unexpected errors. (It can also be used
    /// per-instance if it makes sense.)
    /// </para>
    /// </summary>
    public class CriticalErrorCollector
    {
        /// <summary>
        /// Encapsulates error information <see cref="CriticalErrorCollector.Add"/>ed by external code
        /// or raised by a <see cref="CriticalErrorCollector.OnErrorFromBackgroundThreads"/> event itself.
        /// </summary>
        public struct Error
        {
            internal Error( string c, Exception e, int n )
            {
                Comment = c;
                Exception = e;
                SequenceNumber = n;
            }

            /// <summary>
            /// Unique, increasing, sequence number.
            /// </summary>
            public readonly int SequenceNumber;

            /// <summary>
            /// The origin or a description of the <see cref="P:Exception"/>.
            /// Never null but can be empty if no comment is provided while calling <see cref="CriticalErrorCollector.Add"/>.
            /// </summary>
            public readonly string Comment;
            
            /// <summary>
            /// The exception.
            /// </summary>
            public readonly Exception Exception;

            /// <summary>
            /// Overriden to return <see cref="Comment"/> and <see cref="P:Exception"/> message.
            /// </summary>
            /// <returns>Explicit content.</returns>
            public override string ToString()
            {
                return Comment + " - " + Exception.Message;
            }

        }

        /// <summary>
        /// Event argument of <see cref="CriticalErrorCollector.OnErrorFromBackgroundThreads"/>.
        /// </summary>
        public class ErrorEventArgs : EventArgs
        {
            internal ErrorEventArgs( Error[] e )
            {
                LoggingErrors = e.ToReadOnlyList();
            }

            /// <summary>
            /// The <see cref="Error"/>s. When more than one error exist, the oldest come first.
            /// </summary>
            public readonly IReadOnlyList<Error> LoggingErrors;
        }


        readonly FIFOBuffer<Error> _collector;
        readonly object _raiseLock;
        readonly object _endOfWorkLock;
        int _seqNumber;
        int _lastSeqNumberRaising;
        int _waitingRaiseCount;
        // Number of queued work items that have been created since the creation of this collector.
        int _dispatchQueuedWorkItemCount;
        // This is an optimisation: it avoids queing a new work item
        // if one already exists and has not yet started to dispatch error.
        int _dispatchWorkItemIsReady;
        // This is to measure the impact of the _dispatchWorkItemIsReady optimisation.
        int _savedDispatchQueuedWorkItemCount;

        /// <summary>
        /// Fires when an error has been <see cref="Add"/>ed (there cannot be more than one thread that raises this event at the same time).
        /// Raising this event is itself protected: if an exception is raised by one of the registered EventHandler, the culprit is removed 
        /// from the OnErrorFromBackgroundThreads list of delegates, the exception is appended in the collector, and a new event will 
        /// be raised (to the remaining handlers).
        /// <para>Caution: the event always fire on a background thread (adding an error is not a blocking operation).</para>
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnErrorFromBackgroundThreads;

        /// <summary>
        /// Initializes a new <see cref="CriticalErrorCollector"/> with a default <see cref="Capacity"/> set to 64.
        /// </summary>
        public CriticalErrorCollector()
        {
            _collector = new FIFOBuffer<Error>( 64 );
            _raiseLock = new object();
            _endOfWorkLock = new object();
        }

        /// <summary>
        /// Gets or sets the maximal number of errors kept by this collector.
        /// Defaults to 64 (which should be enough).
        /// It can be safely changed at any time.
        /// </summary>
        public int Capacity
        {
            get { lock( _collector ) return _collector.Capacity; }
            set { lock( _collector ) _collector.Capacity = value; }
        }

        /// <summary>
        /// Adds a critical, unexpected error.
        /// </summary>
        /// <param name="comment">Comment associated to the error (such as the name of the culprit). Can be null.</param>
        /// <param name="ex">The unexpected exception. Must not be null.</param>
        public void Add( Exception ex, string comment )
        {
            if( ex == null ) throw new ArgumentNullException( "ex" );
            if( comment == null ) comment = String.Empty;
            Interlocked.Increment( ref _waitingRaiseCount );
            lock( _collector )
            {
                _collector.Push( new Error( comment, ex, _seqNumber++ ) );
            }
            if( Interlocked.CompareExchange( ref _dispatchWorkItemIsReady, 1, 0 ) == 0 )
            {
                Interlocked.Increment( ref _dispatchQueuedWorkItemCount );
                ThreadPool.QueueUserWorkItem( DoRaiseInBackground );
            }
            else
            {
                Interlocked.Increment( ref _savedDispatchQueuedWorkItemCount );
            }
        }

        void DoRaiseInBackground( object unusedState )
        {
            int raisedCount = 0;
            bool again;
            do
            {
                again = false;
                // This lock guaranties that no more than one event will fire at the same time.
                // Since we capture the errors to raise from inside it, it also guaranties that
                // listeners will receive errors in order.
                lock( _raiseLock )
                {
                    Interlocked.Exchange( ref _dispatchWorkItemIsReady, 0 ); 
                    ErrorEventArgs e = CreateEvent();
                    if( e != null )
                    {
                        raisedCount += e.LoggingErrors.Count;
                        // Thread-safe (C# 4.0 compiler use CompareExchange).
                        var h = OnErrorFromBackgroundThreads;
                        if( h != null )
                        {
                            // h.GetInvocationList() creates an independant copy of Delegate[].
                            foreach( EventHandler<ErrorEventArgs> d in h.GetInvocationList() )
                            {
                                try
                                {
                                    d( this, e );
                                }
                                catch( Exception ex2 )
                                {
                                    // Since this thread will loop, flags it to avoid 
                                    // creating useless new dispathing queue items.
                                    Interlocked.Exchange( ref _dispatchWorkItemIsReady, 1 );
                                    Interlocked.Increment( ref _waitingRaiseCount );
                                    OnErrorFromBackgroundThreads -= (EventHandler<ErrorEventArgs>)d;
                                    lock( _collector )
                                    {
                                        _collector.Push( new Error( R.ErrorWhileCollectorRaiseError, ex2, _seqNumber++ ) );
                                    }
                                    again = true;
                                }
                            }
                        }
                    }
                }
            }
            while( again );

            // Just for fun: a lock-free substraction...
            int w = _waitingRaiseCount;
            if( Interlocked.CompareExchange( ref _waitingRaiseCount, w - raisedCount, w ) != w )
            {
                // After a lot of readings of msdn and internet, I use the SpinWait struct...
                // This is the recommended way, so...
                // Note that tests under heavy loads show that this code is rarely solicited
                // which means that the first Interlocked.CompareExchange always work.
                // Unfortunate consequence: this code is not often covered by any tests. To test it, modify the 
                // line above to be: int w = _waitingRaiseCount + 1;
                SpinWait sw = new SpinWait();
                do
                {
                    sw.SpinOnce();
                    w = _waitingRaiseCount;
                }
                while( Interlocked.CompareExchange( ref _waitingRaiseCount, w - raisedCount, w ) != w );
            }
            // Signals the _endOfWorkLock monitor if _waitingRaiseCount reached 0.
            if( _waitingRaiseCount == 0 )
            {
                lock( _endOfWorkLock ) Monitor.PulseAll( _endOfWorkLock );
            }
        }

        ErrorEventArgs CreateEvent()
        {
            Error[] toRaise;
            lock( _collector )
            {
                if( _collector.Count == 0 ) return null;
                int currentSeqNumber = _seqNumber;
                int exCount = currentSeqNumber - _lastSeqNumberRaising;
                if( exCount == 0 ) return null;
                toRaise = new Error[Math.Min( exCount, _collector.Capacity )];
                _collector.CopyTo( toRaise );
                _lastSeqNumberRaising = currentSeqNumber;
            }
            return new ErrorEventArgs( toRaise );
        }

        /// <summary>
        /// Clears the list. Only errors that have been already raised by <see cref="OnErrorFromBackgroundThreads"/>
        /// are removed from the internal buffer: it can be safely called at any time.
        /// </summary>
        /// <returns>The number of errors waiting to be raised.</returns>
        public int Clear()
        {
            int left = 0;
            lock( _collector )
            {
                // Items are from oldest to newest: take the index of the first one that has not been raised yet.
                int idx = _collector.IndexOf( e => e.SequenceNumber >= _lastSeqNumberRaising );
                if( idx < 0 ) _collector.Clear();
                else _collector.Truncate( (left = _collector.Count - idx) );
            }
            return left;
        }

        /// <summary>
        /// Gets whether any event is waiting to be raised by <see cref="OnErrorFromBackgroundThreads"/> or is being processed.
        /// When this is false, it is guaranteed that any existing errors have been handled: if no more <see cref="Add"/> can be done
        /// it means that this collector has finished its job.
        /// Instead of pooling this property - with an horrible Thread.Sleep( 1 ), you should use <see cref="WaitOnErrorFromBackgroundThreadsPending"/> 
        /// to more efficiently and securely wait for the end of this collector's job.
        /// </summary>
        public bool OnErrorFromBackgroundThreadsPending
        {
            get { return _waitingRaiseCount != 0; }
        }

        /// <summary>
        /// Blocks the caller thread until no more event is waiting to be raised by <see cref="OnErrorFromBackgroundThreads"/> or is being processed.
        /// This is the right function to use instead of pooling <see cref="OnErrorFromBackgroundThreadsPending"/>.
        /// </summary>
        public void WaitOnErrorFromBackgroundThreadsPending()
        {
            lock( _endOfWorkLock )
                while( _waitingRaiseCount != 0 )
                    Monitor.Wait( _endOfWorkLock );
        }

        /// <summary>
        /// Obtains a copy of the last (up to) <see cref="Capacity"/> errors from oldest to newest.
        /// The newest may have not been raised by <see cref="OnErrorFromBackgroundThreads"/> yet.
        /// </summary>
        /// <returns>An independent array.</returns>
        public Error[] ToArray()
        {
            lock( _collector )
            {
                return _collector.ToArray();
            }
        }

        /// <summary>
        /// Gets the next <see cref="Error.SequenceNumber"/>.
        /// Getting this property makes sense only if this collector is not being solicited.
        /// </summary>
        public int NextSequenceNumber
        {
            get { return _seqNumber; }
        }

        /// <summary>
        /// Gets the number of internally created queued work items 
        /// since this collector exists.
        /// </summary>
        public int DispatchQueuedWorkItemCount
        {
            get { return _dispatchQueuedWorkItemCount; }
        }

        /// <summary>
        /// Gets the number of work items that have been saved since another one 
        /// was ready to dispatch the events.
        /// This is a measure of an internal optimization that makes sense only under
        /// heavy loads (unit tests).
        /// </summary>
        public int OptimizedDispatchQueuedWorkItemCount
        {
            get { return _savedDispatchQueuedWorkItemCount; }
        }


    }

}
