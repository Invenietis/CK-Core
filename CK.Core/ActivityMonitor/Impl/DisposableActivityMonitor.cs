using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Impl
{
    /// <summary>
    /// Trivial implementation of <see cref="IDisposableActivityMonitor"/>.
    /// <see cref="Dispose"/> simply closes all opened groups.
    /// </summary>
    public class DisposableActivityMonitor : ActivityMonitor, IDisposableActivityMonitor
    {
        bool _disposed;

        ~DisposableActivityMonitor()
        {
            Dispose( false );
        }

        /// <summary>
        /// Automatically close any opened groups.
        /// Can be called multiple times.
        /// </summary>
        public void Dispose()
        {
            if( !_disposed ) Dispose( true );
        }
        /// <summary>
        /// Automatically close any opened groups.
        /// Can be called multiple times.
        /// </summary>
        /// <param name="disposing">Whether <see cref="Dispose()"/> is called.</param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                _disposed = true;
                GC.SuppressFinalize( this );
                while( CurrentGroup != null ) CloseGroup( DateTime.UtcNow );
            }
        }
    }
}
