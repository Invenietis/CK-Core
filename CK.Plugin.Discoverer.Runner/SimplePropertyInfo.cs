#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\SimplePropertyInfo.cs) is part of CiviKey. 
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

namespace CK.Plugin.Discoverer.Runner
{
    /// <summary>
    ///  Contains property's basic info.
    /// </summary>
    [Serializable]
    public class SimplePropertyInfo : IComparable<SimplePropertyInfo>
    {
        string _name;
        string _propertyType;

        public string Name { get { return _name; } }

        public string PropertyType { get { return _propertyType; } }

        public SimplePropertyInfo(string name, string propertyType)
        {
            _name = name;
            _propertyType = propertyType;
        }

        public int CompareTo( SimplePropertyInfo other )
        {
            if( this == other ) return 0;
            return _name.CompareTo( other.Name );
        }
    } 
}
