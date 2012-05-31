#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\Log\ILogCenter.cs) is part of CiviKey. 
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
using System.Reflection;
using CK.Core;

namespace CK.Plugin
{

    /// <summary>
    /// Centralized management of log events.
    /// Even if these events are designed for <see cref="IServiceHost"/> behavior, 
    /// the <see cref="ExternalLog"/> and <see cref="ExternalLogError"/> methods 
    /// enable injection of external events into the pipe. 
    /// </summary>
    public interface ILogCenter
    {
        /// <summary>
        /// Fires when a <see cref="LogHostEventArgs"/> is beeing created.
        /// This event is "opened": it will be closed when the <see cref="EventCreated"/> fires.
        /// </summary>
        event EventHandler<LogEventArgs> EventCreating;

        /// <summary>
        /// Fires for each <see cref="LogHostEventArgs"/>.
        /// </summary>
        event EventHandler<LogEventArgs> EventCreated;

        /// <summary>
        /// Generates a <see cref="ILogExternalEntry"/> event log.
        /// </summary>
        /// <param name="message">Event message. Should be localized if possible.</param>
        /// <param name="extraData">Optional extra data associated to the event.</param>
        void ExternalLog( string message, object extraData );

        /// <summary>
        /// Generates a <see cref="ILogExternalErrorEntry"/> event log.
        /// </summary>
        /// <param name="e">The <see cref="Exception"/>. When null, a warning is added to the message.</param>
        /// <param name="optionalExplicitCulprit">
        /// Optional <see cref="MemberInfo"/> that designates a culprit. 
        /// Nullable: when not specified, the <see cref="Exception.TargetSite"/> is used.
        /// </param>
        /// <param name="message">Optional event message (localized if possible). Nullable.</param>
        /// <param name="extraData">Optional extra data associated to the event. Nullable.</param>
        void ExternalLogError( Exception e, MemberInfo optionalExplicitCulprit, string message, object extraData );

        /// <summary>
        /// Gets the list of errors that occured while there was no launched plugins to process them.
        /// </summary>
        IReadOnlyList<ILogErrorCaught> UntrackedErrors { get; }

    }
}
