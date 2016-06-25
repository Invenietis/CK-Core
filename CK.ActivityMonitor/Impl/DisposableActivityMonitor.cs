#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\DisposableActivityMonitor.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Impl
{
    /// <summary>
    /// Trivial implementation of <see cref="IDisposableActivityMonitor"/> that respects the disposable 
    /// pattern (to support potential unmanaged resources).
    /// <see cref="Dispose()"/> simply closes all opened groups.
    /// </summary>
    public class DisposableActivityMonitor : ActivityMonitor, IDisposableActivityMonitor
    {
        bool _disposed;

        /// <summary>
        /// Ensures that potential unmanaged resources are correctly released by calling <see cref="Dispose(bool)"/>
        /// with false.
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
        /// <param name="disposing">True when <see cref="Dispose()"/> is called, false when called from the Garbage collector.</param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                while( CurrentGroup != null ) CloseGroup( this.NextLogTime() );
            }
        }
    }
}
