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
        CKExceptionData? _exception;
        byte _state;

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
        public CKExceptionData? Exception
        {
            get
            {
                if( _exception == null && _tcs.Task.IsFaulted )
                {
                    _exception = CKExceptionData.CreateFrom( _tcs.Task.Exception );
                }
                return _exception;
            }
        }

        /// <inheritdoc />
        public Task<TResult> Task => _tcs.Task;

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
            _tcs.SetResult( result );
            _state |= 1;
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
            if( _tcs.TrySetResult( result ) )
            {
                _state |= 1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enables <see cref="ICompletable{TResult}"/> OnError to
        /// transform exceptions into successful or canceled completion.
        /// </summary>
        public ref struct OnError
        {
            readonly CompletionSource<TResult> _c;
            internal bool Try;
            internal bool Called;

            internal OnError( CompletionSource<TResult> c, bool t )
            {
                _c = c;
                Try = t;
                Called = false;
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
                if( Try )
                {
                    Try = _c._tcs.TrySetException( ex );
                }
                else
                {
                    _c._tcs.SetException( ex );
                }
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
                if( Try )
                {
                    Try = _c._tcs.TrySetResult( result );
                }
                else
                {
                    _c._tcs.SetResult( result );
                }
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
                if( Try )
                {
                    Try = _c._tcs.TrySetCanceled();
                }
                else
                {
                    _c._tcs.SetCanceled();
                }
            }

        }

        /// <inheritdoc />
        public void SetException( Exception exception )
        {
            var o = new OnError( this, false );
            _holder.OnError( exception, ref o );
            if( !o.Called ) CompletionSource.ThrowOnErrorCalledRequired();
            _state |= 2;
            if( !_tcs.Task.IsFaulted )
            {
                _exception = CKExceptionData.CreateFrom( exception );
            }
        }

        /// <inheritdoc />
        public bool TrySetException( Exception exception )
        {
            var o = new OnError( this, true );
            _holder.OnError( exception, ref o );
            if( !o.Called ) CompletionSource.ThrowOnErrorCalledRequired();
            if( o.Try )
            {
                _state |= 2;
                if( !_tcs.Task.IsFaulted )
                {
                    _exception = CKExceptionData.CreateFrom( exception );
                }
            }
            return o.Try;
        }

        /// <summary>
        /// Enables <see cref="ICompletable{TResult}"/> OnCanceled method to
        /// transform exceptions into successful or canceled completion.
        /// </summary>
        public ref struct OnCanceled
        {
            readonly CompletionSource<TResult> _c;
            internal bool Try;
            internal bool Called;

            internal OnCanceled( CompletionSource<TResult> c, bool t )
            {
                _c = c;
                Try = t;
                Called = false;
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
                if( Try )
                {
                    Try = _c._tcs.TrySetResult( result );
                }
                else
                {
                    _c._tcs.SetResult( result );
                }
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
                if( Try )
                {
                    Try = _c._tcs.TrySetCanceled();
                }
                else
                {
                    _c._tcs.SetCanceled();
                }
            }
        }

        /// <inheritdoc />
        public void SetCanceled()
        {
            var o = new OnCanceled( this, false );
            _holder.OnCanceled( ref o );
            if( !o.Called ) CompletionSource.ThrowOnCancelCalledRequired();
            _state |= 4;
        }

        /// <inheritdoc />
        public bool TrySetCanceled()
        {
            var o = new OnCanceled( this, true );
            _holder.OnCanceled( ref o );
            if( !o.Called ) CompletionSource.ThrowOnCancelCalledRequired();
            if( o.Try ) _state |= 4;
            return o.Try;
        }

        /// <summary>
        /// Overridden to return the current completion status.
        /// </summary>
        /// <returns>The current status.</returns>
        public override string ToString() => CompletionSource.GetStatus( _tcs.Task.Status, _state );

    }
}
