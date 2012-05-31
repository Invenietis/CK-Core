#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\IContext.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using CK.Plugin;
using CK.Storage;
using CK.Core;
using CK.Plugin.Hosting;
using Common.Logging;

namespace CK.Context
{
    public interface IContext : IServiceProvider
    {
        /// <summary>
        /// Gets the <see cref="IConfigManager"/> that can be used to read and save the configuration.
        /// </summary>
        IConfigManager ConfigManager { get; }

        /// <summary>
        /// Gets or sets a fallback <see cref="IServiceProvider"/> that will be queried for a service
        /// if it is not found at this level.
        /// </summary>
        IServiceProvider BaseServiceProvider { get; set; }

        /// <summary>
        /// Gets the service container for this context.
        /// </summary>
        ISimpleServiceContainer ServiceContainer { get; }

        /// <summary>
        /// Gets the <see cref="RequirementLayer"/> of this context.
        /// </summary>
        RequirementLayer RequirementLayer { get; }

        /// <summary>
        /// Gets the <see cref="ISimplePluginRunner"/>
        /// </summary>
        ISimplePluginRunner PluginRunner { get; }

        /// <summary>
        /// Gets the <see cref="ILogCenter"/>
        /// </summary>
        ILogCenter LogCenter { get; }

        /// <summary>
        /// Writes this <see cref="IContext">context</see> as in a stream.
        /// </summary>
        /// <param name="stream">Stream to the saved document.</param>
        void SaveContext( IStructuredWriter writer );

        /// <summary>
        /// Loads this <see cref="IContext">context</see> from a file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>A list (possibly empty) of <see cref="ISimpleErrorMessage"/> describing read errors.</returns>
        IReadOnlyList<ISimpleErrorMessage> LoadContext( IStructuredReader reader );

        /// <summary>
        /// Fired by <see cref="RaiseExitApplication"/> to signal the end of the application (this is a cancelable event). 
        /// If it is not cancelled, runner is disabled and then <see cref="ApplicationExited"/> event is fired.
        /// </summary>
        event EventHandler<ApplicationExitingEventArgs> ApplicationExiting;

        /// <summary>
        /// Fired by <see cref="RaiseExitApplication"/> to signal the very end of the application. 
        /// Once this event has fired, this <see cref="IContext"/> is no more functionnal.
        /// </summary>
        event EventHandler<ApplicationExitedEventArgs> ApplicationExited;

        /// <summary>
        /// Raises the <see cref="ApplicationExiting"/> (any persistence of information/configuration should be done during this phasis), 
        /// and <see cref="ApplicationExited"/> event.
        /// </summary>
        /// <param name="hostShouldExit">When true, the application host should exit: this is typically used by a plugin to
        /// trigger the end of the current application (ie. the "Exit" button). 
        /// A host would better use false to warn any services and plugins to do what they have to do before leaving and manage to exit the way it wants.</param>
        bool RaiseExitApplication( bool hostShouldExit );
    }
}
