#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\IProxyDefinition.cs) is part of CiviKey. 
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
using System.Text;
using System.Reflection;

namespace CK.Plugin.Hosting
{
	internal class ProxyOptions
	{
        /// <summary>
        /// The proxified method traps any exception and routes it to <see cref="ServiceProxyBase.OnCallException"/>
        /// (or <see cref="ServiceProxyBase.OnEventHandlingException"/> during events raising).
        /// </summary>
        public bool CatchExceptions;

        public enum CheckStatus
        {
            None,
            NotDisabled,
            Running
        }

        /// <summary>
        /// The proxified method will call <see cref="ServiceProxyBase.GetLoggerForRunningCall"/>, <see cref="ServiceProxyBase.GetLoggerForNotDisabledCall"/> 
        /// or <see cref="ServiceProxyBase.GetLoggerForAnyCall"/> (or the 3 similar ones for events).
        /// </summary>
        public CheckStatus RuntimeCheckStatus;
	}



	/// <summary>
	/// Defines the way a service proxy must be generated. 
	/// </summary>
	internal interface IProxyDefinition
	{
		/// <summary>
		/// Gets the interface type that must be proxified.
		/// </summary>
		Type TypeInterface { get; }

		/// <summary>
		/// Gets a type that must be <see cref="ServiceProxyBase"/> or a specialisation of it.
		/// </summary>
		Type ProxyBase { get; }

        /// <summary>
        /// Gets whether the interface is a <see cref="IDynamicService"/> or not.
        /// </summary>
        bool IsDynamicService { get; }

		/// <summary>
		/// Gets for the given event, the options that drives code generation of the raising method.
		/// </summary>
        /// <param name="p">The event.</param>
		/// <returns>A set of options for the proxy.</returns>
        ProxyOptions GetEventOptions( EventInfo p );
        
		/// <summary>
		/// Gets for the given method property (getter or setter), the options that drives code generation of the method proxy.
		/// </summary>
        /// <param name="p">The property.</param>
		/// <param name="m">The method (can be the getter or the setter).</param>
		/// <returns>A set of options for the proxy.</returns>
        ProxyOptions GetPropertyMethodOptions( PropertyInfo p, MethodInfo m );

		/// <summary>
		/// Gets for the given method, the options that drives code generation of the method proxy.
		/// </summary>
		/// <param name="m">The method.</param>
		/// <returns>A set of options for the proxy.</returns>
        ProxyOptions GetMethodOptions( MethodInfo m );
	}

}
