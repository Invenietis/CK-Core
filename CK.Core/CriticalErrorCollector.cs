#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\CriticalErrorCollector.cs) is part of CiviKey. 
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
            internal Error( string c, Exception e, int n, int lostErrorCount )
            {
                Comment = c;
                Exception = e;
                SequenceNumber = n;
                LostErrorCount = lostErrorCount;
            }

            /// <summary>
            /// Holds the count of errors that have been discarded: too many critical errors occur
            /// in a too short time.
            /// When this field is greater than zero, this indicates a serious problem.
            /// </summary>
            public readonly int LostErrorCount;

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
            /// Overridden to return <see cref="Comment"/> and <see cref="P:Exception"/> message.
            /// </summary>
            /// <returns>Explicit content.</returns>
            public override string ToString()
            {
                if( LostErrorCount > 0 )
                    return String.Format( "{0} - {1} - Lost Critical Error count: {2}.",  Comment, Exception.Message, LostErrorCount );
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
        // This is an optimization: it avoids queuing a new work item
        // if one already exists and has not yet started to dispatch error.
        int _dispatchWorkItemIsReady;
        // This is to measure the impact of the _dispatchWorkItemIsReady optimization.
        int _savedDispatchQueuedWorkItemCount;
        // Lost error count: the capacity is too short. Errors have been discarded.
        int _lostErrorCount;

        /// <summary>
        /// Fires when an error has been <see cref="Add"/>ed (there cannot be more than one thread that raises this event at the same time).
        /// Raising this event is itself protected: if an exception is raised by one of the registered EventHandler, the culprit is removed 
        /// from the OnErrorFromBackgroundThreads list of delegates, the exception is appended in the collector, and a new event will 
        /// be raised (to the remaining handlers).
        /// <para>Caution: the event always fire on a background thread (adding an error is not a blocking operation).</para>
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnErrorFromBackgroundThreads;

        /// <summary>
        /// Initializes a new <see cref="CriticalErrorCollector"/> with a default <see cref="Capacity"/> set to 128.
        /// </summary>
        public CriticalErrorCollector()
        {
            _collector = new FIFOBuffer<Error>( 128 );
            _raiseLock = new object();
            _endOfWorkLock = new object();
        }

        /// <summary>
        /// Gets or sets the maximal number of errors kept by this collector.
        /// Defaults to 128 (which should be enough).
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
            if( _lostErrorCount > 1024 ) return;
            if( comment == null ) comment = String.Empty;
            lock( _collector )
            {
                if( _waitingRaiseCount >= _collector.Capacity )
                {
                    Interlocked.Increment( ref _lostErrorCount );
                    return;
                }
                Interlocked.Increment( ref _waitingRaiseCount );
                _collector.Push( new Error( comment, ex, _seqNumber++, _lostErrorCount ) );
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
                            // h.GetInvocationList() creates an independent copy of Delegate[].
                            foreach( EventHandler<ErrorEventArgs> d in h.GetInvocationList() )
                            {
                                try
                                {
                                    d( this, e );
                                }
                                catch( Exception ex2 )
                                {
                                    // Since this thread will loop, flags it to avoid 
                                    // creating useless new dispatching queue items.
                                    Interlocked.Exchange( ref _dispatchWorkItemIsReady, 1 );
                                    Interlocked.Increment( ref _waitingRaiseCount );
                                    OnErrorFromBackgroundThreads -= (EventHandler<ErrorEventArgs>)d;
                                    lock( _collector )
                                    {
                                        if( _collector.Count == _collector.Capacity ) Interlocked.Increment( ref _lostErrorCount );
                                        else _collector.Push( new Error( R.ErrorWhileCollectorRaiseError, ex2, _seqNumber++, _lostErrorCount ) );
                                    }
                                    again = true;
                                }
                            }
                        }
                    }
                }
            }
            while( again );

            // Signals the _endOfWorkLock monitor if _waitingRaiseCount reached 0.
            if( Interlocked.Add( ref _waitingRaiseCount, -raisedCount ) == 0 )
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
        /// <param name="cleared">Number of suppressed errors.</param>
        /// <param name="waitingToBeRaisedErrors">The number of errors waiting to be raised.</param>
        public void Clear( out int cleared, out int waitingToBeRaisedErrors )
        {
            cleared = 0;
            lock( _collector )
            {
                // Items are from oldest to newest: take the index of the first one that has not been raised yet.
                int idx = _collector.IndexOf( e => e.SequenceNumber >= _lastSeqNumberRaising );
                if( idx < 0 )
                {
                    cleared = _collector.Count;
                    waitingToBeRaisedErrors = 0;
                    _collector.Clear();
                }
                else
                {
                    cleared = idx;
                    waitingToBeRaisedErrors = _collector.Count - idx;
                    _collector.Truncate( waitingToBeRaisedErrors );
                }
            }
        }

        /// <summary>
        /// Clears the list. Only errors that have been already raised by <see cref="OnErrorFromBackgroundThreads"/>
        /// are removed from the internal buffer: it can be safely called at any time.
        /// </summary>
        public void Clear()
        {
            int cleared = 0;
            int waiting = 0;
            Clear( out cleared, out waiting );
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
        /// <returns>An independent array. May be empty but never null.</returns>
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
