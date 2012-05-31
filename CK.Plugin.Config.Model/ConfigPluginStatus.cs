#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\ConfigPluginStatus.cs) is part of CiviKey. 
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

namespace CK.Plugin.Config
{
	/// <summary>
	/// Defines the configured status of a plugin. This mimics the windows service behavior... It was not planned: it appears
	/// that this is the best/simplest way to handle plugins that Starts/Stops with dependencies among them.
	/// <remarks>
	/// Note that our dependencies in CK do not define a Direct Acyclic Graph like Windows Services: plugins are free
	/// to reference services provided by others in any way they like. 
	/// Even if the running state of a plugin is more a "declared state" in a cooperative 
	/// environment than a true process/thread state in a preemptive multi-task system, the possible cyclic references 
	/// forces us to design a "2 phase starting" API (and the corresponding status Starting and Stopping).
    /// See <see cref="RunningStatus"/>.
	/// </remarks>
	/// </summary>
    /// <seealso cref="RunningStatus"/>
	[Flags]
	public enum ConfigPluginStatus
	{
		/// <summary>
		/// The plugin is disabled. It will refuse to start and will not be loaded at startup.
        /// This is not the default: <see cref="Manual"/> is the default for a plugin.
		/// </summary>
		Disabled = 0,
		
		/// <summary>
		/// The plugin does not start by default but will automatically attempt to start
		/// if another plugin requires it to run or if the user starts it explicitely.
        /// This is the default.
		/// </summary>
		Manual = 1,
		
		/// <summary>
		/// The plugin will be started as soon as possible.
		/// If the plugin can not be started at the beginning of the application, it will
		/// automatically be started when required conditions are met.
		/// </summary>
		AutomaticStart = 2,

        /// <summary>
        /// This mask covers normal configuration status.
        /// </summary>
        ConfigurationMask = Manual | AutomaticStart,
	}
}
