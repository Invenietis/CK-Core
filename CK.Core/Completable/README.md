# Completable & Completion

These "futures" or "yet another promises" are based on Task/TaskCompletionSource. They extend the capacities of the
Tasks by bringing:

- Covariance of the completion result.
- Optional extension points to map errors (exceptions or cancellation) to regular results.

The read only and covariant model are the 2 interfaces [ICompletion](ICompletion.cs) (no result) and
[ICompletion&lt;out TResult&gt; : ICompletion](ICompletionT.cs).

The real stuff are the sources of completion: [CompletionSource](CompletionSource.cs)
and [CompletionSource&lt;TResult&gt;](CompletionSourceT.cs).

A [Completable](ICompletable.cs) is not, in itself, the important object here (note that [ICompletable<TResult>](ICompletableT.cs)
is not covariant). A completable simply holds a completion and carries the OnError/OnCanceled extension points,
it is more to be used as an implementation detail than to expose it.


