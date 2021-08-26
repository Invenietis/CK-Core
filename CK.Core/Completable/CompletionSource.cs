using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// This is a TaskCompletionSource-like object that allows exceptions or cancellations
    /// to be ignored (see <see cref="ICompletable.OnError(Exception, ref OnError)"/>) and
    /// <see cref="ICompletable.OnCanceled(ref OnCanceled)"/>.
    /// <para>
    /// This is the counterpart of the <see cref="CompletionSource{TResult}"/> that can transform
    /// errors or cancellations into specific results.
    /// It's much more useful than this one, but for the sake of symmetry, this "no result" class exposes the same functionalities.
    /// </para>
    /// <para>
    /// By design, the completable is not exposed by the completion (it may be an adapter that has no real semantics).
    /// If required (for instance if the completable is a Command that should be accessible from its completion), this can be easily
    /// done without changing this base implementation: simply create a generic Completion&lt;TCompletable&gt; where TCompletable is ICompletable
    /// and expose the protected <see cref="Holder"/> property as a TCompletable Completable property.
    /// </para>
    /// </summary>
    public class CompletionSource : ICompletion, ICompletionSource
    {
        /// Adapter waiting for .Net 5 TaskCompletionSource.
        readonly TaskCompletionSource<object?> _tcs;
        readonly ICompletable _holder;
        volatile Exception? _exception;
        volatile int _state;

        /// <summary>
        /// Creates a <see cref="CompletionSource"/>.
        /// </summary>
        /// <param name="holder">The completion's holder.</param>
        public CompletionSource( ICompletable holder )
        {
            _tcs = new TaskCompletionSource<object?>( TaskCreationOptions.RunContinuationsAsynchronously );
            _holder = holder ?? throw new ArgumentNullException( nameof(holder) );
        }

        /// <summary>
        /// Gets the command that holds this completion.
        /// </summary>
        protected ICompletable Holder => _holder;

        /// <inheritdoc />
        public Task Task => _tcs.Task;

        /// <summary>
        /// Gets an awaiter for this completion.
        /// </summary>
        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        /// <inheritdoc />
        public Exception? OriginalException => _exception;

        /// <inheritdoc />
        public bool IsCompleted => _state != 0;

        /// <inheritdoc />
        public bool HasSucceed => (_state & 1) != 0;

        /// <inheritdoc />
        public bool HasFailed => (_state & 2) != 0;

        /// <inheritdoc />
        public bool HasBeenCanceled => (_state & 4) != 0;

        /// <summary>
        /// Transitions the <see cref="Task"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// An <see cref="InvalidOperationException"/> is thrown if Task is already in one of the three final
        /// states: <see cref="TaskStatus.RanToCompletion"/>, <see cref="TaskStatus.Faulted"/> or <see cref="TaskStatus.Canceled"/>.
        /// </summary>
        public void SetResult()
        {
            if( _state == 0 ) _state = 1;
            _tcs.SetResult( null );
        }

        /// <summary>
        /// Attempts to transition the <see cref="Task"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <returns>
        /// True if the operation was successful; false if the operation was unsuccessful.
        /// </returns>
        public bool TrySetResult()
        {
            if( _state != 0 ) return false;
            _state |= 1;
            if( _tcs.TrySetResult( null ) )
            {
                return true;
            }
            _state &= ~1;
            return false;
        }

        /// <summary>
        /// Enables <see cref="ICompletable.OnError(Exception, ref OnError)"/> to
        /// transform exceptions into successful or canceled completion.
        /// </summary>
        public ref struct OnError
        {
            readonly CompletionSource _c;
            internal Exception? ResultError;
            internal bool Called;
            internal bool ResultCancel;

            internal OnError( CompletionSource c )
            {
                _c = c;
                Called = false;
                ResultError = null;
                ResultCancel = false;
            }

            internal static void ThrowMustBeCalledOnlyOnce()
            {
                throw new InvalidOperationException( "OnError methods must be called only once." );
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource.TrySetException(Exception)"/> has been called)
            /// the exception.
            /// The default <see cref="ICompletable.OnError(Exception, ref OnError)"/> calls this method.
            /// </summary>
            /// <param name="ex">The exception to set.</param>
            public void SetException( Exception ex )
            {
                if( Called ) ThrowMustBeCalledOnlyOnce();
                Called = true;
                ResultError = ex;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource.TrySetException(Exception)"/> has been called)
            /// a successful completion instead of an error.
            /// <para>
            /// Note that the <see cref="CompletionSource.HasFailed"/> will be true: the fact that the command
            /// did not properly complete is available.
            /// </para>
            /// </summary>
            public void SetResult()
            {
                if( Called ) ThrowMustBeCalledOnlyOnce();
                Called = true;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource.TrySetException(Exception)"/> has been called)
            /// a cancellation completion instead of an error.
            /// <para>
            /// Note that the <see cref="CompletionSource.HasFailed"/> will be true: the fact that the command
            /// did not properly complete is available.
            /// </para>
            /// </summary>
            public void SetCanceled()
            {
                if( Called ) ThrowMustBeCalledOnlyOnce();
                Called = true;
                ResultCancel = true;
            }

        }

        /// <inheritdoc />
        public void SetException( Exception exception )
        {
            // Fast path if already resolved: the framework exception will be raised.
            // This protects the current state.
            // Only on concurrent SetException will the state be inconsistent.
            if( _state != 0 ) _tcs.SetException( exception );
            var o = new OnError( this );
            _holder.OnError( exception, ref o );
            if( !o.Called ) ThrowOnErrorCalledRequired();
            _state |= 2;
            _exception = exception;
            if( o.ResultError != null )
            {
                _tcs.SetException( o.ResultError );
            }
            else if( o.ResultCancel )
            {
                _tcs.SetCanceled();
            }
            else
            {
                _tcs.SetResult( null );
            }
        }

        /// <inheritdoc />
        public bool TrySetException( Exception exception )
        {
            if( _state != 0 ) return false;
            var o = new OnError( this );
            _holder.OnError( exception, ref o );
            if( !o.Called ) ThrowOnErrorCalledRequired();
            _state |= 2;
            _exception = exception;
            if( o.ResultError != null )
            {
                if( !_tcs.TrySetException( o.ResultError ) )
                {
                    _state &= ~2;
                    _exception = null;
                    return false;
                }
            }
            else if( o.ResultCancel )
            {
                if( !_tcs.TrySetCanceled() )
                {
                    _state &= ~2;
                    _exception = null;
                    return false;
                }
            }
            else
            {
                if( !_tcs.TrySetResult( null ) )
                {
                    _state &= ~2;
                    _exception = null;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Enables <see cref="ICompletable.OnError(Exception, ref OnError)"/> to
        /// transform exceptions into successful or canceled completion.
        /// </summary>
        public ref struct OnCanceled
        {
            readonly CompletionSource _c;
            internal bool Called;
            internal bool ResultSuccess;

            internal OnCanceled( CompletionSource c )
            {
                _c = c;
                Called = false;
                ResultSuccess = false;
            }
            internal static void ThrowMustBeCalledOnlyOnce()
            {
                throw new InvalidOperationException( "OnCanceled methods must be called only once." );
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="TrySetCanceled()"/> has been called)
            /// a successful instead of a cancellation completion.
            /// <para>
            /// Note that the <see cref="HasBeenCanceled"/> will be true: the fact that the command
            /// has been canceled is available.
            /// </para>
            /// </summary>
            public void SetResult()
            {
                if( Called ) ThrowMustBeCalledOnlyOnce();
                Called = true;
                ResultSuccess = true;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="TrySetCanceled()"/> has been called)
            /// the cancellation completion.
            /// The <see cref="ICompletable"/> OnCanceled method default implementation calls this method.
            /// <para>
            /// Note that the <see cref="HasBeenCanceled"/> will be true: the fact that the command
            /// has been canceled is available.
            /// </para>
            /// </summary>
            public void SetCanceled()
            {
                if( Called ) ThrowMustBeCalledOnlyOnce();
                Called = true;
            }

        }

        /// <inheritdoc />
        public void SetCanceled()
        {
            if( _state != 0 ) _tcs.SetCanceled();
            var o = new OnCanceled( this );
            _holder.OnCanceled( ref o );
            if( !o.Called ) ThrowOnCancelCalledRequired();
            _state |= 4;
            if( o.ResultSuccess )
            {
                _tcs.SetResult( null );
            }
            else
            {
                _tcs.SetCanceled();
            }
        }

        /// <inheritdoc />
        public bool TrySetCanceled()
        {
            if( _state != 0 ) return false;
            var o = new OnCanceled( this );
            _holder.OnCanceled( ref o );
            if( !o.Called ) ThrowOnCancelCalledRequired();
            _state |= 4;
           if( o.ResultSuccess )
            {
                if( !_tcs.TrySetResult( null ) )
                {
                    _state &= ~4;
                    return false;
                };
            }
            else
            {
                if( !_tcs.TrySetCanceled() )
                {
                    _state &= ~4;
                    return false;
                }
            }
            return true;
        }

        internal static void ThrowOnCancelCalledRequired()
        {
            throw new InvalidOperationException( "One of the OnCanceled methods must be called." );
        }

        internal static void ThrowOnErrorCalledRequired()
        {
            throw new InvalidOperationException( "One of the OnError methods must be called." );
        }


        /// <summary>
        /// Overridden to return the current completion status.
        /// </summary>
        /// <returns>The current status.</returns>
        public override string ToString() => GetStatus( _tcs.Task.Status, _state );

        static internal string GetStatus( TaskStatus t, int s )
        {
            return t switch
            {
                TaskStatus.RanToCompletion => (s & 2) != 0
                                                ? "Completed (HasFailed)"
                                                : (s & 4) != 0
                                                    ? "Completed (HasBeenCanceled)"
                                                    : "Success",
                TaskStatus.Canceled => (s & 2) != 0
                                        ? "Canceled (HasFailed)"
                                        : "Canceled",
                TaskStatus.Faulted => "Failed",
                _ => "Waiting"
            };
        }
    }
}
