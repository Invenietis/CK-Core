#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\RouteConfigurationLockShell.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.RouteConfig.Impl
{
    class RouteConfigurationLockShell : IRouteConfigurationLock
    {
        readonly CountdownEvent _lock;
        bool _closed;

        internal RouteConfigurationLockShell( CountdownEvent l )
        {
            _lock = l;
            _closed = true;
        }

        internal void Open()
        {
            Debug.Assert( _closed == true );
            _closed = false;
        }

        public void Lock()
        {
            if( _closed ) throw new InvalidOperationException( "RouteConfigurationLock must be used only when routes are ready." );
            _lock.AddCount();
        }

        public void Unlock()
        {
            if( _closed ) throw new InvalidOperationException( "RouteConfigurationLock must be used only when routes are ready." );
            _lock.Signal();
        }
    }
}
