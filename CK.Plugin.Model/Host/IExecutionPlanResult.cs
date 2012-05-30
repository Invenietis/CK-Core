#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\IExecutionPlanResult.cs) is part of CiviKey. 
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
using CK.Core;

namespace CK.Plugin
{

    /// <summary>
    /// Qualifies the type of error during plugin management.
    /// </summary>
    public enum ExecutionPlanResultStatus
    {
        /// <summary>
        /// No error.
        /// </summary>
        Success = 0,
        
        /// <summary>
        /// An error occured while loading (activating) the plugin.
        /// </summary>
        LoadError = 1,
        
        /// <summary>
        /// An error occured during the call to <see cref="IPlugin.Setup"/>.
        /// </summary>
        SetupError = 2,

        /// <summary>
        /// An error occured during the call to <see cref="IPlugin.Start"/>.
        /// </summary>
        StartError = 3

    }

    /// <summary>
    /// Defines the return of the <see cref="IPluginHost.Execute"/> method.
    /// </summary>
    public interface IExecutionPlanResult
    {
        /// <summary>
        /// Kind of error.
        /// </summary>
        ExecutionPlanResultStatus Status { get; }
        
        /// <summary>
        /// The plugin that raised the error.
        /// </summary>
        IPluginInfo Culprit { get; }

        /// <summary>
        /// Detailed error information specific to the <see cref="IPlugin.Setup"/> phasis.
        /// </summary>
        IPluginSetupInfo SetupInfo { get; }

        /// <summary>
        /// Gets the exception if it exists (note that a <see cref="IPlugin.Setup"/> may not throw exception but simply 
        /// returns false).
        /// </summary>
        Exception Error { get; }
    }
}
