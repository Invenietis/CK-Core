#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer.Runner\SimpleMethodInfo.cs) is part of CiviKey. 
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

namespace CK.Plugin.Discoverer.Runner
{
    /// <summary>
    ///  Contains a method's basic info
    /// </summary>
    [Serializable]
    public class SimpleMethodInfo : IComparable<SimpleMethodInfo>
    {
        List<SimpleParameterInfo> _parameters;

        string _returnType;
        string _name;

        public string ReturnType { get { return _returnType; } }
        public string Name { get { return _name; } }

        public IList<SimpleParameterInfo> Parameters { get { return _parameters; } }
        
        public SimpleMethodInfo(string name, string returnType)
        {
            _parameters = new List<SimpleParameterInfo>();
            _name = name;
            _returnType = returnType;
        }

        public SimpleMethodInfo Clone()
        {
            SimpleMethodInfo copiedMethod = new SimpleMethodInfo(this.Name,this.ReturnType);
            foreach (SimpleParameterInfo p in this.Parameters)
            {
                copiedMethod.Parameters.Add(new SimpleParameterInfo(p.ParameterName, p.ParameterType));
            }

            return copiedMethod;
        }

        public string GetSimpleSignature()
        {
            StringBuilder b = new StringBuilder();
            b.Append(ReturnType).Append(' ').Append(Name).Append('(');
            foreach (var p in Parameters) b.Append(p.ParameterType).Append(',');
            b.Length = b.Length - 1;
            b.Append(')');
            return b.ToString();
        }

        public int CompareTo( SimpleMethodInfo other )
        {
            if( this == other ) return 0;
            int cmp = this.GetSimpleSignature().CompareTo( other.GetSimpleSignature() );
            return cmp;
        }

     }
}
