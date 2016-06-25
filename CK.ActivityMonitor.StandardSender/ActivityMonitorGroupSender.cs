#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorGroupSender.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace CK.Core
{
    internal class ActivityMonitorGroupSender : ActivityMonitorGroupData, IActivityMonitorGroupSender
    {
        internal readonly IActivityMonitor Monitor;

        /// <summary>
        /// Gets whether the log has been rejected.
        /// </summary>
        public bool IsRejected => Level == LogLevel.None;

        /// <summary>
        /// Used only by filtering extension methods (level is always filtered).
        /// </summary>
        internal ActivityMonitorGroupSender( IActivityMonitor monitor, LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
            Debug.Assert( monitor != null );
            Debug.Assert( ((level & LogLevel.IsFiltered) != 0 && MaskedLevel != LogLevel.None),
                "The level is already filtered and not None or we are initializing the monitor's FakeLineSender." );
            Monitor = monitor;
        }

        /// <summary>
        /// Used only to initialize a ActivityMonitorGroupSender for rejected opened group.
        /// </summary>
        internal ActivityMonitorGroupSender( IActivityMonitor monitor )
        {
            Debug.Assert( monitor != null );
            Monitor = monitor;
        }

        internal IDisposableGroup InitializeAndSend( Exception exception, CKTrait tags, string text )
        {
            Debug.Assert( !IsRejected );
            Initialize( text, exception, tags, Monitor.NextLogTime() );
            return Monitor.UnfilteredOpenGroup( this );
        }

    }
}
