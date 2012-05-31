#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\ServiceRefInfo.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Plugin.Discoverer.Runner
{
    internal sealed class ServiceRefInfo
    {
        public readonly bool IsIDynamicService;
        
        public readonly bool IsUnknownGenericInterface;

        public readonly bool IsIServiceWrapped;

        public readonly ServiceInfo Reference;

        public ServiceRefInfo( ServiceInfo r, bool isIServiceWrapped, bool isGeneric, bool isIDynamicService )
        {
            Debug.Assert( !isIServiceWrapped || isIDynamicService, "IService<T> ==> T : IDynamicService" );
            Reference = r;
            IsIServiceWrapped = isIServiceWrapped;
            IsUnknownGenericInterface = isGeneric && !isIServiceWrapped;
            IsIDynamicService = isIDynamicService;
        }
    }
}
