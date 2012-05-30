#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\MuxActivityLoggerClientDemux.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

namespace CK.Core
{
    /// <summary>
    /// Abstract base class for <see cref="IMuxActivityLoggerClient"/> that routes
    /// multiplexed log data back to simple <see cref="IActivityLoggerClient"/> specific 
    /// to each <see cref="IActivityLogger"/> sender.
    /// </summary>
    public abstract class MuxActivityLoggerClientDemux : IMuxActivityLoggerClient
    {
        IActivityLogger _lastSender;
        IActivityLoggerClient _lastClient;
        ListDictionary _clients;

        IActivityLoggerClient FindOrCreate( IActivityLogger sender )
        {
            if( _lastSender == sender ) return _lastClient;
            if( _lastSender == null )
            {
                _lastClient = CreateClient( sender );
            }
            else
            {
                if( _clients == null )
                {
                    _clients = new ListDictionary();
                    _clients.Add( _lastSender, _lastClient );
                    _lastClient = null;
                }
                else _lastClient = (IActivityLoggerClient)_clients[sender];
                if( _lastClient == null )
                {
                    _lastClient = CreateClient( sender );
                    _clients.Add( sender, _lastClient );
                }
            }
            _lastSender = sender;
            return _lastClient;
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="current">Current level filter.</param>
        /// <param name="newValue">The new level filter.</param>
        void IMuxActivityLoggerClient.OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue )
        {
            FindOrCreate( sender ).OnFilterChanged( current, newValue );
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having 
        /// called <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log (never null).</param>
        void IMuxActivityLoggerClient.OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text )
        {
            FindOrCreate( sender ).OnUnfilteredLog( level, text );
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        void IMuxActivityLoggerClient.OnOpenGroup( IActivityLogger sender, IActivityLogGroup group )
        {
            FindOrCreate( sender ).OnOpenGroup( group );
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The group that will be closed.</param>
        /// <param name="conclusions">Mutable conclusions associated to the closing group.</param>
        void IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            FindOrCreate( sender ).OnGroupClosing( group, conclusions );
        }


        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The group that will be closed.</param>
        /// <param name="conclusions">Texts that conclude the closed group.</param>
        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            FindOrCreate( sender ).OnGroupClosed( group, conclusions );
        }

        /// <summary>
        /// Must be overriden to create a new <see cref="IActivityLoggerClient"/> for the given <see cref="IActivityLogger"/>.
        /// </summary>
        /// <param name="logger">The new sender for which a <see cref="IActivityLoggerClient"/> must be created.</param>
        /// <returns>A new concrete <see cref="IActivityLoggerClient"/> bound to the given logger.</returns>
        protected abstract IActivityLoggerClient CreateClient( IActivityLogger logger );
    }
}
