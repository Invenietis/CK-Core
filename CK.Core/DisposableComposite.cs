using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Hold multiples <see cref="IDisposable"/> classes.
    /// </summary>
    public class DisposableComposite : IDisposable
    {
        readonly IDisposable?[] _disposables;

        /// <summary>
        /// Instantiate a <see cref="DisposableComposite"/>.
        /// </summary>
        /// <param name="disposables">The classes to dispose. Will be diposed in the same order.</param>
        public DisposableComposite( params IDisposable?[] disposables )
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach( IDisposable? disposable in _disposables )
            {
                disposable?.Dispose();
            }
        }
    }
}
