using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// This is a <see cref="TaskCompletionSource{TResult}"/>-like object that allows exceptions or cancellations
    /// to be ignored (see <see cref="ICompletable{TResult}"/> OnError and OnCanceled protected methods).
    /// </summary>
    public class CompletionSource<TResult> : ICompletion<TResult>, ICompletionSource
    {
        readonly TaskCompletionSource<TResult> _tcs;
        readonly ICompletable<TResult> _holder;
        volatile Exception? _exception;
        volatile int _state;

        /// <summary>
        /// Creates a <see cref="CompletionSource{TResult}"/>.
        /// </summary>
        /// <param name="holder">The completion's holder.</param>
        public CompletionSource( ICompletable<TResult> holder )
        {
            _tcs = new TaskCompletionSource<TResult>( TaskCreationOptions.RunContinuationsAsynchronously );
            _holder = holder ?? throw new ArgumentNullException( nameof( holder ) );
        }

        /// <summary>
        /// Gets the this completion' holder.
        /// See note in <see cref="CompletionSource"/>.
        /// </summary>
        protected ICompletable<TResult> Holder => _holder;

        /// <summary>
        /// This is required to expose a covariant TResult.
        /// </summary>
        class Awaitable : IAwaitable<TResult>
        {
            readonly TaskAwaiter<TResult> _awaiter;

            public Awaitable( TaskAwaiter<TResult> awaiter )
            {
                _awaiter = awaiter;
            }

            public bool IsCompleted => _awaiter.IsCompleted;

            public TResult GetResult() => _awaiter.GetResult();

            public void OnCompleted( Action continuation ) => _awaiter.OnCompleted( continuation );

            public void UnsafeOnCompleted( Action continuation ) => _awaiter.UnsafeOnCompleted( continuation );
        }

        /// <summary>
        /// Gets the task that will be resolved when the command completes.
        /// </summary>
        public IAwaitable<TResult> GetAwaiter() => new Awaitable( _tcs.Task.GetAwaiter() );

        TaskAwaiter ICompletion.GetAwaiter() => ((Task)_tcs.Task).GetAwaiter();

        /// <inheritdoc />
        public Exception? OriginalException => _exception;

        /// <inheritdoc />
        public Task<TResult> Task => _tcs.Task;

        /// <inheritdoc />
        public TResult Result => _tcs.Task.Result;

        Task ICompletion.Task => _tcs.Task;

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
        /// <param name="result">The command result.</param>
        public void SetResult( TResult result )
        {
            if( _state == 0 ) _state = 1;
            _tcs.SetResult( result );
        }

        /// <summary>
        /// Attempts to transition the <see cref="Task"/> into the <see cref="TaskStatus.Canceled"/> state.
        /// </summary>
        /// <param name="result">The command result.</param>
        /// <returns>
        /// True if the operation was successful; false if the operation was unsuccessful.
        /// </returns>
        public bool TrySetResult( TResult result )
        {
            if( _state != 0 ) return false;
            _state |= 1;
            if( _tcs.TrySetResult( result ) )
            {
                return true;
            }
            _state &= ~1;
            return false;
        }

        /// <summary>
        /// Enables <see cref="ICompletable{TResult}"/> OnError to
        /// transform exceptions into successful or canceled completion.
        /// </summary>
        public ref struct OnError
        {
            readonly CompletionSource<TResult> _c;
            internal TResult Result;
            internal Exception? ResultError;
            internal bool Called;
            internal bool ResultCancel;

            internal OnError( CompletionSource<TResult> c )
            {
                _c = c;
                Result = default!;
                ResultError = default;
                Called = false;
                ResultCancel = false;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource.TrySetException(Exception)"/> has been called)
            /// the exception.
            /// The default <see cref="ICompletable{TResult}"/> OnError method calls this method.
            /// </summary>
            /// <param name="ex">The exception to set.</param>
            public void SetException( Exception ex )
            {
                if( Called ) CompletionSource.OnError.ThrowMustBeCalledOnlyOnce();
                Called = true;
                ResultError = ex;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource{TResult}.TrySetException(Exception)"/> has been called)
            /// a successful completion instead of an error.
            /// <para>
            /// Note that the <see cref="CompletionSource{TResult}.HasFailed"/> will be true: the fact that the command
            /// did not properly complete is available.
            /// </para>
            /// </summary>
            public void SetResult( TResult result )
            {
                if( Called ) CompletionSource.OnError.ThrowMustBeCalledOnlyOnce();
                Called = true;
                Result = result;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource{TResult}.TrySetException(Exception)"/> has been called)
            /// a cancellation completion instead of an error.
            /// <para>
            /// Note that the <see cref="CompletionSource{TResult}.HasFailed"/> will be true: the fact that the command
            /// did not properly complete is available.
            /// </para>
            /// </summary>
            public void SetCanceled()
            {
                if( Called ) CompletionSource.OnError.ThrowMustBeCalledOnlyOnce();
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
            if( !o.Called ) CompletionSource.ThrowOnErrorCalledRequired();
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
                _tcs.SetResult( o.Result );
            }
        }

        /// <inheritdoc />
        public bool TrySetException( Exception exception )
        {
            if( _state != 0 ) return false;
            var o = new OnError( this );
            _holder.OnError( exception, ref o );
            if( !o.Called ) CompletionSource.ThrowOnErrorCalledRequired();
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
                if( !_tcs.TrySetResult( o.Result ) )
                {
                    _state &= ~2;
                    _exception = null;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Enables <see cref="ICompletable{TResult}"/> OnCanceled method to
        /// transform exceptions into successful or canceled completion.
        /// </summary>
        public ref struct OnCanceled
        {
            readonly CompletionSource<TResult> _c;
            internal TResult Result;
            internal bool Called;
            internal bool ResultCanceled;

            internal OnCanceled( CompletionSource<TResult> c )
            {
                _c = c;
                Result = default!;
                Called = false;
                ResultCanceled = false;
            }


            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource{TResult}.TrySetCanceled()"/> has been called)
            /// a successful instead of a cancellation completion.
            /// <para>
            /// Note that the <see cref="CompletionSource{TResult}.HasBeenCanceled"/> will be true: the fact that the command
            /// has been canceled is available.
            /// </para>
            /// </summary>
            public void SetResult( TResult result )
            {
                if( Called ) CompletionSource.OnCanceled.ThrowMustBeCalledOnlyOnce();
                Called = true;
                Result = result;
            }

            /// <summary>
            /// Sets (or tries to set if <see cref="CompletionSource{TResult}.TrySetCanceled()"/> has been called)
            /// the cancellation completion.
            /// The default <see cref="ICompletable{TResult}"/> OnCanceled method calls this method.
            /// <para>
            /// Note that the <see cref="CompletionSource{TResult}.HasBeenCanceled"/> will be true: the fact that the command
            /// has been canceled is available.
            /// </para>
            /// </summary>
            public void SetCanceled()
            {
                if( Called ) CompletionSource.OnCanceled.ThrowMustBeCalledOnlyOnce();
                Called = true;
                ResultCanceled = true;
            }
        }

        /// <inheritdoc />
        public void SetCanceled()
        {
            if( _state != 0 ) _tcs.SetCanceled();
            var o = new OnCanceled( this );
            _holder.OnCanceled( ref o );
            if( !o.Called ) CompletionSource.ThrowOnCancelCalledRequired();
            _state |= 4;
            if( o.ResultCanceled )
            {
                _tcs.SetCanceled();
            }
            else
            {
                _tcs.SetResult( o.Result );
            }
        }

        /// <inheritdoc />
        public bool TrySetCanceled()
        {
            if( _state != 0 ) return false;
            var o = new OnCanceled( this );
            _holder.OnCanceled( ref o );
            if( !o.Called ) CompletionSource.ThrowOnCancelCalledRequired();
            _state |= 4;
            if( o.ResultCanceled )
            {
                if( !_tcs.TrySetCanceled() )
                {
                    _state &= ~4;
                    return false;
                }
            }
            else
            {
                if( !_tcs.TrySetResult( o.Result ) )
                {
                    _state &= ~4;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Overridden to return the current completion status.
        /// </summary>
        /// <returns>The current status.</returns>
        public override string ToString() => CompletionSource.GetStatus( _tcs.Task.Status, _state );

    }
}
