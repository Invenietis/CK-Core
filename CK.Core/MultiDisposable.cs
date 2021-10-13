using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Hold multiples <see cref="IDisposable"/> class.
    /// </summary>
    public class MultiDisposable : IDisposable
    {
        readonly IDisposable?[] _disposables;

        /// <summary>
        /// Instantiate a <see cref="MultiDisposable"/>.
        /// </summary>
        /// <param name="disposables">The class to dispose. Will be diposed in the same order.</param>
        public MultiDisposable( params IDisposable?[] disposables )
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
