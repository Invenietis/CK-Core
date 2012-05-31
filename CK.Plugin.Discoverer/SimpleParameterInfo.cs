#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\SimpleParameterInfo.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Contains a parameter's basic info
    /// </summary>
    [Serializable]
    public class SimpleParameterInfo : ISimpleParameterInfo, IComparable<SimpleParameterInfo>
    {
        string _parameterName;
        string _parameterType;

        public string ParameterName { get { return _parameterName; } }
        public string ParameterType { get { return _parameterType; } }

        public SimpleParameterInfo()
        {            
        }

        internal void Initialize( Discoverer.Runner.SimpleParameterInfo rP )
        {
            _parameterType = rP.ParameterType;
            _parameterName = rP.ParameterName;
        }

        internal bool Merge( Discoverer.Runner.SimpleParameterInfo rP )
        {
            Debug.Assert( rP.ParameterType == _parameterType );
            if( rP.ParameterName != _parameterName )
            {
                _parameterName = rP.ParameterName;
                return true;
            }
            return false;
        }

        public int CompareTo( SimpleParameterInfo other )
        {
            if (this == other) return 0;
            int cmp = _parameterName.CompareTo(other.ParameterName);
            if (cmp == 0) cmp = _parameterType.CompareTo(other.ParameterType);
            return cmp;
        }

    }
}
