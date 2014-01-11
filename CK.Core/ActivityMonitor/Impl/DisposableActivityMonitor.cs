﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Impl
{
    /// <summary>
    /// Trivial implementation of <see cref="IDisposableActivityMonitor"/>.
    /// <see cref="Dispose()"/> simply closes all opened groups.
    /// </summary>
    public class DisposableActivityMonitor : ActivityMonitor, IDisposableActivityMonitor
    {
        bool _disposed;

        /// <summary>
        /// Ensures that potential unmanaged resources are correctly released by calling <see cref="Dispose(bool)"/> with false.
        /// </summary>
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
            if( !_disposed )
            {
                Dispose( true );
                GC.SuppressFinalize( this );
                _disposed = true;
            }
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
                while( CurrentGroup != null ) CloseGroup( this.NextLogTime() );
            }
        }
    }
}