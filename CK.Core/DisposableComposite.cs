using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Hold multiples <see cref="IDisposable"/> and dispose them at once.
    /// </summary>
    public readonly struct DisposableComposite : IDisposable
    {
        readonly IDisposable?[] _disposables;

        /// <summary>
        /// Instantiate a <see cref="DisposableComposite"/>.
        /// </summary>
        /// <param name="disposables">The disposables to dispose. Will be disposed in the same order.</param>
        public DisposableComposite( params IDisposable?[] disposables )
        {
            _disposables = disposables;
        }

        /// <summary>
        /// Disposes all the registered disposable.
        /// </summary>
        public void Dispose()
        {
            foreach( IDisposable? disposable in _disposables )
            {
                disposable?.Dispose();
            }
        }
    }
}
