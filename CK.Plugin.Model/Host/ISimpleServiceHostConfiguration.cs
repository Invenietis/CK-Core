#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\ISimpleServiceHostConfiguration.cs) is part of CiviKey. 
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
using System.Reflection;

namespace CK.Plugin
{
    /// <summary>
    /// Extension of the basic <see cref="IServiceHostConfiguration"/> that 
    /// memorizes its configuration and provides helpers to set multiple configurations at once.
    /// </summary>
    public interface ISimpleServiceHostConfiguration : IServiceHostConfiguration
    {
        void Clear();
        void SetConfiguration( EventInfo e, ServiceLogEventOptions option );
        void SetConfiguration( MethodInfo m, ServiceLogMethodOptions option );
        void SetConfiguration( PropertyInfo p, ServiceLogMethodOptions option );
        void SetAllEventsConfiguration( Type type, ServiceLogEventOptions option );
        void SetAllMethodsConfiguration( Type type, ServiceLogMethodOptions option );
        void SetAllPropertiesConfiguration( Type type, ServiceLogMethodOptions option );
        void SetMethodGroupConfiguration( Type type, string methodName, ServiceLogMethodOptions option );
    }
}
